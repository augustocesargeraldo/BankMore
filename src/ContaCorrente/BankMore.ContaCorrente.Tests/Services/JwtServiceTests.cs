using BankMore.ContaCorrente.Application.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BankMore.ContaCorrente.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;
        private readonly string _secret = "TESTE_SUPER_SECRETO_256bits_minimo_1234567890";
        private readonly int _expirationMinutes = 60;

        public JwtServiceTests()
        {
            _jwtService = new JwtService(_secret, _expirationMinutes);
        }

        [Fact(DisplayName = "Deve gerar token válido com claim sub")]
        public void DeveGerarTokenValido()
        {
            // Arrange
            var idConta = "12345";

            // Act
            var token = _jwtService.GerarToken(idConta);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            Assert.NotNull(subClaim);
            Assert.Equal(idConta, subClaim.Value);
        }

        [Fact(DisplayName = "Construtor lança exceção se secret ou expirationMinutes não configurados")]
        public void Construtor_DeveLancarExcecao_SeParametrosInvalidos()
        {
            // Secret nulo ou vazio
            Assert.Throws<ArgumentException>(() => new JwtService(null!, 60));
            Assert.Throws<ArgumentException>(() => new JwtService("", 60));

            // ExpirationMinutes nulo
            Assert.Throws<ArgumentException>(() => new JwtService(_secret, null));
        }
    }
}
