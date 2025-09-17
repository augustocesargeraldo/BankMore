using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Application.Repositories
{
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> ObterPorRequisicaoAsync(string requisicaoJson);
        Task SalvarAsync(Idempotencia idempotencia);
    }
}
