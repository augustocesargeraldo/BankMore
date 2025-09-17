namespace BankMore.ContaCorrente.Application.Repositories
{
    public interface IContaCorrenteRepository
    {
        Task<Domain.Entities.ContaCorrente?> ObterPorNumeroAsync(int numero);
        Task AdicionarAsync(Domain.Entities.ContaCorrente conta);
        Task<Domain.Entities.ContaCorrente?> ObterPorIdAsync(string id);
        Task InativarAsync(string idContaCorrente);
    }
}