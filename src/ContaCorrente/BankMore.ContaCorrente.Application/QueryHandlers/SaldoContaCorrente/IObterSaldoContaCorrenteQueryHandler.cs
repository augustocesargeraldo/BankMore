using BankMore.ContaCorrente.Application.Common;

namespace BankMore.ContaCorrente.Application.QueryHandlers.SaldoContaCorrente
{
    public interface IObterSaldoContaCorrenteQueryHandler
    {
        Task<ResultadoOperacao<SaldoContaCorrenteResponse>> Handle(ObterSaldoContaCorrenteQuery query);
    }
}
