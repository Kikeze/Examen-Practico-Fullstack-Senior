using TareasEmpleadoWebAPI.Enums;

namespace TareasEmpleadoWebAPI.DTOs;

public class TaskDto
{
    public int TaskId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemPriority Priority { get; set; }

    public TaskItemStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime DueDate { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;
}
