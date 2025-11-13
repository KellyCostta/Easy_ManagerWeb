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

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [Column("nome_cliente")]
        public string Nome { get; set; } = string.Empty;

        [Column("email_cliente")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Column("telefone_cliente")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O endereço é obrigatório.")]
        [Column("endereco_cliente")]
        public string Endereco { get; set; } = string.Empty;

        // [NotMapped] ignora o banco e o campo é recalculado toda vez que acessado.
        [NotMapped]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Relacionamento com entregas (restante do código)
        public ICollection<Entrega>? Entregas { get; set; }
    }
}