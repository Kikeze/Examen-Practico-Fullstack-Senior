using System.ComponentModel.DataAnnotations;

namespace TareasEmpleadoWebAPI.DTOs;

public class PendingTaskReportQueryDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

}
