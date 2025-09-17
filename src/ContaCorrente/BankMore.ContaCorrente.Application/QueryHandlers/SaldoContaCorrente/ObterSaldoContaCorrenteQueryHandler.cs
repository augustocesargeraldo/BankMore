using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Repositories;

namespace BankMore.ContaCorrente.Application.QueryHandlers.SaldoContaCorrente
{
    public class ObterSaldoContaCorrenteQueryHandler(
        IContaCorrenteRepository contaCorrenteRepository,
        IMovimentoRepository movimentoRepository) : IObterSaldoContaCorrenteQueryHandler
    {
        public async Task<ResultadoOperacao<SaldoContaCorrenteResponse>> Handle(ObterSaldoContaCorrenteQuery query)
        {
            if (string.IsNullOrWhiteSpace(query.IdContaCorrente))
                return ResultadoOperacao<SaldoContaCorrenteResponse>.Falha(
                    "USER_UNAUTHORIZED",
                    ["Token inválido ou expirado."]);

            // Busca a conta corrente
            var conta = await contaCorrenteRepository.ObterPorIdAsync(query.IdContaCorrente);
            if (conta == null)
                return ResultadoOperacao<SaldoContaCorrenteResponse>.Falha(
                    "INVALID_ACCOUNT",
                    ["Conta corrente não cadastrada."]);

            if (!conta.Ativo)
                return ResultadoOperacao<SaldoContaCorrenteResponse>.Falha(
                    "INACTIVE_ACCOUNT",
                    ["Conta corrente inativa."]);

            // Busca movimentos da conta
            var movimentos = await movimentoRepository.ObterPorContaAsync(conta.IdContaCorrente);

            // Calcula saldo: créditos - débitos
            var saldo = movimentos.Sum(m => m.TipoMovimento == "C" ? m.Valor : -m.Valor);

            var response = new SaldoContaCorrenteResponse
            {
                NumeroConta = conta.Numero,
                NomeTitular = conta.Nome,
                DataConsulta = DateTime.UtcNow,
                Saldo = saldo
            };

            return ResultadoOperacao<SaldoContaCorrenteResponse>.SucessoResultado(response);
        }
    }
}
