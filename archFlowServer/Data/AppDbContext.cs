using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ================================
        // ðŸ”¹ Tabelas do banco
        // ================================

        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<ProjectInvite> ProjectInvites => Set<ProjectInvite>();
        public DbSet<ProductBacklog> ProductBacklogs => Set<ProductBacklog>();
        public DbSet<Epic> Epics => Set<Epic>();
        public DbSet<UserStory> UserStories => Set<UserStory>();
        public DbSet<Sprint> Sprints => Set<Sprint>();
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<SprintItem> SprintItems => Set<SprintItem>();




        // ================================
        // ðŸ”¹ ConfiguraÃ§Ãµes adicionais
        // ================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Habilita extensÃ£o UUID do PostgreSQL
            modelBuilder.HasPostgresExtension("uuid-ossp");

            // Chave primÃ¡ria e Ã­ndices bÃ¡sicos
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

                  // ðŸ”— ÃšNICO relacionamento Project â†’ Members
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

                  // ðŸ”— ProjectMember â†’ User (COM navigation)
                  entity.HasOne(pm => pm.User)
                        .WithMany()
                        .HasForeignKey(pm => pm.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                  // ðŸ” Um usuário sÃ³ pode entrar uma vez por projeto
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

                entity.Property(i => i.ProjectId)
                      .IsRequired();

                entity.Property(i => i.Email)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(i => i.Token)
                      .IsRequired();

                // ✅ Role como string (ok para legibilidade)
                entity.Property(i => i.Role)
                      .HasConversion<string>()
                      .IsRequired();

                // ✅ Status como INT (ideal para índice parcial e performance)
                entity.Property(i => i.Status)
                      .HasConversion<int>()
                      .IsRequired();

                entity.Property(i => i.ExpiresAt)
                      .IsRequired();

                entity.Property(i => i.CreatedAt)
                      .IsRequired();

                // ✅ Token deve ser único sempre
                entity.HasIndex(i => i.Token)
                      .IsUnique();

                // ✅ Partial Unique Index: impede duplicado apenas se Status == Pending (0)
                entity.HasIndex(i => new { i.ProjectId, i.Email })
                      .IsUnique()
                      .HasFilter($"\"Status\" = {(int)InviteStatus.Pending}");

                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(i => i.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductBacklog>(entity =>
            {
                  entity.ToTable("product_backlogs");

                  entity.HasKey(pb => pb.Id);

                  entity.Property(pb => pb.Id)
                        .HasDefaultValueSql("gen_random_uuid()");

                  entity.Property(pb => pb.ProjectId)
                        .IsRequired();

                  entity.HasIndex(pb => pb.ProjectId)
                        .IsUnique();

                  entity.Property(pb => pb.Overview)
                        .HasColumnType("text");

                  entity.Property(pb => pb.CreatedAt)
                        .HasDefaultValueSql("NOW()")
                        .IsRequired();

                  entity.Property(pb => pb.UpdatedAt)
                        .HasDefaultValueSql("NOW()")
                        .IsRequired();

                  // âœ… 1:1 Project <-> ProductBacklog
                  entity.HasOne(pb => pb.Project)
                        .WithOne(p => p.ProductBacklog)
                        .HasForeignKey<ProductBacklog>(pb => pb.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<Epic>(entity =>
            {
                entity.ToTable("epics");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn(); // Npgsql identity

                entity.Property(e => e.ProductBacklogId).IsRequired();

                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Description).HasColumnType("text");

                entity.Property(e => e.BusinessValue)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();
                
                entity.Property(e => e.Position)
                      .IsRequired();
                
                entity.Property(e => e.Priority).HasDefaultValue(0).IsRequired();
                entity.Property(e => e.Color).HasMaxLength(7).HasDefaultValue("#3498db").IsRequired();
                entity.Property(e => e.IsArchived)
                      .HasDefaultValue(false)
                      .IsRequired();

                entity.Property(e => e.ArchivedAt).IsRequired(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()").IsRequired();
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()").IsRequired();

                entity.HasIndex(e => new { e.ProductBacklogId, e.IsArchived });
                entity.HasIndex(e => new { e.ProductBacklogId, e.Position })
                      .IsUnique();
                entity.HasIndex(e => e.ProductBacklogId);

                entity.HasOne(e => e.ProductBacklog)
                    .WithMany(pb => pb.Epics)
                    .HasForeignKey(e => e.ProductBacklogId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasCheckConstraint("CK_Epic_BusinessValue", "\"BusinessValue\" IN ('High','Medium','Low')");
                entity.HasCheckConstraint("CK_Epic_Status", "\"Status\" IN ('Draft','Active','Completed')");
            });

            modelBuilder.Entity<UserStory>(entity =>
            {
                entity.ToTable("user_stories");
                entity.HasKey(us => us.Id);

                entity.Property(us => us.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();

                entity.Property(us => us.EpicId).IsRequired();

                entity.Property(us => us.Title).HasMaxLength(255).IsRequired();
                entity.Property(us => us.Persona).HasMaxLength(100).IsRequired();

                entity.Property(us => us.Description).HasColumnType("text").IsRequired();
                entity.Property(us => us.AcceptanceCriteria).HasColumnType("text");

                entity.Property(us => us.Complexity)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(us => us.Effort);
                entity.Property(us => us.Dependencies).HasColumnType("text");

                entity.Property(us => us.Priority).HasDefaultValue(0).IsRequired();

                entity.Property(us => us.BusinessValue)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(us => us.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(us => us.Position)
                      .IsRequired();

                entity.Property(us => us.AssigneeId);
                entity.Property(e => e.IsArchived)
                      .HasDefaultValue(false)
                      .IsRequired();

                entity.Property(e => e.ArchivedAt).IsRequired(false);
                entity.Property(us => us.CreatedAt).HasDefaultValueSql("NOW()").IsRequired();
                entity.Property(us => us.UpdatedAt).HasDefaultValueSql("NOW()").IsRequired();

                entity.HasIndex(s => new { s.EpicId, s.Position })
                      .IsUnique();

                entity.HasIndex(s => new { s.EpicId, s.IsArchived });
                entity.HasIndex(s => s.EpicId);

                entity.HasOne(us => us.Epic)
                    .WithMany(e => e.UserStories)
                    .HasForeignKey(us => us.EpicId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasCheckConstraint("CK_UserStory_Complexity", "\"Complexity\" IN ('Low','Medium','High','VeryHigh')");
                entity.HasCheckConstraint("CK_UserStory_BusinessValue", "\"BusinessValue\" IN ('High','Medium','Low')");
                entity.HasCheckConstraint("CK_UserStory_Status", "\"Status\" IN ('Draft','Ready','InProgress','Done')");
            });

            modelBuilder.Entity<Board>(entity =>
            {
                entity.ToTable("boards");
                entity.HasKey(b => b.Id);

                entity.Property(b => b.ProjectId).IsRequired();
                entity.Property(b => b.SprintId).IsRequired();

                entity.Property(b => b.Name).HasMaxLength(255).IsRequired();
                entity.Property(b => b.Description).HasColumnType("text");

                entity.Property(b => b.BoardType)
                      .HasConversion<string>()
                      .HasMaxLength(30)
                      .HasDefaultValue(BoardType.Kanban)
                      .IsRequired();

                entity.Property(b => b.CreatedAt).HasDefaultValueSql("NOW()").IsRequired();
                entity.Property(b => b.UpdatedAt).HasDefaultValueSql("NOW()").IsRequired();

                entity.HasOne(b => b.Project)
                      .WithMany()
                      .HasForeignKey(b => b.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Sprint)
                      .WithOne(s => s.Board)
                      .HasForeignKey<Board>(b => b.SprintId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(b => b.SprintId)
                      .IsUnique();
            });

            modelBuilder.Entity<SprintItem>(entity =>
            {
                entity.ToTable("sprint_items");
                entity.HasKey(si => si.Id);

                entity.Property(si => si.Id)
                      .ValueGeneratedOnAdd()
                      .UseIdentityByDefaultColumn(); 

                entity.Property(si => si.SprintId).IsRequired();
                entity.Property(si => si.UserStoryId).IsRequired();

                entity.Property(si => si.AddedAt)
                      .HasDefaultValueSql("NOW()")
                      .IsRequired();

                entity.HasOne(si => si.Sprint)
                      .WithMany(s => s.Items)
                      .HasForeignKey(si => si.SprintId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(si => si.UserStory)
                      .WithMany()
                      .HasForeignKey(si => si.UserStoryId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(si => new { si.SprintId, si.UserStoryId })
                      .IsUnique();
            });


            modelBuilder.Entity<Sprint>(entity =>
            {
                entity.ToTable("sprints");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.ProjectId).IsRequired();

                entity.Property(s => s.Name).HasMaxLength(255).IsRequired();
                entity.Property(s => s.Goal).HasColumnType("text");

                entity.Property(s => s.StartDate).HasColumnType("date").IsRequired();
                entity.Property(s => s.EndDate).HasColumnType("date").IsRequired();

                entity.Property(s => s.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .HasDefaultValue(SprintStatus.Planned)
                      .IsRequired();

                entity.Property(s => s.CapacityHours).HasDefaultValue(0).IsRequired();

                entity.Property(s => s.IsArchived).HasDefaultValue(false).IsRequired();
                entity.Property(s => s.ArchivedAt).IsRequired(false);

                // se você seta no domínio, pode remover o default SQL (recomendado),
                // mas aqui vou manter só o que você pediu: remover gen_random_uuid.
                entity.Property(s => s.CreatedAt).HasDefaultValueSql("NOW()").IsRequired();
                entity.Property(s => s.UpdatedAt).HasDefaultValueSql("NOW()").IsRequired();

                entity.HasOne(s => s.Project)
                      .WithMany()
                      .HasForeignKey(s => s.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasCheckConstraint("CK_Sprint_DateRange", "\"StartDate\" < \"EndDate\"");
                entity.HasCheckConstraint("CK_Sprint_Status", "\"Status\" IN ('Planned','Active','Closed','Cancelled')");

                entity.HasIndex(s => s.ProjectId)
                      .IsUnique()
                      .HasFilter("\"Status\" = 'Active' AND \"IsArchived\" = false");
            });
        }
    }
}

