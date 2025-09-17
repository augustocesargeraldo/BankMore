using System.ComponentModel.DataAnnotations;

namespace BankMore.Transferencia.Application.UseCases.EfetuarTransferencia
{
    public class EfetuarTransferenciaRequest
    {
        [Required(ErrorMessage = "A identificação da requisição é obrigatória.")]
        [MinLength(1, ErrorMessage = "A identificação da requisição não pode ser vazia.")]
        public string IdRequisicao { get; set; } = null!;

        [Required(ErrorMessage = "Número da conta de destino é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Número da conta de destino deve ser maior que zero.")]
        public int NumeroContaDestino { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }
    }
}
