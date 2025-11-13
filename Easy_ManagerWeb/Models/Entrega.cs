using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy_ManagerWeb.Models
{
    [Table("entrega")]
    public class Entrega
    {
        [Key]
        [Column("id_entrega")]
        public int Id { get; set; }

        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [Column("distancia")]
        public double? Distancia { get; set; }

        [Column("tempo")]
        [StringLength(20)]
        public string Tempo { get; set; } = string.Empty;

        [Column("id_cliente")]
        public int? ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        [Column("id_pacote")]
        public int? PacoteId { get; set; }
        [ForeignKey("PacoteId")]
        public Pacote? Pacote { get; set; }

        [Column("orcamento")]
        [Display(Name = "Valor do Orçamento")]
        public double? Orcamento { get; set; } = 0;

        [Column("data_agendada")]
        [Display(Name = "Data Agendada")]
        [Required(ErrorMessage = "A data agendada é obrigatória.")]
        public DateTime DataAgendada { get; set; }




        [NotMapped]
        public string? Endereco => Cliente?.Endereco;
    }
}
