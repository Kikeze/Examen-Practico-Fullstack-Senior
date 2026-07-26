using System.ComponentModel.DataAnnotations;

using TareasEmpleadoWebAPI.Enums;

namespace TareasEmpleadoWebAPI.DTOs;

public class CreateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public TaskItemPriority Priority { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public int UserId { get; set; }
}
