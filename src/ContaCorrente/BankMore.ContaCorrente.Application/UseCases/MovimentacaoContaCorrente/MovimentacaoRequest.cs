using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Application.UseCases.MovimentacaoContaCorrente
{
    public class MovimentacaoRequest
    {
        [StringLength(37, MinimumLength = 1, ErrorMessage = "IdContaCorrente deve ter entre 1 e 37 caracteres.")]
        public string? IdContaCorrente { get; set; }

        [Required(ErrorMessage = "A identificação da requisição é obrigatória.")]
        [MinLength(1, ErrorMessage = "A identificação da requisição não pode ser vazia.")]
        public string IdRequisicao { get; set; } = string.Empty;

        public int? NumeroConta { get; set; }

        [Required(ErrorMessage = "O valor da movimentação é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "O tipo de movimentação é obrigatório.")]
        [RegularExpression("^[CD]$", ErrorMessage = "O tipo de movimentação deve ser 'C' (crédito) ou 'D' (débito).")]
        public string TipoMovimento { get; set; } = null!;
    }
}
