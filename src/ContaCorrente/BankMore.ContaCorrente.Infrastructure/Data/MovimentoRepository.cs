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
    }
}
