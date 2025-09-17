using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Application.UseCases.CriarContaCorrente;
using Moq;

namespace BankMore.ContaCorrente.Tests.UseCases
{
    public class CadastrarContaCorrenteUseCaseTests
    {
        private readonly Mock<IIdempotenciaService> _idempotenciaServiceMock;
        private readonly Mock<IContaCorrenteRepository> _contaCorrenteRepositoryMock;
        private readonly CriarContaCorrenteUseCase _useCase;

        public CadastrarContaCorrenteUseCaseTests()
        {
            _idempotenciaServiceMock = new Mock<IIdempotenciaService>();
            _contaCorrenteRepositoryMock = new Mock<IContaCorrenteRepository>();

            // O IdempotenciaService deve apenas executar a função recebida
            _idempotenciaServiceMock
                .Setup(s => s.ObterOuRegistrarAsync(
                    It.IsAny<CriarContaCorrenteRequest>(),
                    It.IsAny<Func<Task<ResultadoOperacao<CriarContaCorrenteResponse>>>>()
                ))
                .Returns<CriarContaCorrenteRequest, Func<Task<ResultadoOperacao<CriarContaCorrenteResponse>>>>(
                    async (req, func) => await func()
                );

            _useCase = new CriarContaCorrenteUseCase(
                _idempotenciaServiceMock.Object,
                _contaCorrenteRepositoryMock.Object
            );
        }

        [Fact(DisplayName = "Deve cadastrar conta corrente com sucesso")]
        public async Task DeveCadastrarContaCorrente_ComSucesso()
        {
            // Arrange
            var request = new CriarContaCorrenteRequest
            {
                Numero = 123456,
                Nome = "Cliente Sucesso",
                Senha = "senha123"
            };

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorNumeroAsync(request.Numero))
                .ReturnsAsync((Domain.Entities.ContaCorrente?)null);

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.True(resultado.Sucesso);
            Assert.NotNull(resultado.Dados);
            Assert.Equal(request.Numero, resultado.Dados!.Numero);

            _contaCorrenteRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Domain.Entities.ContaCorrente>()), Times.Once);
        }

        [Fact(DisplayName = "Não deve cadastrar conta corrente se número já existe")]
        public async Task NaoDeveCadastrarContaCorrente_SeNumeroJaExiste()
        {
            // Arrange
            var request = new CriarContaCorrenteRequest
            {
                Numero = 123456,
                Nome = "Cliente Conflito",
                Senha = "senha123"
            };

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorNumeroAsync(request.Numero))
                .ReturnsAsync(Domain.Entities.ContaCorrente.Criar(request.Numero, "Outro Cliente", "senha123"));

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("ACCOUNT_NUMBER_CONFLICT", resultado.TipoFalha);
            Assert.Contains("já está em uso", resultado.Mensagens![0]);

            _contaCorrenteRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Domain.Entities.ContaCorrente>()), Times.Never);
        }

        [Fact(DisplayName = "Deve passar a requisição pelo serviço de idempotência")]
        public async Task DevePassarPorIdempotenciaService()
        {
            // Arrange
            var request = new CriarContaCorrenteRequest
            {
                Numero = 789012,
                Nome = "Cliente Idempotencia",
                Senha = "senha123"
            };

            // Forçar repositório a não encontrar conta existente
            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorNumeroAsync(request.Numero))
                .ReturnsAsync((Domain.Entities.ContaCorrente?)null);

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            _idempotenciaServiceMock.Verify(s => s.ObterOuRegistrarAsync(
                request,
                It.IsAny<Func<Task<ResultadoOperacao<CriarContaCorrenteResponse>>>>()
            ), Times.Once);

            Assert.True(resultado.Sucesso);
        }
    }
}
