using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankMore.ContaCorrente.Application.UseCases.InativarContaCorrente
{
    public class InativarContaCorrenteRequest
    {
        [JsonIgnore]
        public string? IdContaCorrente { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; } = string.Empty;       
    }
}
