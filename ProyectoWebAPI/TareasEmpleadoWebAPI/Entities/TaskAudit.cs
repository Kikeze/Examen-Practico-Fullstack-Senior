using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TareasEmpleadoWebAPI.Enums;

namespace TareasEmpleadoWebAPI.Entities;

[Table("TaskAudit")]
public class TaskAudit
{
    [Key]
    public int AuditId { get; set; }

    public int TaskId { get; set; }

    public TaskItemStatus OldStatus { get; set; }

    public TaskItemStatus NewStatus { get; set; }

    public DateTime ChangeDate { get; set; }

    [ForeignKey(nameof(TaskId))]
    public TaskItem? Task { get; set; }
}
