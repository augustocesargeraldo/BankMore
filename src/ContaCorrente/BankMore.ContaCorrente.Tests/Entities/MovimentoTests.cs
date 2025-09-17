using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Tests.Entities
{
    public class MovimentoTests
    {
        [Fact(DisplayName = "Deve criar movimento de crédito com sucesso")]
        public void CriarMovimentoCredito_ComSucesso()
        {
            // Arrange
            var idConta = Guid.NewGuid().ToString();
            var valor = 100m;

            // Act
            var movimento = Movimento.Criar(idConta, "C", valor);

            // Assert
            Assert.NotNull(movimento);
            Assert.Equal(idConta, movimento.IdContaCorrente);
            Assert.Equal("C", movimento.TipoMovimento);
            Assert.Equal(valor, movimento.Valor);
            Assert.False(string.IsNullOrWhiteSpace(movimento.IdMovimento));
            Assert.True(movimento.DataMovimento <= DateTime.UtcNow);
        }

        [Fact(DisplayName = "Deve criar movimento de débito com sucesso")]
        public void CriarMovimentoDebito_ComSucesso()
        {
            // Arrange
            var idConta = Guid.NewGuid().ToString();
            var valor = 50m;

            // Act
            var movimento = Movimento.Criar(idConta, "D", valor);

            // Assert
            Assert.NotNull(movimento);
            Assert.Equal(idConta, movimento.IdContaCorrente);
            Assert.Equal("D", movimento.TipoMovimento);
            Assert.Equal(valor, movimento.Valor);
        }

        [Fact(DisplayName = "Não deve criar movimento com conta inválida")]
        public void CriarMovimento_ComContaInvalida_DeveLancarExcecao()
        {
            // Arrange
            var idConta = "";
            var valor = 100m;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => Movimento.Criar(idConta, "C", valor));
            Assert.Contains("Conta corrente inválida.", ex.Message);
        }

        [Fact(DisplayName = "Não deve criar movimento com valor menor ou igual a zero")]
        public void CriarMovimento_ComValorInvalido_DeveLancarExcecao()
        {
            // Arrange
            var idConta = Guid.NewGuid().ToString();

            // Act & Assert
            var ex1 = Assert.Throws<ArgumentException>(() => Movimento.Criar(idConta, "C", 0));
            Assert.Contains("O valor deve ser maior que zero.", ex1.Message);

            var ex2 = Assert.Throws<ArgumentException>(() => Movimento.Criar(idConta, "C", -10));
            Assert.Contains("O valor deve ser maior que zero.", ex2.Message);
        }

        [Fact(DisplayName = "Não deve criar movimento com tipo inválido")]
        public void CriarMovimento_ComTipoInvalido_DeveLancarExcecao()
        {
            // Arrange
            var idConta = Guid.NewGuid().ToString();
            var valor = 100m;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => Movimento.Criar(idConta, "X", valor));
            Assert.Contains("Tipo de movimento inválido. Use 'C' (crédito) ou 'D' (débito).", ex.Message);
        }
    }
}
