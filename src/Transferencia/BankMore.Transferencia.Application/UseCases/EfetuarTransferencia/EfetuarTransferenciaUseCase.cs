using BankMore.Transferencia.Application.Clients.ContaCorrente;
using BankMore.Transferencia.Application.Clients.ContaCorrente.Dto;
using BankMore.Transferencia.Application.Common;
using BankMore.Transferencia.Application.Repositories;
using BankMore.Transferencia.Application.Service;

namespace BankMore.Transferencia.Application.UseCases.EfetuarTransferencia
{
    public class EfetuarTransferenciaUseCase(
        IIdempotenciaService idempotenciaService, 
        IContaCorrenteClient contaCorrenteClient, 
        ITransferenciaRepository transferenciaRepository) : IEfetuarTransferenciaUseCase
    {
        public async Task<ResultadoOperacao<object>> EfetuarTransferencia(string idContaOrigem, EfetuarTransferenciaRequest request, string token)
        {
            if (request.Valor <= 0)
                return ResultadoOperacao<object>.Falha("INVALID_VALUE", "O valor deve ser maior que zero.");

            // Validar conta de origem
            var contaOrigemInfo = await contaCorrenteClient.ObterPorIdAsync(idContaOrigem, token);
            if (contaOrigemInfo == null)
                return ResultadoOperacao<object>.Falha("INVALID_ACCOUNT", "Conta de origem inválida.");
            if (!contaOrigemInfo.Ativo)
                return ResultadoOperacao<object>.Falha("INACTIVE_ACCOUNT", "Conta de origem inativa.");

            // Validar conta destino
            var contaDestinoInfo = await contaCorrenteClient.ObterPorNumeroAsync(request.NumeroContaDestino, token);
            if (contaDestinoInfo == null)
                return ResultadoOperacao<object>.Falha("INVALID_ACCOUNT", "Conta de destino inválida.");
            if (!contaDestinoInfo.Ativo)
                return ResultadoOperacao<object>.Falha("INACTIVE_ACCOUNT", "Conta de destino inativa.");

            return await idempotenciaService.ObterOuRegistrarAsync(
                request,
                async () =>
                {
                    // Efetuar débito na conta de origem
                    await contaCorrenteClient.MovimentarAsync(new MovimentacaoRequest
                    {
                        IdContaCorrente = contaOrigemInfo.IdContaCorrente,
                        IdRequisicao = request.IdRequisicao,
                        Valor = request.Valor,
                        TipoMovimento = "D"
                    }, token);

                    try
                    {
                        // Efetuar crédito na conta de destino
                        await contaCorrenteClient.MovimentarAsync(new MovimentacaoRequest
                        {
                            IdContaCorrente = contaDestinoInfo.IdContaCorrente,
                            IdRequisicao = request.IdRequisicao,
                            Valor = request.Valor,
                            TipoMovimento = "C"
                        }, token);                        
                    }
                    catch (Exception ex)
                    {
                        // Estorno em caso de erro
                        await contaCorrenteClient.MovimentarAsync(new MovimentacaoRequest
                        {
                            IdContaCorrente = contaOrigemInfo.IdContaCorrente,
                            IdRequisicao = request.IdRequisicao,
                            Valor = request.Valor,
                            TipoMovimento = "C"
                        }, token);

                        return ResultadoOperacao<object>.Falha("TRANSFER_FAILED", $"Erro na transferência: {ex.Message}. Estorno realizado.");
                    }

                    // Persistir transferência
                    var transferencia = Domain.Entities.Transferencia.Criar(
                        contaOrigemInfo.IdContaCorrente,
                        contaDestinoInfo.IdContaCorrente,
                        request.Valor
                    );

                    await transferenciaRepository.AdicionarAsync(transferencia);

                    return ResultadoOperacao<object>.SucessoResultado(null!);
                }
            );
        }
    }
}
