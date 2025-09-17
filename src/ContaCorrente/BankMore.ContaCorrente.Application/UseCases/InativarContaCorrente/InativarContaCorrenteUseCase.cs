using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Application.Services;

namespace BankMore.ContaCorrente.Application.UseCases.InativarContaCorrente
{
    public class InativarContaCorrenteUseCase(
        IIdempotenciaService idempotenciaService,
        IContaCorrenteRepository contaCorrenteRepository) : IInativarContaCorrenteUseCase
    {
        public async Task<ResultadoOperacao<object>> ExecutarAsync(InativarContaCorrenteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdContaCorrente))
            {
                return ResultadoOperacao<object>.Falha(
                    "USER_UNAUTHORIZED",
                    ["Token inválido ou expirado."]
                );
            }

            var conta = await contaCorrenteRepository.ObterPorIdAsync(request.IdContaCorrente);
            if (conta == null)
            {
                return ResultadoOperacao<object>.Falha(
                    "INVALID_ACCOUNT",
                    ["Conta corrente não cadastrada."]
                );
            }

            if (conta.Ativo == false)
            {
                return ResultadoOperacao<object>.Falha(
                    "ALREADY_INACTIVE",
                    ["A conta corrente já está inativa."]
                );
            }

            if (Domain.Entities.ContaCorrente.HashSenha(request.Senha, conta.Salt) != conta.Senha)
            {
                return ResultadoOperacao<object>.Falha(
                    "INVALID_ACCOUNT",
                    ["Senha inválida."]
                );
            }

            return await idempotenciaService.ObterOuRegistrarAsync(
                request,
                async () =>
                {
                    await contaCorrenteRepository.InativarAsync(request.IdContaCorrente);

                    return ResultadoOperacao<object>.SucessoResultado(null!);
                }
            );
        }
    }
}
