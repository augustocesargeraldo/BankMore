using BankMore.ContaCorrente.Application.QueryHandlers.SaldoContaCorrente;
using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Domain.Entities;
using Moq;

namespace BankMore.ContaCorrente.Tests.QueryHandlers
{
    public class ObterSaldoContaCorrenteQueryHandlerTests
    {
        private readonly Mock<IContaCorrenteRepository> _contaCorrenteRepositoryMock;
        private readonly Mock<IMovimentoRepository> _movimentoRepositoryMock;
        private readonly ObterSaldoContaCorrenteQueryHandler _handler;

        public ObterSaldoContaCorrenteQueryHandlerTests()
        {
            _contaCorrenteRepositoryMock = new Mock<IContaCorrenteRepository>();
            _movimentoRepositoryMock = new Mock<IMovimentoRepository>();

            _handler = new ObterSaldoContaCorrenteQueryHandler(
                _contaCorrenteRepositoryMock.Object,
                _movimentoRepositoryMock.Object
            );
        }

        [Fact(DisplayName = "Deve retornar falha USER_UNAUTHORIZED se IdContaCorrente for nulo ou vazio")]
        public async Task DeveFalhar_SeIdContaCorrenteNuloOuVazio()
        {
            // Arrange
            var query = new ObterSaldoContaCorrenteQuery(null!);

            // Act
            var resultado = await _handler.Handle(query);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("USER_UNAUTHORIZED", resultado.TipoFalha);
            Assert.Contains("Token inválido ou expirado", resultado.Mensagens![0]);
        }

        [Fact(DisplayName = "Deve retornar falha INVALID_ACCOUNT se conta não existir")]
        public async Task DeveFalhar_SeContaNaoExistir()
        {
            // Arrange
            var query = new ObterSaldoContaCorrenteQuery("conta-inexistente");

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(query.IdContaCorrente))
                .ReturnsAsync((Domain.Entities.ContaCorrente?)null);

            // Act
            var resultado = await _handler.Handle(query);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("INVALID_ACCOUNT", resultado.TipoFalha);
            Assert.Contains("Conta corrente não cadastrada", resultado.Mensagens![0]);
        }

        [Fact(DisplayName = "Deve retornar falha INACTIVE_ACCOUNT se conta estiver inativa")]
        public async Task DeveFalhar_SeContaInativa()
        {
            // Arrange
            var conta = Domain.Entities.ContaCorrente.Criar(123456, "Cliente Teste", "senha123");
            conta.InativarParaTeste();

            var query = new ObterSaldoContaCorrenteQuery(conta.IdContaCorrente);

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(conta.IdContaCorrente))
                .ReturnsAsync(conta);

            // Act
            var resultado = await _handler.Handle(query);

            // Assert
            Assert.False(resultado.Sucesso);
            Assert.Equal("INACTIVE_ACCOUNT", resultado.TipoFalha);
            Assert.Contains("Conta corrente inativa", resultado.Mensagens![0]);
        }

        [Fact(DisplayName = "Deve retornar saldo correto quando houver movimentações")]
        public async Task DeveRetornarSaldoCorreto()
        {
            // Arrange
            var conta = Domain.Entities.ContaCorrente.Criar(123456, "Cliente Teste", "senha123");

            var query = new ObterSaldoContaCorrenteQuery(conta.IdContaCorrente);

            var movimentos = new List<Movimento>
            {
                Movimento.Criar(conta.IdContaCorrente, "C", 100),
                Movimento.Criar(conta.IdContaCorrente, "D", 40),
                Movimento.Criar(conta.IdContaCorrente, "C", 60)
            };

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(conta.IdContaCorrente))
                .ReturnsAsync(conta);

            _movimentoRepositoryMock
                .Setup(r => r.ObterPorContaAsync(conta.IdContaCorrente))
                .ReturnsAsync(movimentos);

            // Act
            var resultado = await _handler.Handle(query);

            // Assert
            Assert.True(resultado.Sucesso);
            Assert.NotNull(resultado.Dados);
            Assert.Equal(123456, resultado.Dados!.NumeroConta);
            Assert.Equal(120, resultado.Dados!.Saldo); // 100 + 60 - 40
        }

        [Fact(DisplayName = "Deve retornar saldo zero quando não houver movimentações")]
        public async Task DeveRetornarSaldoZero_SeNaoHouverMovimentos()
        {
            // Arrange
            var conta = Domain.Entities.ContaCorrente.Criar(123456, "Cliente Teste", "senha123");

            var query = new ObterSaldoContaCorrenteQuery(conta.IdContaCorrente);

            _contaCorrenteRepositoryMock
                .Setup(r => r.ObterPorIdAsync(conta.IdContaCorrente))
                .ReturnsAsync(conta);

            _movimentoRepositoryMock
                .Setup(r => r.ObterPorContaAsync(conta.IdContaCorrente))
                .ReturnsAsync(new List<Movimento>());

            // Act
            var resultado = await _handler.Handle(query);

            // Assert
            Assert.True(resultado.Sucesso);
            Assert.NotNull(resultado.Dados);
            Assert.Equal(0, resultado.Dados!.Saldo);
        }
    }
}
