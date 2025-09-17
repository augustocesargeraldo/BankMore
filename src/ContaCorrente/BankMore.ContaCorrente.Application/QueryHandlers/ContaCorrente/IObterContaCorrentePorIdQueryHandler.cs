namespace BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente
{
    public interface IObterContaCorrentePorIdQueryHandler
    {
        Task<ContaCorrenteResponse?> Handle(ObterContaCorrentePorIdQuery query);
    }
}
