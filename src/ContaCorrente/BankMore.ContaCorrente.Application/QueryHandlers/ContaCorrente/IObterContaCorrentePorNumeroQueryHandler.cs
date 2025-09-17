namespace BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente
{
    public interface IObterContaCorrentePorNumeroQueryHandler
    {
        Task<ContaCorrenteResponse?> Handle(ObterContaCorrentePorNumeroQuery query);
    }
}
