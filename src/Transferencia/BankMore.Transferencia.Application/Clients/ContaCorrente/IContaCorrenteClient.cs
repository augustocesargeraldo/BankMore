using BankMore.Transferencia.Application.Clients.ContaCorrente.Dto;

namespace BankMore.Transferencia.Application.Clients.ContaCorrente
{
    public interface IContaCorrenteClient
    {
        Task MovimentarAsync(MovimentacaoRequest request, string token);
        Task<ContaCorrenteResponse?> ObterPorIdAsync(string idContaCorrente, string token);
        Task<ContaCorrenteResponse?> ObterPorNumeroAsync(int numeroConta, string token);
    }
}
