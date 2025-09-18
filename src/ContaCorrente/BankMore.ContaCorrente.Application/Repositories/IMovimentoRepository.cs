using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Application.Repositories
{
    public interface IMovimentoRepository
    {
        Task AdicionarAsync(Movimento movimento);
    }
}
