using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Application.Services;

namespace BankMore.ContaCorrente.Application.UseCases.CriarSessao
{
    public class CriarSessaoUseCase(IContaCorrenteRepository contaCorrenteRepository, IJwtService jwtService) : ICriarSessaoUseCase
    {

        public async Task<ResultadoOperacao<CriarSessaoResponse>> ExecutarAsync(CriarSessaoRequest request)
        {
            var conta = await contaCorrenteRepository.ObterPorNumeroAsync(request.Numero);

            if (conta == null || Domain.Entities.ContaCorrente.HashSenha(request.Senha, conta.Salt) != conta.Senha)
            {
                return ResultadoOperacao<CriarSessaoResponse>.Falha(
                    "USER_UNAUTHORIZED",
                    ["Número da conta ou senha inválidos."]
                );
            }

            var token = jwtService.GerarToken(conta.IdContaCorrente);

            return ResultadoOperacao<CriarSessaoResponse>.SucessoResultado(new CriarSessaoResponse
            {
                Token = token
            });
        }
    }
}
