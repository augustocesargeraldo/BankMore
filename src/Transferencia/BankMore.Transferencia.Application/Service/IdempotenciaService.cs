using BankMore.Transferencia.Application.Repositories;
using BankMore.Transferencia.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Transactions;

namespace BankMore.Transferencia.Application.Service
{
    public class IdempotenciaService(IIdempotenciaRepository repository) : IIdempotenciaService
    {
        public async Task<TResponse> ObterOuRegistrarAsync<TRequest, TResponse>(
            TRequest request,
            Func<Task<TResponse>> executar)
        {
            // 1. Serializa o request
            var requestJson = JsonSerializer.Serialize(request);

            // 2. Gera hash SHA256 do request
            var requestHash = GerarHash(requestJson);

            // 3. Consulta idempotência
            var existente = await repository.ObterPorRequisicaoAsync(requestHash);
            if (existente != null)
            {
                return JsonSerializer.Deserialize<TResponse>(existente.Resultado)!;
            }

            // TransactionScope não funciona para SQLite mas já deixei preparado pois funciona no Oracle
            TResponse response;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // 4. Executa a operação real
                response = await executar();

                // 5. Serializa e persiste o resultado
                var resultadoJson = JsonSerializer.Serialize(response);
                var idempotencia = Idempotencia.Criar(requestHash, resultadoJson);
                await repository.SalvarAsync(idempotencia);

                scope.Complete();
            }

            return response;
        }

        private static string GerarHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}
