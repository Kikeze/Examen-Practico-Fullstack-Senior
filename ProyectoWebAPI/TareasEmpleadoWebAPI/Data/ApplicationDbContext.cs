using Microsoft.EntityFrameworkCore;

using TareasEmpleadoWebAPI.Entities;

namespace TareasEmpleadoWebAPI.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<TaskAudit> TaskAudits => Set<TaskAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);

            entity.HasMany(e => e.Tasks)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(e => e.TaskId);

            entity.Property(e => e.Priority)
                  .HasConversion<int>();

            entity.Property(e => e.Status)
                  .HasConversion<int>();
        });

        modelBuilder.Entity<TaskAudit>(entity =>
        {
            entity.ToTable("TaskAudit");
            entity.HasKey(e => e.AuditId);

            entity.Property(e => e.OldStatus)
                  .HasConversion<int>();

            entity.Property(e => e.NewStatus)
                  .HasConversion<int>();
        });
    }
}

