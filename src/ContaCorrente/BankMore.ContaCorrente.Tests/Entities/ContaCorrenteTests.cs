namespace BankMore.ContaCorrente.Tests.Entities
{
    public class ContaCorrenteTests
    {
        [Fact(DisplayName = "Deve criar conta corrente válida")]
        public void DeveCriarContaCorrenteValida()
        {
            // Arrange
            var numero = 123456;
            var nome = "Cliente Teste";
            var senha = "senha123";

            // Act
            var conta = Domain.Entities.ContaCorrente.Criar(numero, nome, senha);

            // Assert
            Assert.NotNull(conta);
            Assert.Equal(numero, conta.Numero);
            Assert.Equal(nome, conta.Nome);
            Assert.True(conta.Ativo);
            Assert.NotNull(conta.Senha);
            Assert.NotNull(conta.Salt);
        }

        [Theory(DisplayName = "Não deve criar conta com número inválido")]
        [InlineData(0)]
        [InlineData(-1)]
        public void NaoDeveCriarContaComNumeroInvalido(int numeroInvalido)
        {
            // Arrange
            var nome = "Cliente Teste";
            var senha = "senha123";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                Domain.Entities.ContaCorrente.Criar(numeroInvalido, nome, senha)
            );

            Assert.Contains("Número inválido", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory(DisplayName = "Não deve criar conta com nome inválido")]
        [InlineData("")]
        [InlineData("AB")]
        [InlineData("A")]
        public void NaoDeveCriarContaComNomeCurto(string nomeInvalido)
        {
            // Arrange
            var numero = 123456;
            var senha = "senha123";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                Domain.Entities.ContaCorrente.Criar(numero, nomeInvalido, senha)
            );

            Assert.Contains("Nome inválido", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "Não deve criar conta com nome muito longo")]
        public void NaoDeveCriarContaComNomeMuitoLongo()
        {
            // Arrange
            var numero = 123456;
            var senha = "senha123";
            var nomeInvalido = new string('N', 101); // 101 caracteres

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                Domain.Entities.ContaCorrente.Criar(numero, nomeInvalido, senha)
            );

            Assert.Contains("Nome inválido", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory(DisplayName = "Não deve criar conta com senha inválida")]
        [InlineData("")]
        [InlineData("12345")] // menos de 6 caracteres
        public void NaoDeveCriarContaComSenhaCurta(string senhaInvalida)
        {
            // Arrange
            var numero = 123456;
            var nome = "Cliente Teste";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                Domain.Entities.ContaCorrente.Criar(numero, nome, senhaInvalida)
            );

            Assert.Contains("Senha inválida", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "Não deve criar conta com senha muito longa")]
        public void NaoDeveCriarContaComSenhaMuitoLonga()
        {
            // Arrange
            var numero = 123456;
            var nome = "Cliente Teste";
            var senhaInvalida = new string('S', 101); // 101 caracteres

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                Domain.Entities.ContaCorrente.Criar(numero, nome, senhaInvalida)
            );

            Assert.Contains("Senha inválida", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "Senha deve ser armazenada com hash e salt")]
        public void DeveArmazenarSenhaComHashESalt()
        {
            // Arrange
            var numero = 123456;
            var nome = "Cliente Teste";
            var senha = "senha123";

            // Act
            var conta = Domain.Entities.ContaCorrente.Criar(numero, nome, senha);

            // Assert
            Assert.NotEqual(senha, conta.Senha); // não deve armazenar senha pura
            Assert.NotNull(conta.Salt);
            Assert.True(conta.Salt.Length > 0);
        }

        [Fact(DisplayName = "Deve inativar conta")]
        public void DeveInativarConta()
        {
            // Arrange
            var conta = Domain.Entities.ContaCorrente.Criar(123456, "Cliente Teste", "senha123");

            // Act
            conta.InativarParaTeste();

            // Assert
            Assert.False(conta.Ativo);
        }
    }
}
