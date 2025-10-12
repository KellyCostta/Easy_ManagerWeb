using Microsoft.EntityFrameworkCore;

namespace Easy_ManagerWeb.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Entrega> Entregas { get; set; }
        public DbSet<Pacote> Pacotes { get; set; }  
    }
}
