using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy_ManagerWeb.Models
{
    [Table("usuario")]
    public class DadosUsuario
    {
        [Key]
        [Column("nome_usuario")] 
        public string Usuario { get; set; } = string.Empty;

        [Required]
        [Column("email_usuario")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("senha_usuario")]
        public string Senha { get; set; } = string.Empty;
    }
}
