namespace BankMore.Transferencia.Application.Service
{
    public interface IIdempotenciaService
    {
        Task<TResponse> ObterOuRegistrarAsync<TRequest, TResponse>(
            TRequest request,
            Func<Task<TResponse>> executar);
    }
}
