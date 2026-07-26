using TareasEmpleadoWebAPI.DTOs;

namespace TareasEmpleadoWebAPI.Interfaces;

public interface IReportService
{
    Task<PagedResultDto<PendingTaskReportDto>> GetPendingTasksAsync(PendingTaskReportQueryDto query);
}
