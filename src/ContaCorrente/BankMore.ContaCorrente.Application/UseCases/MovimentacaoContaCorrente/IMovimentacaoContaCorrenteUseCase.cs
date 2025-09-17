using BankMore.ContaCorrente.Application.Common;

namespace BankMore.ContaCorrente.Application.UseCases.MovimentacaoContaCorrente
{
    public interface IMovimentacaoContaCorrenteUseCase
    {
        Task<ResultadoOperacao<object>> ExecutarAsync(MovimentacaoRequest request);
    }
}
