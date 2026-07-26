using Microsoft.AspNetCore.Mvc;

using TareasEmpleadoWebAPI.DTOs;
using TareasEmpleadoWebAPI.Interfaces;

namespace TareasEmpleadoWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
public class ReportsController(IReportService reportService) : ControllerBase
{
    private readonly IReportService _reportService = reportService;

    [HttpGet("pending-tasks")]
    [ProducesResponseType(typeof(IEnumerable<PendingTaskReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PendingTaskReportDto>>>GetPendingTasksAsync(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var report = await _reportService.GetPendingTasksAsync(page, pageSize);

        return Ok(report);
    }
}
