using Microsoft.EntityFrameworkCore;
using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureWorkspace(modelBuilder);
        ConfigureWorkspaceMember(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureTaskItem(modelBuilder);
        ConfigureComment(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .IsRequired();

            entity.Property(u => u.IsActive)
                .IsRequired();
        });
    }

    private static void ConfigureWorkspace(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(w => w.Id);

            entity.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(w => w.Description)
                .HasMaxLength(500);

            entity.Property(w => w.CreatedAt)
                .IsRequired();

            entity.Property(w => w.IsActive)
                .IsRequired();

            entity.HasOne(w => w.Owner)
                .WithMany()
                .HasForeignKey(w => w.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureWorkspaceMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkspaceMember>(entity =>
        {
            entity.HasKey(wm => wm.Id);

            entity.Property(wm => wm.Role)
                .IsRequired();

            entity.Property(wm => wm.JoinedAt)
                .IsRequired();

            entity.Property(wm => wm.IsActive)
                .IsRequired();

            entity.HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wm => wm.User)
                .WithMany(u => u.WorkspaceMemberships)
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(wm => new { wm.WorkspaceId, wm.UserId })
                .IsUnique();
        });
    }

    private static void ConfigureProject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.Description)
                .HasMaxLength(500);

            entity.Property(p => p.Status)
                .IsRequired();

            entity.Property(p => p.CreatedAt)
                .IsRequired();

            entity.HasOne(p => p.Workspace)
                .WithMany(w => w.Projects)
                .HasForeignKey(p => p.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTaskItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Description)
                .HasMaxLength(1000);

            entity.Property(t => t.Status)
                .IsRequired();

            entity.Property(t => t.Priority)
                .IsRequired();

            entity.Property(t => t.CreatedAt)
                .IsRequired();

            entity.Property(t => t.IsActive)
                .IsRequired();

            entity.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.CreatedByUser)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureComment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(c => c.CreatedAt)
                .IsRequired();

            entity.Property(c => c.IsActive)
                .IsRequired();

            entity.HasOne(c => c.TaskItem)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}