using BankMore.ContaCorrente.Application.Common;

namespace BankMore.ContaCorrente.Application.UseCases.CriarSessao
{
    public interface ICriarSessaoUseCase
    {
        Task<ResultadoOperacao<CriarSessaoResponse>> ExecutarAsync(CriarSessaoRequest request);
    }
}
