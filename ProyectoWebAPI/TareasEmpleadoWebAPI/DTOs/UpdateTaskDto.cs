using System.ComponentModel.DataAnnotations;

using TareasEmpleadoWebAPI.Enums;

namespace TareasEmpleadoWebAPI.DTOs;

public class UpdateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public TaskItemPriority Priority { get; set; }

    public TaskItemStatus Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime DueDate { get; set; }

    public int UserId { get; set; }
}
