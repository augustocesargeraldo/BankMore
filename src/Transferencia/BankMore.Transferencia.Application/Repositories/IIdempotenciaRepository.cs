using BankMore.Transferencia.Domain.Entities;

namespace BankMore.Transferencia.Application.Repositories
{
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> ObterPorRequisicaoAsync(string requisicaoJson);
        Task SalvarAsync(Idempotencia idempotencia);
    }
}
