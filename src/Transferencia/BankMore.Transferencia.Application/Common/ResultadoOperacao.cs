namespace BankMore.Transferencia.Application.Common
{
    public class ResultadoOperacao<T>
    {
        public bool Sucesso { get; init; }
        public T? Dados { get; init; }
        public string? TipoFalha { get; init; }
        public string[]? Mensagens { get; init; }

        public static ResultadoOperacao<T> Falha(string tipoFalha, params string[] mensagens) =>
            new()
            {
                Sucesso = false,
                Dados = default,
                TipoFalha = tipoFalha,
                Mensagens = mensagens
            };

        public static ResultadoOperacao<T> SucessoResultado(T dados) =>
            new()
            {
                Sucesso = true,
                Dados = dados
            };
    }
}
