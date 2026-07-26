using TareasEmpleadoWebAPI.DTOs;

namespace TareasEmpleadoWebAPI.Interfaces;

public interface IReportService
{
    Task<IEnumerable<PendingTaskReportDto>> GetPendingTasksAsync(int page, int pageSize);
}
