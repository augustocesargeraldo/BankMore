namespace BankMore.ContaCorrente.Application.QueryHandlers.SaldoContaCorrente
{
    public class SaldoContaCorrenteResponse
    {
        public int NumeroConta { get; set; }
        public string NomeTitular { get; set; } = null!;
        public DateTime DataConsulta { get; set; }
        public decimal Saldo { get; set; }
    }
}
