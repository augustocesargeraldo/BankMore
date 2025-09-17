using BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente;
using BankMore.ContaCorrente.Application.Repositories;
using Moq;

namespace BankMore.ContaCorrente.Tests.QueryHandlers
{
    public class ObterContaCorrentePorIdQueryHandlerTests
    {
        private readonly Mock<IContaCorrenteRepository> _repositoryMock;
        private readonly ObterContaCorrentePorIdQueryHandler _handler;

        public ObterContaCorrentePorIdQueryHandlerTests()
        {
            _repositoryMock = new Mock<IContaCorrenteRepository>();
            _handler = new ObterContaCorrentePorIdQueryHandler(_repositoryMock.Object);
        }

        [Fact(DisplayName = "Deve retornar null quando conta não existir")]
        public async Task Handle_ContaNaoExistente_DeveRetornarNull()
        {
            // Arrange
            var query = new ObterContaCorrentePorIdQuery("conta-inexistente");
            _repositoryMock
                .Setup(r => r.ObterPorIdAsync(query.IdContaCorrente))
                .ReturnsAsync((Domain.Entities.ContaCorrente?)null);

            // Act
            var resultado = await _handler.Handle(query);

            // Assert
            Assert.Null(resultado);
        }

        [Fact(DisplayName = "Deve retornar status ativo quando conta existir e estiver ativa")]
        public async Task Handle_ContaExistenteAtiva_DeveRetornarStatusAtivo()
        {
            // Arrange
            var query = new ObterContaCorrentePorIdQuery("conta-123");
            var conta = Domain.Entities.ContaCorrente.Criar(12345, "Nome", "Senha123");

            _repositoryMock
                .Setup(r => r.ObterPorIdAsync(query.IdContaCorrente))
                .ReturnsAsync(conta);

            // Act
            var resultado = await _handler.Handle(query);

            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado!.Ativo);
        }

        [Fact(DisplayName = "Deve retornar status inativo quando conta existir e estiver inativa")]
        public async Task Handle_ContaExistenteInativa_DeveRetornarStatusInativo()
        {
            // Arrange
            var query = new ObterContaCorrentePorIdQuery("conta-123");
            var conta = Domain.Entities.ContaCorrente.Criar(12345, "Nome", "Senha123");
            conta.InativarParaTeste();

            _repositoryMock
                .Setup(r => r.ObterPorIdAsync(query.IdContaCorrente))
                .ReturnsAsync(conta);

            // Act
            var resultado = await _handler.Handle(query);

            // Assert
            Assert.NotNull(resultado);
            Assert.False(resultado!.Ativo);
        }
    }
}
