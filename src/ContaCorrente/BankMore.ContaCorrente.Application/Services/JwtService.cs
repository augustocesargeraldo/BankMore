using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankMore.ContaCorrente.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _secret;
        private readonly int _expirationMinutes;

        public JwtService(string secret, int? expirationMinutes)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("JWT Secret não configurado!", nameof(secret));

            if (!expirationMinutes.HasValue)
                throw new ArgumentException("JWT ExpirationMinutes não configurado!", nameof(secret));

            _secret = secret;
            _expirationMinutes = expirationMinutes.Value;
        }

        public string GerarToken(string idContaCorrente)
        {
            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, idContaCorrente),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
