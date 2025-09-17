namespace BankMore.Transferencia.Application.Repositories
{
    public interface ITransferenciaRepository
    {
        Task AdicionarAsync(Domain.Entities.Transferencia transferencia);
    }
}
