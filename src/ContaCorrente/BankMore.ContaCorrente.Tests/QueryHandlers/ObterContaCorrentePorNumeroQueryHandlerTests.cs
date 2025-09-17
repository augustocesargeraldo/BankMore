using BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente;
using BankMore.ContaCorrente.Application.Repositories;
using Moq;

namespace BankMore.ContaCorrente.Tests.QueryHandlers
{
    public class ObterContaCorrentePorNumeroQueryHandlerTests
    {
        private readonly Mock<IContaCorrenteRepository> _contaCorrenteRepositoryMock;
        private readonly ObterContaCorrentePorNumeroQueryHandler _handler;

        public ObterContaCorrentePorNumeroQueryHandlerTests()
        {
            _contaCorrenteRepositoryMock = new Mock<IContaCorrenteRepository>();
            _handler = new ObterContaCorrentePorNumeroQueryHandler(_contaCorrenteRepositoryMock.Object);
        }

        [Fact(DisplayName = "Deve retornar conta existente quando número válido")]
        public async Task DeveRetornarContaExistente()
        {
            // Arrange
            var numeroConta = 12345;
            var conta = Domain.Entities.ContaCorrente.Criar(12345, "Nome", "Senha123");

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorNumeroAsync(numeroConta))
                .ReturnsAsync(conta);

            var query = new ObterContaCorrentePorNumeroQuery(numeroConta);

            // Act
            var result = await _handler.Handle(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(conta.IdContaCorrente, result!.IdContaCorrente);
            Assert.Equal(conta.Ativo, result.Ativo);
        }

        [Fact(DisplayName = "Deve retornar null quando número da conta não existir")]
        public async Task DeveRetornarNullQuandoContaNaoExistir()
        {
            // Arrange
            var numeroConta = 99999;

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorNumeroAsync(numeroConta))
                .ReturnsAsync((Domain.Entities.ContaCorrente?)null);

            var query = new ObterContaCorrentePorNumeroQuery(numeroConta);

            // Act
            var result = await _handler.Handle(query);

            // Assert
            Assert.Null(result);
        }
    }
}
