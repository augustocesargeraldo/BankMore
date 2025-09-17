using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Application.Services;

namespace BankMore.ContaCorrente.Application.UseCases.CriarContaCorrente
{
    public class CriarContaCorrenteUseCase(IIdempotenciaService idempotenciaService, IContaCorrenteRepository contaCorrenteRepository) : ICriarContaCorrenteUseCase
    {
        public async Task<ResultadoOperacao<CriarContaCorrenteResponse>> ExecutarAsync(CriarContaCorrenteRequest request)
        {
            return await idempotenciaService.ObterOuRegistrarAsync(
                request,
                async () =>
                {
                    // Verifica se o número da conta já existe
                    var existente = await contaCorrenteRepository.ObterPorNumeroAsync(request.Numero);
                    if (existente != null)
                    {
                        return ResultadoOperacao<CriarContaCorrenteResponse>.Falha(
                            "ACCOUNT_NUMBER_CONFLICT",
                            [$"O número da conta {request.Numero} já está em uso."]
                        );
                    }

                    var conta = Domain.Entities.ContaCorrente.Criar(request.Numero, request.Nome, request.Senha);

                    // Persiste no banco
                    await contaCorrenteRepository.AdicionarAsync(conta);

                    // Retorna o response
                    return ResultadoOperacao<CriarContaCorrenteResponse>.SucessoResultado(new CriarContaCorrenteResponse
                    {
                        Numero = conta.Numero
                    });
                });
        }
    }
}
