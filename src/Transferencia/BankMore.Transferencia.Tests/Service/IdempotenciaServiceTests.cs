using BankMore.Transferencia.Application.Repositories;
using BankMore.Transferencia.Application.Service;
using Moq;
using System.Text.Json;

namespace BankMore.Transferencia.Tests.Service
{
    public class IdempotenciaServiceTests
    {
        private readonly Mock<IIdempotenciaRepository> _idempotenciaRepositoryMock;
        private readonly IdempotenciaService _idempotenciaService;

        public IdempotenciaServiceTests()
        {
            _idempotenciaRepositoryMock = new Mock<IIdempotenciaRepository>();
            _idempotenciaService = new IdempotenciaService(_idempotenciaRepositoryMock.Object);
        }

        [Fact(DisplayName = "Deve retornar resultado existente se chave de idempotência já existe")]
        public async Task DeveRetornarResultadoExistente()
        {
            // Arrange
            var request = new { Numero = 123, Nome = "Teste" };
            var expectedResponse = new { Sucesso = true, Numero = 123 };

            var requisicaoJson = JsonSerializer.Serialize(request);
            var resultadoJson = JsonSerializer.Serialize(expectedResponse);

            _idempotenciaRepositoryMock
                .Setup(r => r.ObterPorRequisicaoAsync(It.IsAny<string>()))
                .ReturnsAsync(Domain.Entities.Idempotencia.Criar(requisicaoJson, resultadoJson));

            // Act
            var resultado = await _idempotenciaService.ObterOuRegistrarAsync(
                request,
                () => Task.FromResult(expectedResponse)
            );

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(expectedResponse.Numero, resultado.Numero);
            _idempotenciaRepositoryMock.Verify(r => r.SalvarAsync(It.IsAny<Domain.Entities.Idempotencia>()), Times.Never);
        }

        [Fact(DisplayName = "Deve executar e salvar resultado se chave de idempotência não existe")]
        public async Task DeveExecutarENovoRegistro()
        {
            // Arrange
            var request = new { Numero = 456, Nome = "Novo Cliente" };
            var expectedResponse = new { Sucesso = true, Numero = 456 };

            _idempotenciaRepositoryMock
                .Setup(r => r.ObterPorRequisicaoAsync(It.IsAny<string>()))
                .ReturnsAsync((Domain.Entities.Idempotencia?)null);

            // Act
            var resultado = await _idempotenciaService.ObterOuRegistrarAsync(
                request,
                () => Task.FromResult(expectedResponse)
            );

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(expectedResponse.Numero, resultado.Numero);
            _idempotenciaRepositoryMock.Verify(r => r.SalvarAsync(It.IsAny<Domain.Entities.Idempotencia>()), Times.Once);
        }
    }
}
