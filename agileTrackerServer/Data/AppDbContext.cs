using agileTrackerServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace agileTrackerServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ================================
        // 🔹 Tabelas do banco
        // ================================

        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();

        // ================================
        // 🔹 Configurações adicionais
        // ================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Habilita extensão UUID do PostgreSQL
            modelBuilder.HasPostgresExtension("uuid-ossp");

            // Chave primária e índices básicos
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(u => u.Name)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(u => u.Email)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.HasIndex(u => u.Email)
                      .IsUnique();
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("projects");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id)
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(p => p.Name)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(p => p.Status)
                      .HasMaxLength(50)
                      .HasDefaultValue("Active");

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("NOW()");

                // 🔗 Relacionamento Project → Owner (User)
                entity.HasOne(p => p.Owner)
                      .WithMany()
                      .HasForeignKey(p => p.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
