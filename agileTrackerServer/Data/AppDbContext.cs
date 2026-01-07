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
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<ProjectInvite> ProjectInvites => Set<ProjectInvite>();



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
                
                entity.Property(u => u.Type)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();
                
                entity.Property(u => u.PasswordHash)
                      .IsRequired();

                entity.Property(u => u.AvatarUrl)
                      .HasMaxLength(400);

                entity.Property(u => u.CreatedAt)
                      .IsRequired();
                
                entity.Property(u => u.UpdatedAt)
                      .IsRequired();
                
                entity.HasCheckConstraint(
                      "CK_User_Type",
                      "\"Type\" IN ('Free', 'Plus', 'Admin')"
                );
            });

            modelBuilder.Entity<Project>(entity =>
            {
                  entity.ToTable("projects");

                  entity.HasKey(p => p.Id);

                  entity.Property(p => p.Name)
                        .HasMaxLength(255)
                        .IsRequired();

                  entity.Property(p => p.Status)
                        .HasConversion<string>()
                        .IsRequired();

                  // 🔗 ÚNICO relacionamento Project → Members
                  entity.HasMany(p => p.Members)
                        .WithOne(pm => pm.Project)
                        .HasForeignKey(pm => pm.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<ProjectMember>(entity =>
            {
                  entity.ToTable("project_members");

                  entity.HasKey(pm => pm.Id);

                  entity.Property(pm => pm.Id)
                        .ValueGeneratedOnAdd();

                  entity.Property(pm => pm.ProjectId)
                        .IsRequired();

                  entity.Property(pm => pm.UserId)
                        .IsRequired();

                  entity.Property(pm => pm.Role)
                        .HasConversion<string>()
                        .HasMaxLength(30)
                        .IsRequired();

                  entity.Property(pm => pm.JoinedAt)
                        .HasDefaultValueSql("NOW()")
                        .IsRequired();

                  // 🔗 ProjectMember → User (COM navigation)
                  entity.HasOne(pm => pm.User)
                        .WithMany()
                        .HasForeignKey(pm => pm.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                  // 🔐 Um usuário só pode entrar uma vez por projeto
                  entity.HasIndex(pm => new { pm.ProjectId, pm.UserId })
                        .IsUnique();

                  entity.HasCheckConstraint(
                        "CK_ProjectMember_Role",
                        "\"Role\" IN ('Owner', 'ScrumMaster', 'ProductOwner', 'Developer')"
                  );
            });
            modelBuilder.Entity<ProjectInvite>(entity =>
            {
                  entity.ToTable("project_invites");

                  entity.HasKey(i => i.Id);

                  entity.Property(i => i.Email)
                        .HasMaxLength(255)
                        .IsRequired();

                  entity.Property(i => i.Token)
                        .IsRequired();

                  entity.Property(i => i.Role)
                        .HasConversion<string>()
                        .IsRequired();

                  entity.Property(i => i.ExpiresAt)
                        .IsRequired();

                  entity.Property(i => i.CreatedAt)
                        .IsRequired();

                  entity.Property(i => i.Accepted)
                        .IsRequired();

                  entity.HasIndex(i => i.Token)
                        .IsUnique();

                  entity.HasIndex(i => new { i.ProjectId, i.Email })
                        .IsUnique();

                  entity.HasOne<Project>()
                        .WithMany()
                        .HasForeignKey(i => i.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
