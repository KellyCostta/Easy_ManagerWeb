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
        public DbSet<DadosUsuario> usuarios { get; set; }
        public DbSet<Orcamento> Orcamentos { get; set; }
        public DbSet<Gasto> Gasto { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Garante o nome correto da tabela, independentemente do nome do DbSet
            modelBuilder.Entity<DadosUsuario>().ToTable("usuario");
            modelBuilder.Entity<Orcamento>().ToTable("Orcamentos");
        }

    }


}
