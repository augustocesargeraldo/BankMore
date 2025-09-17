using BankMore.ContaCorrente.Application.Repositories;

namespace BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente
{
    public class ObterContaCorrentePorNumeroQueryHandler(IContaCorrenteRepository contaCorrenteRepository) : IObterContaCorrentePorNumeroQueryHandler
    {
        public async Task<ContaCorrenteResponse?> Handle(ObterContaCorrentePorNumeroQuery query)
        {
            var conta = await contaCorrenteRepository.ObterPorNumeroAsync(query.NumeroConta);

            if (conta == null)
                return null;

            return new ContaCorrenteResponse
            {
                IdContaCorrente = conta.IdContaCorrente,
                Ativo = conta.Ativo
            };
        }
    }
}
