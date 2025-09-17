using BankMore.ContaCorrente.Application.Common;

namespace BankMore.ContaCorrente.Application.UseCases.InativarContaCorrente
{
    public interface IInativarContaCorrenteUseCase
    {
        Task<ResultadoOperacao<object>> ExecutarAsync(InativarContaCorrenteRequest request);
    }
}
