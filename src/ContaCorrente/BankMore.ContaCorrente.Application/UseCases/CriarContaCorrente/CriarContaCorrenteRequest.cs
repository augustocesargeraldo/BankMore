using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Application.UseCases.CriarContaCorrente
{
    public class CriarContaCorrenteRequest
    {
        [Required(ErrorMessage = "O número da conta é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O número da conta deve ser maior que zero.")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "O nome do titular é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Nome { get; set; } = default!;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
        public string Senha { get; set; } = default!;
    }
}
