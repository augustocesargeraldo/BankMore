using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Application.UseCases.CriarSessao
{
    public class CriarSessaoRequest
    {
        [Required(ErrorMessage = "O número da conta é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O número da conta deve ser maior que zero.")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }
}
