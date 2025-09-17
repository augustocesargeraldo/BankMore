using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Tests.Entities
{
    public class IdempotenciaTests
    {
        [Fact(DisplayName = "Deve criar idempotencia válida usando a fábrica")]
        public void DeveCriarIdempotenciaValida()
        {
            // Arrange
            var requisicao = "{ \"numero\": 123 }";
            var resultado = "{ \"sucesso\": true }";

            // Act
            var idem = Idempotencia.Criar(requisicao, resultado);

            // Assert
            Assert.NotNull(idem);
            Assert.False(string.IsNullOrWhiteSpace(idem.ChaveIdempotencia));
            Assert.Equal(requisicao, idem.Requisicao);
            Assert.Equal(resultado, idem.Resultado);
        }

        [Theory(DisplayName = "Permite resultado nulo ou vazio")]
        [InlineData(null)]
        [InlineData("")]
        public void DeveCriarIdempotenciaComResultadoNuloOuVazio(string resultado)
        {
            // Arrange
            var requisicao = "{ \"numero\": 123 }";

            // Act
            var idem = Idempotencia.Criar(requisicao, resultado);

            // Assert
            Assert.NotNull(idem);
            Assert.False(string.IsNullOrWhiteSpace(idem.ChaveIdempotencia));
            Assert.Equal(requisicao, idem.Requisicao);
            Assert.Equal(resultado, idem.Resultado);
        }
    }
}
