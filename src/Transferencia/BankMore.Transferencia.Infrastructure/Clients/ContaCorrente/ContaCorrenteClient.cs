using BankMore.Transferencia.Application.Clients.ContaCorrente;
using BankMore.Transferencia.Application.Clients.ContaCorrente.Dto;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BankMore.Transferencia.Infrastructure.Clients.ContaCorrente
{
    public class ContaCorrenteClient(HttpClient httpClient) : IContaCorrenteClient
    {
        public async Task MovimentarAsync(MovimentacaoRequest request, string token)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/contas-correntes/movimentos")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Erro ao movimentar conta corrente. Status: {response.StatusCode}. Body: {body}");
            }
        }

        public async Task<ContaCorrenteResponse?> ObterPorIdAsync(string idContaCorrente, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/contas-correntes/{idContaCorrente}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ContaCorrenteResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<ContaCorrenteResponse?> ObterPorNumeroAsync(int numeroConta, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/contas-correntes/numero/{numeroConta}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ContaCorrenteResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
