using System;
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

        [Column("tamanho")]
        public string Tamanho { get; set; } = string.Empty;

        [Column("peso")]
        public string Peso { get; set; } = string.Empty;


        [Display(Name = "Data de Cadastro")]
        [Column("data_cadastro")]
        public DateTime? DataCadastro { get; set; }
        public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();

    }
}
