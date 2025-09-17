namespace BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente
{
    public class ObterContaCorrentePorIdQuery(string idContaCorrente)
    {
        public string IdContaCorrente { get; } = idContaCorrente;
    }
}
