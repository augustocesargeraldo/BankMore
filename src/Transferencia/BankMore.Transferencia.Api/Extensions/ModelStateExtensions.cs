using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BankMore.Transferencia.Api.Extensions
{
    public static class ModelStateExtensions
    {
        public static object ToValidationErrorResponse(this ModelStateDictionary modelState)
        {
            var mensagens = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();

            return new
            {
                tipoFalha = "VALIDATION_ERROR",
                mensagens
            };
        }
    }
}
