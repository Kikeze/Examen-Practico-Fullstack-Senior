using Microsoft.EntityFrameworkCore;

using TareasEmpleadoWebAPI.Data;
using TareasEmpleadoWebAPI.DTOs;
using TareasEmpleadoWebAPI.Interfaces;

namespace TareasEmpleadoWebAPI.Services;

public class ReportService(ApplicationDbContext context) : IReportService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<PendingTaskReportDto>> GetPendingTasksAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var report = await _context.Database
            .SqlQueryRaw<PendingTaskReportDto>(
                "EXEC dbo.sp_GetPendingTasks")
            .ToListAsync();

        return [.. report
            .Skip((page - 1) * pageSize)
            .Take(pageSize)];
    }
}
