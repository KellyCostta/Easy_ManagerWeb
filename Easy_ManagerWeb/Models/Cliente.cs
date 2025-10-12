using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy_ManagerWeb.Models
{
    [Table("cliente")] // nome exato da tabela no banco
    public class Cliente
    {
        [Key]
        [Column("id_cliente")]
        public int Id { get; set; }

        [Required]
        [Column("nome_cliente")]
        public string Nome { get; set; } = string.Empty;

        [Column("email_cliente")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("telefone_cliente")]
        public string Telefone { get; set; } = string.Empty;

        [Required]
        [Column("endereco_cliente")]
        public string Endereco { get; set; } = string.Empty;

        [NotMapped] // não existe no banco, só para uso interno no sistema
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}
