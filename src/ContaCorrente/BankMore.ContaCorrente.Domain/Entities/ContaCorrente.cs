using System.Security.Cryptography;
using System.Text;

namespace BankMore.ContaCorrente.Domain.Entities
{
    public class ContaCorrente
    {
        public string IdContaCorrente { get; private set; } = string.Empty;
        public int Numero { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Senha { get; private set; } = string.Empty;
        public string Salt { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }

        private ContaCorrente() { } // Dapper precisa de construtor vazio

        // Fábrica para criar uma nova ContaCorrente
        public static ContaCorrente Criar(int numero, string nome, string senha)
        {
            if (numero < 1)
                throw new ArgumentException("Número inválido", nameof(numero));

            if (string.IsNullOrWhiteSpace(nome) || nome.Length < 3 || nome.Length > 100)
                throw new ArgumentException("Nome inválido", nameof(nome));

            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6 || senha.Length > 100)
                throw new ArgumentException("Senha inválida", nameof(senha));

            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            var senhaHash = HashSenha(senha, salt);

            return new ContaCorrente
            {
                IdContaCorrente = Guid.NewGuid().ToString(),
                Numero = numero,
                Nome = nome,
                Senha = senhaHash,
                Salt = salt,
                Ativo = true
            };
        }

        public static string HashSenha(string senha, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(senha + salt);
            var hash = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }

        public void InativarParaTeste()
        {
            Ativo = false;
        }
    }
}
