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
            entity.ToTable("Tasks", tableBuilder =>
            {
                tableBuilder.UseSqlOutputClause(false);
            });

            entity.HasKey(task => task.TaskId);

            entity.Property(task => task.Title)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(task => task.Description)
                .HasMaxLength(500);

            entity.Property(task => task.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(task => task.DeletedDate);

            entity.HasIndex(task => new
                {
                    task.UserId,
                    task.Title
                })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_Tasks_User_Title");

            entity.HasQueryFilter(task => !task.IsDeleted);
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

