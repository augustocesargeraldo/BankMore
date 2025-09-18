using BankMore.ContaCorrente.Application.Repositories;
using Dapper;
using System.Data;

namespace BankMore.ContaCorrente.Infrastructure.Data
{
    public class ContaCorrenteRepository(IDbConnection db) : IContaCorrenteRepository
    {
        public async Task<Domain.Entities.ContaCorrente?> ObterPorNumeroAsync(int numero)
        {
            var sql = "SELECT * FROM contacorrente WHERE numero = @Numero";
            return await db.QueryFirstOrDefaultAsync<Domain.Entities.ContaCorrente>(sql, new { Numero = numero });
        }                

        public async Task<Domain.Entities.ContaCorrente?> ObterPorIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var sql = @"SELECT idcontacorrente, numero, nome, ativo, senha, salt
                FROM contacorrente
                WHERE idcontacorrente = @Id";

            return await db.QueryFirstOrDefaultAsync<Domain.Entities.ContaCorrente>(
                sql,
                new { Id = id }
            );
        }

        public async Task AdicionarAsync(Domain.Entities.ContaCorrente conta)
        {
            const string sql = @"
                INSERT INTO contacorrente (
                    idcontacorrente,
                    numero,
                    nome,
                    ativo,
                    senha,
                    salt
                )
                VALUES (
                    @IdContaCorrente,
                    @Numero,
                    @Nome,
                    @Ativo,
                    @Senha,
                    @Salt
                );
            ";

            await db.ExecuteAsync(sql, new
            {
                IdContaCorrente = conta.IdContaCorrente.ToString(),
                Numero = conta.Numero,
                Nome = conta.Nome,
                Ativo = conta.Ativo ? 1 : 0,
                Senha = conta.Senha,
                Salt = conta.Salt
            });
        }

        public async Task InativarAsync(string idContaCorrente)
        {
            if (string.IsNullOrWhiteSpace(idContaCorrente))
                throw new ArgumentNullException(nameof(idContaCorrente));

            var sql = @"UPDATE contacorrente
                SET ativo = 0
                WHERE idcontacorrente = @IdContaCorrente";

            await db.ExecuteAsync(sql, new { IdContaCorrente = idContaCorrente });
        }

        public async Task<decimal> ObterSaldoAsync(string idContaCorrente)
        {
            if (string.IsNullOrWhiteSpace(idContaCorrente))
                return 0m;

            var sql = @"
                SELECT 
                    COALESCE(SUM(
                        CASE 
                            WHEN m.tipomovimento = 'C' THEN m.valor
                            WHEN m.tipomovimento = 'D' THEN -m.valor
                            ELSE 0
                        END
                    ), 0) AS Saldo
                FROM movimento m
                WHERE m.idcontacorrente = @IdContaCorrente";

            return await db.ExecuteScalarAsync<decimal>(sql, new { IdContaCorrente = idContaCorrente });
        }
    }
}
