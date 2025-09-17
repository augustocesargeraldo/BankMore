using BankMore.ContaCorrente.Application.Repositories;

namespace BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente
{
    public class ObterContaCorrentePorIdQueryHandler(IContaCorrenteRepository contaCorrenteRepository) : IObterContaCorrentePorIdQueryHandler
    {
        public async Task<ContaCorrenteResponse?> Handle(ObterContaCorrentePorIdQuery query)
        {
            var conta = await contaCorrenteRepository.ObterPorIdAsync(query.IdContaCorrente);

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
