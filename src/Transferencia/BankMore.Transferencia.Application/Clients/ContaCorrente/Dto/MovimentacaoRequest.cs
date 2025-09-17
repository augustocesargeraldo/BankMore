using System.ComponentModel.DataAnnotations;

namespace BankMore.Transferencia.Application.Clients.ContaCorrente.Dto
{
    public class MovimentacaoRequest
    {
        [Required(ErrorMessage = "A identificação da conta corrente é obrigatória.")]
        public string IdContaCorrente { get; set; } = default!;

        [Required(ErrorMessage = "A identificação da requisição é obrigatória.")]
        [MinLength(1, ErrorMessage = "A identificação da requisição não pode ser vazia.")]
        public string IdRequisicao { get; set; } = default!;

        [Required(ErrorMessage = "O valor da movimentação é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "O tipo de movimentação é obrigatório.")]
        [RegularExpression("^[CD]$", ErrorMessage = "O tipo de movimentação deve ser 'C' (crédito) ou 'D' (débito).")]
        public string TipoMovimento { get; set; } = null!;
    }
}
