namespace BankMore.ContaCorrente.Application.Services
{
    public interface IIdempotenciaService
    {
        Task<TResponse> ObterOuRegistrarAsync<TRequest, TResponse>(
            TRequest request,
            Func<Task<TResponse>> executar);
    }
}
