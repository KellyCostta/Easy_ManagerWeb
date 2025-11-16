// Pacote.cs
using Easy_ManagerWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy_ManagerWeb.Models
{
    [Table("pacote")]
    public class Pacote
    {
        [Key]
        [Column("id_pacote")]
        public int Id { get; set; }


        [Column("id_entrega")]
        public int? EntregaId { get; set; }

        [ForeignKey("EntregaId")]
        public Entrega? Entrega { get; set; }

        [Column("tamanho")]
        public string Tamanho { get; set; } = string.Empty;

        [Column("peso")]
        public string Peso { get; set; } = string.Empty;

        [Column("data_cadastro")]
        public DateTime? DataCadastro { get; set; }
    }
}