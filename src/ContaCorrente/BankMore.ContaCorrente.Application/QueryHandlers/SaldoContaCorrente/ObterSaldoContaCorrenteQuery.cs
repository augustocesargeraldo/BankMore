namespace BankMore.ContaCorrente.Application.QueryHandlers.SaldoContaCorrente
{
    public class ObterSaldoContaCorrenteQuery(string? idContaCorrente)
    {
        public string? IdContaCorrente { get; } = idContaCorrente;
    }
}
