namespace BankMore.Transferencia.Tests.Entities
{
    public class TransferenciaTests
    {
        [Fact(DisplayName = "Deve criar transferencia com dados válidos")]
        public void CriarTransferencia_ComDadosValidos_DeveFuncionar()
        {
            // Arrange
            var origem = "origem-123";
            var destino = "destino-456";
            var valor = 100.50m;

            // Act
            var transferencia = Domain.Entities.Transferencia.Criar(origem, destino, valor);

            // Assert
            Assert.NotNull(transferencia);
            Assert.Equal(origem, transferencia.IdContaCorrenteOrigem);
            Assert.Equal(destino, transferencia.IdContaCorrenteDestino);
            Assert.Equal(Math.Round(valor, 2), transferencia.Valor);
            Assert.NotNull(transferencia.IdTransferencia);
            Assert.True(transferencia.DataMovimento <= DateTime.UtcNow);
        }

        [Fact(DisplayName = "Não deve permitir conta de origem vazia")]
        public void CriarTransferencia_OrigemVazia_DeveLancarExcecao()
        {
            // Arrange
            var destino = "destino-456";
            var valor = 50m;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => Domain.Entities.Transferencia.Criar("", destino, valor));
            Assert.Contains("Conta de origem é obrigatória", ex.Message);
        }

        [Fact(DisplayName = "Não deve permitir conta de destino vazia")]
        public void CriarTransferencia_DestinoVazio_DeveLancarExcecao()
        {
            // Arrange
            var origem = "origem-123";
            var valor = 50m;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => Domain.Entities.Transferencia.Criar(origem, "", valor));
            Assert.Contains("Conta de destino é obrigatória", ex.Message);
        }

        [Fact(DisplayName = "Não deve permitir contas iguais")]
        public void CriarTransferencia_ContasIguais_DeveLancarExcecao()
        {
            // Arrange
            var conta = "conta-123";
            var valor = 50m;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => Domain.Entities.Transferencia.Criar(conta, conta, valor));
            Assert.Contains("Conta de origem e destino não podem ser iguais", ex.Message);
        }

        [Theory(DisplayName = "Não deve permitir valor negativo ou zero")]
        [InlineData(0)]
        [InlineData(-10)]
        public void CriarTransferencia_ValorInvalido_DeveLancarExcecao(decimal valor)
        {
            // Arrange
            var origem = "origem-123";
            var destino = "destino-456";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => Domain.Entities.Transferencia.Criar(origem, destino, valor));
            Assert.Contains("O valor da transferência deve ser positivo", ex.Message);
        }
    }
}
