using BankMore.ContaCorrente.Application.Common;

namespace BankMore.ContaCorrente.Application.UseCases.CriarContaCorrente
{
    public interface ICriarContaCorrenteUseCase
    {
        Task<ResultadoOperacao<CriarContaCorrenteResponse>> ExecutarAsync(CriarContaCorrenteRequest request);
    }
}
