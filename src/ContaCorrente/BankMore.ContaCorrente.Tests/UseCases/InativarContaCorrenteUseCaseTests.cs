using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Application.UseCases.InativarContaCorrente;
using Moq;

namespace BankMore.ContaCorrente.Tests.UseCases
{
    public class InativarContaCorrenteUseCaseTests
    {
        private readonly Mock<IContaCorrenteRepository> _contaCorrenteRepositoryMock;
        private readonly Mock<IIdempotenciaService> _idempotenciaServiceMock;
        private readonly InativarContaCorrenteUseCase _useCase;

        public InativarContaCorrenteUseCaseTests()
        {
            _contaCorrenteRepositoryMock = new Mock<IContaCorrenteRepository>();
            _idempotenciaServiceMock = new Mock<IIdempotenciaService>();

            // Idempotência executa a função passada diretamente
            _idempotenciaServiceMock
                .Setup(s => s.ObterOuRegistrarAsync(
                    It.IsAny<InativarContaCorrenteRequest>(),
                    It.IsAny<Func<Task<ResultadoOperacao<Object>>>>()
                ))
                .Returns<InativarContaCorrenteRequest, Func<Task<ResultadoOperacao<Object>>>>(
                    async (req, func) => await func()
                );

            _useCase = new InativarContaCorrenteUseCase(
                _idempotenciaServiceMock.Object,
                _contaCorrenteRepositoryMock.Object
            );
        }

        [Fact(DisplayName = "Deve falhar se IdContaCorrente não for informado")]
        public async Task DeveFalhar_SeIdContaCorrenteNaoInformado()
        {
            // Arrange
            var request = new InativarContaCorrenteRequest { IdContaCorrente = null!, Senha = "senha" };

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("USER_UNAUTHORIZED", resultado.TipoFalha);
            Assert.Contains("Token inválido", resultado.Mensagens![0]);
        }

        [Fact(DisplayName = "Deve falhar se a conta não existir")]
        public async Task DeveFalhar_SeContaNaoExistir()
        {
            // Arrange
            var request = new InativarContaCorrenteRequest { IdContaCorrente = "123", Senha = "senha" };
            _contaCorrenteRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdContaCorrente))
                .ReturnsAsync((Domain.Entities.ContaCorrente?)null);

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("INVALID_ACCOUNT", resultado.TipoFalha);
            Assert.Contains("não cadastrada", resultado.Mensagens![0]);
        }

        [Fact(DisplayName = "Deve falhar se a conta já estiver inativa")]
        public async Task DeveFalhar_SeContaJaInativa()
        {
            // Arrange
            var request = new InativarContaCorrenteRequest { IdContaCorrente = "12345", Senha = "Senha123" };
            var conta = Domain.Entities.ContaCorrente.Criar(12345, "Nome", "Senha123");
            conta.InativarParaTeste();

            _contaCorrenteRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdContaCorrente))
                .ReturnsAsync(conta);

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("ALREADY_INACTIVE", resultado.TipoFalha);
            Assert.Contains("já está inativa", resultado.Mensagens![0]);
        }

        [Fact(DisplayName = "Deve falhar se a senha for inválida")]
        public async Task DeveFalhar_SeSenhaInvalida()
        {
            // Arrange
            var request = new InativarContaCorrenteRequest { IdContaCorrente = "12345", Senha = "senhaErrada" };
            var conta = Domain.Entities.ContaCorrente.Criar(12345, "Nome", "Senha123");

            _contaCorrenteRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdContaCorrente))
                .ReturnsAsync(conta);

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("INVALID_ACCOUNT", resultado.TipoFalha);
            Assert.Contains("Senha inválida", resultado.Mensagens![0]);
        }

        [Fact(DisplayName = "Deve inativar conta ativa com sucesso")]
        public async Task DeveInativar_ContaAtivaComSucesso()
        {
            // Arrange
            var request = new InativarContaCorrenteRequest { IdContaCorrente = "12345", Senha = "senhaCorreta" };
            var conta = Domain.Entities.ContaCorrente.Criar(12345, "Nome", "senhaCorreta");

            _contaCorrenteRepositoryMock.Setup(r => r.ObterPorIdAsync(request.IdContaCorrente))
                .ReturnsAsync(conta);

            _contaCorrenteRepositoryMock.Setup(r => r.InativarAsync(request.IdContaCorrente))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.True(resultado.Sucesso);
            _contaCorrenteRepositoryMock.Verify(r => r.InativarAsync(request.IdContaCorrente), Times.Once);
        }
    }
}
