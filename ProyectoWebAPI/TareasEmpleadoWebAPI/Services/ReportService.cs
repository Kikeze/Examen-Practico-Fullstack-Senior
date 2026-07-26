using Microsoft.EntityFrameworkCore;

using TareasEmpleadoWebAPI.Data;
using TareasEmpleadoWebAPI.DTOs;
using TareasEmpleadoWebAPI.Interfaces;

namespace TareasEmpleadoWebAPI.Services;

public class ReportService(ApplicationDbContext context) : IReportService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PagedResultDto<PendingTaskReportDto>> GetPendingTasksAsync(PendingTaskReportQueryDto query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var report = await _context.Database
            .SqlQueryRaw<PendingTaskReportDto>("EXEC dbo.sp_GetPendingTasks")
            .ToListAsync();
        
        return new PagedResultDto<PendingTaskReportDto>
        {
            Items = [.. report
                .Skip((page - 1) * pageSize)
                .Take(pageSize)],
            Page = page,
            PageSize = pageSize,
            TotalRecords = report.Count,
            TotalPages = (int)Math.Ceiling(report.Count / (double)pageSize),
        };
    }
}
