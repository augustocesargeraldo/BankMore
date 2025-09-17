namespace BankMore.ContaCorrente.Domain.Entities
{
    public class Idempotencia
    {
        public string ChaveIdempotencia { get; private set; } = string.Empty;
        public string Requisicao { get; private set; } = string.Empty;
        public string Resultado { get; private set; } = string.Empty;

        private Idempotencia() { } // Dapper precisa de construtor vazio

        // Fábrica para criar uma nova Idempotencia
        public static Idempotencia Criar(string requisicao, string resultado)
        {
            return new Idempotencia
            {
                ChaveIdempotencia = Guid.NewGuid().ToString(),
                Requisicao = requisicao,
                Resultado = resultado
            };
        }
    }
}
