using BankMore.Transferencia.Application.Common;

namespace BankMore.Transferencia.Application.UseCases.EfetuarTransferencia
{
    public interface IEfetuarTransferenciaUseCase
    {
        Task<ResultadoOperacao<object>> EfetuarTransferencia(string contaOrigem, EfetuarTransferenciaRequest request, string token);
    }
}
