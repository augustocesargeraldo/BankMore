using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Application.UseCases.MovimentacaoContaCorrente;
using BankMore.ContaCorrente.Domain.Entities;
using Moq;

namespace BankMore.ContaCorrente.Tests.UseCases
{
    public class MovimentacaoContaCorrenteUseCaseTests
    {
        private readonly Mock<IIdempotenciaService> _idempotenciaServiceMock;
        private readonly Mock<IContaCorrenteRepository> _contaCorrenteRepositoryMock;
        private readonly Mock<IMovimentoRepository> _movimentoRepositoryMock;
        private readonly MovimentacaoContaCorrenteUseCase _useCase;

        public MovimentacaoContaCorrenteUseCaseTests()
        {
            _idempotenciaServiceMock = new Mock<IIdempotenciaService>();
            _contaCorrenteRepositoryMock = new Mock<IContaCorrenteRepository>();
            _movimentoRepositoryMock = new Mock<IMovimentoRepository>();

            _idempotenciaServiceMock
                .Setup(s => s.ObterOuRegistrarAsync(
                    It.IsAny<MovimentacaoRequest>(),
                    It.IsAny<Func<Task<ResultadoOperacao<object>>>>()
                ))
                .Returns<MovimentacaoRequest, Func<Task<ResultadoOperacao<object>>>>(
                    async (req, func) => await func()
                );

            _useCase = new MovimentacaoContaCorrenteUseCase(
                _idempotenciaServiceMock.Object,
                _contaCorrenteRepositoryMock.Object,
                _movimentoRepositoryMock.Object
            );
        }

        [Fact(DisplayName = "Deve criar movimentacao com sucesso quando crédito")]
        public async Task DeveCriarMovimentacaoCredito_ComSucesso()
        {
            // Arrange
            var request = new MovimentacaoRequest
            {
                IdContaCorrente = "user-1",
                TipoMovimento = "C",
                Valor = 100m
            };

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(request.IdContaCorrente))
                .ReturnsAsync(Domain.Entities.ContaCorrente.Criar(12345, "Cliente", "senha123"));

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.True(resultado.Sucesso);
            _movimentoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Movimento>()), Times.Once);
        }

        [Fact(DisplayName = "Não deve criar movimentacao quando token inválido")]
        public async Task NaoDeveCriarMovimentacao_TokenInvalido()
        {
            // Arrange
            var request = new MovimentacaoRequest
            {
                IdContaCorrente = null,
                TipoMovimento = "C",
                Valor = 100m
            };

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("USER_UNAUTHORIZED", resultado.TipoFalha);
            _movimentoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Movimento>()), Times.Never);
        }

        [Fact(DisplayName = "Não deve criar movimentacao em conta inativa")]
        public async Task NaoDeveCriarMovimentacao_ContaInativa()
        {
            // Arrange
            var request = new MovimentacaoRequest
            {
                IdContaCorrente = "user-1",
                TipoMovimento = "C",
                Valor = 100m
            };

            var conta = Domain.Entities.ContaCorrente.Criar(12345, "Cliente", "senha123");
            conta.InativarParaTeste();

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(request.IdContaCorrente))
                .ReturnsAsync(conta);

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("INACTIVE_ACCOUNT", resultado.TipoFalha);
            _movimentoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Movimento>()), Times.Never);
        }

        [Theory(DisplayName = "Não deve criar movimentacao com valor inválido")]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task NaoDeveCriarMovimentacao_ValorInvalido(decimal valor)
        {
            // Arrange
            var request = new MovimentacaoRequest
            {
                IdContaCorrente = "user-1",
                TipoMovimento = "C",
                Valor = valor
            };

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(request.IdContaCorrente))
                .ReturnsAsync(Domain.Entities.ContaCorrente.Criar(12345, "Cliente", "senha123"));

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("INVALID_VALUE", resultado.TipoFalha);
            _movimentoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Movimento>()), Times.Never);
        }

        [Theory(DisplayName = "Não deve criar movimentacao com tipo inválido")]
        [InlineData("X")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task NaoDeveCriarMovimentacao_TipoInvalido(string tipo)
        {
            // Arrange
            var request = new MovimentacaoRequest
            {
                IdContaCorrente = "user-1",
                TipoMovimento = tipo ?? string.Empty,
                Valor = 100m
            };

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(request.IdContaCorrente))
                .ReturnsAsync(Domain.Entities.ContaCorrente.Criar(12345, "Cliente", "senha123"));

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("INVALID_TYPE", resultado.TipoFalha);
            _movimentoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Movimento>()), Times.Never);
        }

        [Fact(DisplayName = "Não permite débito em conta de outro usuário")]
        public async Task NaoDeveCriarMovimentacao_DebitoEmOutraConta()
        {
            // Arrange
            var request = new MovimentacaoRequest
            {
                IdContaCorrente = "user-1",
                NumeroConta = 54321,
                TipoMovimento = "D",
                Valor = 50m
            };

            var outraConta = Domain.Entities.ContaCorrente.Criar(12345, "Outro Cliente", "senha123");

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorNumeroAsync(It.IsAny<int>()))
                .ReturnsAsync(outraConta);

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("INVALID_TYPE", resultado.TipoFalha);
            _movimentoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Movimento>()), Times.Never);
        }
    }
}
