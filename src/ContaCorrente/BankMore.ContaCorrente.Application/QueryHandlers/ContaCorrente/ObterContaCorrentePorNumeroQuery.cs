namespace BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente
{
    public class ObterContaCorrentePorNumeroQuery
    {
        public int NumeroConta { get; }

        public ObterContaCorrentePorNumeroQuery(int numeroConta)
        {
            if (numeroConta <= 0)
                throw new ArgumentException("Número da conta deve ser maior que zero.", nameof(numeroConta));

            NumeroConta = numeroConta;
        }
    }
}
