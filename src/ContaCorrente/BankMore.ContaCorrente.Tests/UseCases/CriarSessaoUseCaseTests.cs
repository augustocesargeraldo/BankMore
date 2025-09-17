using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Application.UseCases.CriarSessao;
using Moq;

namespace BankMore.ContaCorrente.Tests.UseCases
{
    public class CriarSessaoUseCaseTests
    {
        private readonly Mock<IContaCorrenteRepository> _contaCorrenteRepositoryMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly CriarSessaoUseCase _useCase;

        public CriarSessaoUseCaseTests()
        {
            _contaCorrenteRepositoryMock = new Mock<IContaCorrenteRepository>();
            _jwtServiceMock = new Mock<IJwtService>();

            _useCase = new CriarSessaoUseCase(
                _contaCorrenteRepositoryMock.Object,
                _jwtServiceMock.Object
            );
        }

        [Fact(DisplayName = "Deve criar sessão com token válido")]
        public async Task DeveCriarSessaoComTokenValido()
        {
            // Arrange
            var request = new CriarSessaoRequest { Numero = 123456, Senha = "senha123" };
            var conta = Domain.Entities.ContaCorrente.Criar(request.Numero, "Cliente Teste", request.Senha);

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorNumeroAsync(request.Numero))
                .ReturnsAsync(conta);

            _jwtServiceMock
                .Setup(s => s.GerarToken(conta.IdContaCorrente))
                .Returns("TOKEN_DE_TESTE");

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.True(resultado.Sucesso);
            Assert.NotNull(resultado.Dados);
            Assert.Equal("TOKEN_DE_TESTE", resultado.Dados.Token);
        }

        [Fact(DisplayName = "Não deve criar sessão se conta não existe")]
        public async Task NaoDeveCriarSessaoSeContaNaoExiste()
        {
            // Arrange
            var request = new CriarSessaoRequest { Numero = 123456, Senha = "senha123" };

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorNumeroAsync(request.Numero))
                .ReturnsAsync((Domain.Entities.ContaCorrente)null);

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("USER_UNAUTHORIZED", resultado.TipoFalha);
            Assert.Contains("inválidos", resultado.Mensagens?[0]);
        }

        [Fact(DisplayName = "Não deve criar sessão se senha inválida")]
        public async Task NaoDeveCriarSessaoSeSenhaInvalida()
        {
            // Arrange
            var request = new CriarSessaoRequest { Numero = 123456, Senha = "senhaErrada" };
            var conta = Domain.Entities.ContaCorrente.Criar(request.Numero, "Cliente Teste", "senha123"); // senha correta é "senha123"

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorNumeroAsync(request.Numero))
                .ReturnsAsync(conta);

            // Act
            var resultado = await _useCase.ExecutarAsync(request);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("USER_UNAUTHORIZED", resultado.TipoFalha);
            Assert.Contains("inválidos", resultado.Mensagens?[0]);
        }
    }
}
