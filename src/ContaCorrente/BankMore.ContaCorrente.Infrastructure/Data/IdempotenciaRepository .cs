using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Domain.Entities;
using Dapper;
using System.Data;

namespace BankMore.ContaCorrente.Infrastructure.Data
{
    public class IdempotenciaRepository(IDbConnection db) : IIdempotenciaRepository
    {
        private readonly IDbConnection _db = db;

        public async Task<Idempotencia?> ObterPorRequisicaoAsync(string requisicao)
        {
            const string sql = "SELECT * FROM idempotencia WHERE requisicao = @Requisicao LIMIT 1";
            return await _db.QueryFirstOrDefaultAsync<Idempotencia>(sql, new { Requisicao = requisicao });
        }

        public async Task SalvarAsync(Idempotencia idempotencia)
        {
            const string sql = @"
            INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
            VALUES (@ChaveIdempotencia, @Requisicao, @Resultado)";
            await _db.ExecuteAsync(sql, new { idempotencia.ChaveIdempotencia, idempotencia.Requisicao, idempotencia.Resultado });
        }
    }
}
