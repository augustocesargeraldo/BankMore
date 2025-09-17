namespace BankMore.Transferencia.Domain.Entities
{
    public class Transferencia
    {
        public string IdTransferencia { get; private set; } = Guid.NewGuid().ToString();
        public string IdContaCorrenteOrigem { get; private set; } = null!;
        public string IdContaCorrenteDestino { get; private set; } = null!;
        public DateTime DataMovimento { get; private set; } = DateTime.UtcNow;
        public decimal Valor { get; private set; }

        private Transferencia() { } // Dapper precisa de construtor vazio

        public Transferencia(string idContaCorrenteOrigem, string idContaCorrenteDestino, decimal valor)
        {
            IdTransferencia = Guid.NewGuid().ToString();
            IdContaCorrenteOrigem = idContaCorrenteOrigem;
            IdContaCorrenteDestino = idContaCorrenteDestino;
            Valor = Math.Round(valor, 2); // garante 2 casas decimais
        }

        public static Transferencia Criar(string idContaCorrenteOrigem, string idContaCorrenteDestino, decimal valor)
        {
            if (string.IsNullOrWhiteSpace(idContaCorrenteOrigem))
                throw new ArgumentException("Conta de origem é obrigatória", nameof(idContaCorrenteOrigem));

            if (string.IsNullOrWhiteSpace(idContaCorrenteDestino))
                throw new ArgumentException("Conta de destino é obrigatória", nameof(idContaCorrenteDestino));

            if (idContaCorrenteOrigem == idContaCorrenteDestino)
                throw new ArgumentException("Conta de origem e destino não podem ser iguais");

            if (valor <= 0)
                throw new ArgumentException("O valor da transferência deve ser positivo", nameof(valor));

            return new Transferencia(idContaCorrenteOrigem, idContaCorrenteDestino, valor);
        }        
    }
}
