using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Application.Services;

namespace BankMore.ContaCorrente.Application.UseCases.MovimentacaoContaCorrente
{
    public class MovimentacaoContaCorrenteUseCase(
        IIdempotenciaService idempotenciaService,
        IContaCorrenteRepository contaCorrenteRepository,
        IMovimentoRepository movimentoRepository) : IMovimentacaoContaCorrenteUseCase
    {
        public async Task<ResultadoOperacao<object>> ExecutarAsync(MovimentacaoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdContaCorrente))
                return ResultadoOperacao<object>.Falha(
                    "USER_UNAUTHORIZED",
                    ["Token inválido ou expirado."]
                );

            var conta = request.NumeroConta.HasValue ? await contaCorrenteRepository.ObterPorNumeroAsync(request.NumeroConta.Value) : await contaCorrenteRepository.ObterPorIdAsync(request.IdContaCorrente);

            if (conta == null)
                return ResultadoOperacao<object>.Falha(
                    "INVALID_ACCOUNT",
                    ["Conta corrente não cadastrada."]
                );

            if (!conta.Ativo)
                return ResultadoOperacao<object>.Falha(
                    "INACTIVE_ACCOUNT",
                    ["Conta corrente inativa."]
                );

            if (request.Valor <= 0)
                return ResultadoOperacao<object>.Falha(
                    "INVALID_VALUE",
                    ["Valor deve ser positivo."]
                );

            if (request.TipoMovimento != "C" && request.TipoMovimento != "D")
                return ResultadoOperacao<object>.Falha(
                    "INVALID_TYPE",
                    ["Tipo de movimentação inválido."]
                );

            if (request.TipoMovimento == "D" && conta.IdContaCorrente != request.IdContaCorrente)
            {
                return ResultadoOperacao<object>.Falha(
                    "INVALID_TYPE",
                    ["Não é permitido débito em conta de outro usuário."]
                );
            }

            var movimento = Domain.Entities.Movimento.Criar(conta.IdContaCorrente, 
                request.TipoMovimento, 
                request.Valor);

            return await idempotenciaService.ObterOuRegistrarAsync(
                request,
                async () =>
                {
                    // Persistir movimento
                    await movimentoRepository.AdicionarAsync(movimento);
                    return ResultadoOperacao<object>.SucessoResultado(null!);
                }
            );            
        }
    }
}
