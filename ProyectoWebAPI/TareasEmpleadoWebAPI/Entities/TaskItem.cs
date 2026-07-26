using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TareasEmpleadoWebAPI.Enums;

namespace TareasEmpleadoWebAPI.Entities;

[Table("Tasks")]
public class TaskItem
{
    [Key]
    public int TaskId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public TaskItemPriority Priority { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime DueDate { get; set; }

    public TaskItemStatus Status { get; set; }

    public int UserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
