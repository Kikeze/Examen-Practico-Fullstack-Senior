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
    [ProducesResponseType(typeof(PagedResultDto<PendingTaskReportDto>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<PendingTaskReportDto>>> GetPendingTasks([FromQuery] PendingTaskReportQueryDto query)
    {
        var result = await _reportService.GetPendingTasksAsync(query);

        return Ok(result);
    }
}
