using System;
using System.ComponentModel.DataAnnotations;

namespace Easy_ManagerWeb.Models
{
    public class Orcamento
    {
        [Key]
        public int Id { get; set; }

        // Dados do cliente
        [Required]
        public string? NomeCliente { get; set; }
        public string? TelefoneCliente { get; set; }
        public string? EnderecoCliente { get; set; }

        // Dados do pacote

        [Required]
        public required string Tamanho { get; set; }
        public string? Peso { get; set; } 

        // Dados da entrega
        [Required]
        public double Distancia { get; set; } 
        public required string Tempo { get; set; }

        [Display(Name = "Valor do Orçamento")]
        [DataType(DataType.Currency)]
        public double ValorOrcamento { get; set; }

        public string Status { get; set; } = "Em Potencial";
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}
