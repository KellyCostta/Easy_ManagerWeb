using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy_ManagerWeb.Models
{
    [Table("gasto")]
    public class Gasto
    {
        [Key]
        [Column("id_gasto")]
        public int IdGasto { get; set; }

        [Required(ErrorMessage = "O tipo é obrigatório.")]
        [Column("tipo")]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Column("valor")]
        public double Valor { get; set; }

        [Column("data_gasto")]
        [DataType(DataType.Date)]
        public DateTime DataGasto { get; set; } = DateTime.Now;

    }
}
