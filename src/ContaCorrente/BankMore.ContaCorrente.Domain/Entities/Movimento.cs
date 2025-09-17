namespace BankMore.ContaCorrente.Domain.Entities
{
    public class Movimento
    {
        public string IdMovimento { get; private set; } = Guid.NewGuid().ToString();
        public string IdContaCorrente { get; private set; } = null!;
        public DateTime DataMovimento { get; private set; } = DateTime.UtcNow;
        public string TipoMovimento { get; private set; } = null!;
        public decimal Valor { get; private set; }

        private Movimento() { } // Dapper precisa de construtor vazio

        private Movimento(string idContaCorrente, string tipoMovimento, decimal valor)
        {
            IdContaCorrente = idContaCorrente;
            TipoMovimento = tipoMovimento;
            Valor = valor;
        }

        public static Movimento Criar(string idContaCorrente, string tipoMovimento, decimal valor)
        {
            if (string.IsNullOrWhiteSpace(idContaCorrente))
                throw new ArgumentException("Conta corrente inválida.", nameof(idContaCorrente));

            if (valor <= 0)
                throw new ArgumentException("O valor deve ser maior que zero.", nameof(valor));

            if (tipoMovimento != "C" && tipoMovimento != "D")
                throw new ArgumentException("Tipo de movimento inválido. Use 'C' (crédito) ou 'D' (débito).", nameof(tipoMovimento));

            return new Movimento(idContaCorrente, tipoMovimento, valor);
        }
    }
}
