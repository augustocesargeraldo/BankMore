using BankMore.Transferencia.Application.Repositories;
using Dapper;
using System.Data;

namespace BankMore.Transferencia.Infrastructure.Data
{
    public class TransferenciaRepository(IDbConnection db) : ITransferenciaRepository
    {
        public async Task AdicionarAsync(Domain.Entities.Transferencia transferencia)
        {
            const string sql = @"
                INSERT INTO transferencia (
                    idtransferencia,
                    idcontacorrente_origem,
                    idcontacorrente_destino,
                    datamovimento,
                    valor
                )
                VALUES (
                    @IdTransferencia,
                    @IdContaCorrenteOrigem,
                    @IdContaCorrenteDestino,
                    @DataMovimento,
                    @Valor
                );";

            await db.ExecuteAsync(sql, transferencia);
        }
    }
}
