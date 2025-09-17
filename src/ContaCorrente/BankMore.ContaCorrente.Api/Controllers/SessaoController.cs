using BankMore.ContaCorrente.Api.Extensions;
using BankMore.ContaCorrente.Application.UseCases.CriarSessao;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.ContaCorrente.Api.Controllers
{
    [ApiController]
    [Route("api/sessoes")]
    public class SessoesController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Criar([FromServices] ICriarSessaoUseCase useCase, [FromBody] CriarSessaoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ToValidationErrorResponse());

            var resultado = await useCase.ExecutarAsync(request);

            if (!resultado.Sucesso)
                return Unauthorized(new
                {
                    resultado.TipoFalha,
                    resultado.Mensagens
                });

            return Ok(resultado.Dados);
        }
    }
}
