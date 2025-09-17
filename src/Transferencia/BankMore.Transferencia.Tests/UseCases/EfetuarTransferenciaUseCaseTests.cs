using BankMore.Transferencia.Application.Clients.ContaCorrente;
using BankMore.Transferencia.Application.Clients.ContaCorrente.Dto;
using BankMore.Transferencia.Application.Common;
using BankMore.Transferencia.Application.Repositories;
using BankMore.Transferencia.Application.Service;
using BankMore.Transferencia.Application.UseCases.EfetuarTransferencia;
using Moq;

namespace BankMore.Transferencia.Tests.UseCases
{
    public class EfetuarTransferenciaUseCaseTests
    {
        private readonly Mock<IContaCorrenteClient> _contaClientMock;
        private readonly Mock<ITransferenciaRepository> _transferenciaRepoMock;
        private readonly Mock<IIdempotenciaService> _idempotenciaServiceMock;
        private readonly EfetuarTransferenciaUseCase _useCase;

        public EfetuarTransferenciaUseCaseTests()
        {
            _contaClientMock = new Mock<IContaCorrenteClient>();
            _transferenciaRepoMock = new Mock<ITransferenciaRepository>();
            _idempotenciaServiceMock = new Mock<IIdempotenciaService>();

            _useCase = new EfetuarTransferenciaUseCase(
                _idempotenciaServiceMock.Object,
                _contaClientMock.Object,
                _transferenciaRepoMock.Object
            );
        }

        [Fact(DisplayName = "Deve falhar se valor for menor ou igual a zero")]
        public async Task DeveFalhar_ValorInvalido()
        {
            var request = new EfetuarTransferenciaRequest
            {
                IdRequisicao = "req-123",
                NumeroContaDestino = 456,
                Valor = 0
            };

            var result = await _useCase.EfetuarTransferencia("origem-123", request, "token");

            Assert.False(result.Sucesso);
            Assert.Equal("INVALID_VALUE", result.TipoFalha);
        }

        [Fact(DisplayName = "Deve falhar se conta de origem não existir")]
        public async Task DeveFalhar_ContaOrigemInvalida()
        {
            _contaClientMock.Setup(c => c.ObterPorIdAsync("origem-123", "token"))
                .ReturnsAsync((ContaCorrenteResponse?)null);

            var request = new EfetuarTransferenciaRequest
            {
                IdRequisicao = "req-123",
                NumeroContaDestino = 456,
                Valor = 100
            };

            var result = await _useCase.EfetuarTransferencia("origem-123", request, "token");

            Assert.False(result.Sucesso);
            Assert.Equal("INVALID_ACCOUNT", result.TipoFalha);
        }

        [Fact(DisplayName = "Deve falhar se conta de destino não existir")]
        public async Task DeveFalhar_ContaDestinoInvalida()
        {
            _contaClientMock.Setup(c => c.ObterPorIdAsync("origem-123", "token"))
                .ReturnsAsync(new ContaCorrenteResponse { IdContaCorrente = "origem-123", Ativo = true });

            _contaClientMock.Setup(c => c.ObterPorNumeroAsync(456, "token"))
                .ReturnsAsync((ContaCorrenteResponse?)null);

            var request = new EfetuarTransferenciaRequest
            {
                IdRequisicao = "req-123",
                NumeroContaDestino = 456,
                Valor = 100
            };

            var result = await _useCase.EfetuarTransferencia("origem-123", request, "token");

            Assert.False(result.Sucesso);
            Assert.Equal("INVALID_ACCOUNT", result.TipoFalha);
        }

        [Fact(DisplayName = "Deve efetuar transferencia com sucesso")]
        public async Task DeveEfetuarTransferenciaComSucesso()
        {
            // Arrange
            _contaClientMock.Setup(c => c.ObterPorIdAsync("origem-123", "token"))
                .ReturnsAsync(new ContaCorrenteResponse { IdContaCorrente = "origem-123", Ativo = true });

            _contaClientMock.Setup(c => c.ObterPorNumeroAsync(456, "token"))
                .ReturnsAsync(new ContaCorrenteResponse { IdContaCorrente = "destino-456", Ativo = true });

            // Idempotência retorna a execução do delegate
            _idempotenciaServiceMock
                .Setup(s => s.ObterOuRegistrarAsync(It.IsAny<EfetuarTransferenciaRequest>(), It.IsAny<Func<Task<ResultadoOperacao<object>>>>()))
                .Returns<EfetuarTransferenciaRequest, Func<Task<ResultadoOperacao<object>>>>(
                    (req, func) => func()
                );

            var request = new EfetuarTransferenciaRequest
            {
                IdRequisicao = "req-123",
                NumeroContaDestino = 456,
                Valor = 100
            };

            // Act
            var result = await _useCase.EfetuarTransferencia("origem-123", request, "token");

            // Assert
            Assert.True(result.Sucesso);

            // Verifica que débito e crédito foram chamados
            _contaClientMock.Verify(c => c.MovimentarAsync(It.IsAny<MovimentacaoRequest>(), "token"), Times.Exactly(2));

            // Verifica que a transferência foi persistida
            _transferenciaRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Domain.Entities.Transferencia>()), Times.Once);
        }

        [Fact(DisplayName = "Deve estornar se crédito falhar")]
        public async Task DeveEstornarSeCreditoFalhar()
        {
            // Arrange
            _contaClientMock.Setup(c => c.ObterPorIdAsync("origem-123", "token"))
                .ReturnsAsync(new ContaCorrenteResponse { IdContaCorrente = "origem-123", Ativo = true });

            _contaClientMock.Setup(c => c.ObterPorNumeroAsync(456, "token"))
                .ReturnsAsync(new ContaCorrenteResponse { IdContaCorrente = "destino-456", Ativo = true });

            // Simula débito ok, crédito falha, estorno ok
            var movimentoCalls = 0;
            _contaClientMock
                .Setup(c => c.MovimentarAsync(It.IsAny<MovimentacaoRequest>(), "token"))
                .Returns(() =>
                {
                    movimentoCalls++;
                    if (movimentoCalls == 2)
                        throw new Exception("Erro no crédito"); // simula falha no crédito
                    return Task.CompletedTask; // débito e estorno
                });

            // Idempotência retorna a execução do delegate
            _idempotenciaServiceMock
                .Setup(s => s.ObterOuRegistrarAsync(It.IsAny<EfetuarTransferenciaRequest>(), It.IsAny<Func<Task<ResultadoOperacao<object>>>>()))
                .Returns<EfetuarTransferenciaRequest, Func<Task<ResultadoOperacao<object>>>>(
                    (req, func) => func()
                );

            var request = new EfetuarTransferenciaRequest
            {
                IdRequisicao = "req-123",
                NumeroContaDestino = 456,
                Valor = 100
            };

            // Act
            var result = await _useCase.EfetuarTransferencia("origem-123", request, "token");

            // Assert
            Assert.False(result.Sucesso);
            Assert.Equal("TRANSFER_FAILED", result.TipoFalha);

            // Verifica que o débito + crédito (falha) + estorno foram chamados
            _contaClientMock.Verify(c => c.MovimentarAsync(It.IsAny<MovimentacaoRequest>(), "token"), Times.Exactly(3));

            // Nenhuma transferência foi persistida
            _transferenciaRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Domain.Entities.Transferencia>()), Times.Never);
        }
    }
}
