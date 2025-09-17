using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Domain.Entities;
using Dapper;
using System.Data;

namespace BankMore.ContaCorrente.Infrastructure.Data
{
    public class MovimentoRepository(IDbConnection db) : IMovimentoRepository
    {
        public async Task AdicionarAsync(Movimento movimento)
        {
            var sql = @"
                INSERT INTO movimento (
                    idmovimento,
                    idcontacorrente,
                    datamovimento,
                    tipomovimento,
                    valor
                ) VALUES (
                    @IdMovimento,
                    @IdContaCorrente,
                    @DataMovimento,
                    @TipoMovimento,
                    @Valor
                );";

            await db.ExecuteAsync(sql, movimento);
        }

        public async Task<IEnumerable<Movimento>> ObterPorContaAsync(string idContaCorrente)
        {
            if (string.IsNullOrWhiteSpace(idContaCorrente))
                return [];

            var sql = @"SELECT IdMovimento, IdContaCorrente, DataMovimento, TipoMovimento, Valor
                        FROM Movimento
                        WHERE IdContaCorrente = @IdContaCorrente
                        ORDER BY DataMovimento ASC";

            return await db.QueryAsync<Movimento>(sql, new { IdContaCorrente = idContaCorrente });
        }
    }
}
