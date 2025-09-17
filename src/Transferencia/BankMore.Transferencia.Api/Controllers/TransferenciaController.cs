using BankMore.Transferencia.Application.UseCases.EfetuarTransferencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankMore.Transferencia.Api.Controllers
{
    [Authorize] // Exige autenticação JWT
    [ApiController]
    [Route("api/[controller]")]
    public class TransferenciaController(IEfetuarTransferenciaUseCase transferenciaService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> EfetuarTransferencia([FromBody] EfetuarTransferenciaRequest request)
        {
            var idConta = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(idConta))
                return Forbid(); // 403 se token inválido

            var authHeader = HttpContext.Request.Headers.Authorization.ToString();

            // Remove o prefixo "Bearer " (caso exista)
            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader["Bearer ".Length..].Trim()
                : authHeader;

            var resultado = await transferenciaService.EfetuarTransferencia(idConta, request, token);

            if (resultado.Sucesso)
                return NoContent(); // 204

            return BadRequest(new { resultado.TipoFalha, resultado.Mensagens });
        }
    }
}
