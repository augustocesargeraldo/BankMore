using BankMore.ContaCorrente.Api.Extensions;
using BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente;
using BankMore.ContaCorrente.Application.QueryHandlers.SaldoContaCorrente;
using BankMore.ContaCorrente.Application.UseCases.CriarContaCorrente;
using BankMore.ContaCorrente.Application.UseCases.InativarContaCorrente;
using BankMore.ContaCorrente.Application.UseCases.MovimentacaoContaCorrente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankMore.ContaCorrente.Api.Controllers
{
    [Authorize] // Exige autenticação JWT
    [ApiController]
    [Route("api/contas-correntes")]
    public class ContasCorrentesController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Criar([FromServices] ICriarContaCorrenteUseCase useCase, [FromBody] CriarContaCorrenteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ToValidationErrorResponse());

            var resultado = await useCase.ExecutarAsync(request);

            if (!resultado.Sucesso)
            {
                if (resultado.TipoFalha == "ACCOUNT_NUMBER_CONFLICT")
                    return Conflict(new { resultado.TipoFalha, resultado.Mensagens });

                return BadRequest(new { resultado.TipoFalha, resultado.Mensagens });
            }

            return Created(string.Empty, resultado.Dados);
        }

        [HttpPatch("status")]
        public async Task<IActionResult> Inativar([FromServices] IInativarContaCorrenteUseCase useCase, [FromBody] InativarContaCorrenteRequest request)
        {
            request.IdContaCorrente = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!ModelState.IsValid)
                return BadRequest(ModelState.ToValidationErrorResponse());

            var resultado = await useCase.ExecutarAsync(request);

            if (!resultado.Sucesso)
            {
                if (resultado.TipoFalha == "USER_UNAUTHORIZED")
                    return Forbid();

                return BadRequest(new { resultado.TipoFalha, resultado.Mensagens });
            }

            return NoContent();
        }

        [HttpPost("movimentos")]
        public async Task<IActionResult> Movimentacao([FromServices] IMovimentacaoContaCorrenteUseCase useCase, [FromBody] MovimentacaoRequest request)
        {
            request.IdContaCorrente = request.IdContaCorrente ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!ModelState.IsValid)
                return BadRequest(ModelState.ToValidationErrorResponse());

            var resultado = await useCase.ExecutarAsync(request);

            if (!resultado.Sucesso)
            {
                if (resultado.TipoFalha == "USER_UNAUTHORIZED")
                    return Forbid();

                return BadRequest(new { resultado.TipoFalha, resultado.Mensagens });
            }

            return NoContent();
        }

        [HttpGet("saldo")]
        public async Task<IActionResult> ObterSaldo([FromServices] IObterSaldoContaCorrenteQueryHandler handler)
        {
            // Pega o Id da conta a partir do token JWT
            var idContaCorrente = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var query = new ObterSaldoContaCorrenteQuery(idContaCorrente);

            var resultado = await handler.Handle(query);

            if (!resultado.Sucesso)
            {
                return resultado.TipoFalha switch
                {
                    "USER_UNAUTHORIZED" => Forbid(),
                    "INVALID_ACCOUNT" => BadRequest(new { resultado.TipoFalha, resultado.Mensagens }),
                    "INACTIVE_ACCOUNT" => BadRequest(new { resultado.TipoFalha, resultado.Mensagens }),
                    _ => BadRequest(new { resultado.TipoFalha, resultado.Mensagens })
                };
            }

            return Ok(resultado.Dados);
        }

        [HttpGet("{idcontacorrente}")]
        public async Task<IActionResult> ObterPorId(string idcontacorrente, [FromServices] IObterContaCorrentePorIdQueryHandler handler)
        {
            var query = new ObterContaCorrentePorIdQuery(idcontacorrente);
            var result = await handler.Handle(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("numero/{numeroConta}")]
        public async Task<IActionResult> ObterPorNumero(int numeroConta, [FromServices] IObterContaCorrentePorNumeroQueryHandler handler)
        {
            var query = new ObterContaCorrentePorNumeroQuery(numeroConta);
            var result = await handler.Handle(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
