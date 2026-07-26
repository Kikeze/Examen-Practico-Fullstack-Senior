using Microsoft.AspNetCore.Mvc;

using TareasEmpleadoWebAPI.DTOs;
using TareasEmpleadoWebAPI.Enums;
using TareasEmpleadoWebAPI.Interfaces;

namespace TareasEmpleadoWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllAsync(
            [FromQuery] TaskItemPriority? priority, [FromQuery] TaskItemStatus? status,
            [FromQuery] int? userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,[FromQuery] int pageSize = 20)
    {
        var tasks = await _taskService.GetAllAsync(
            priority, status,
            userId, startDate, endDate,
            page, pageSize);

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetByIdAsync(int id)
    {
        var task = await _taskService.GetByIdAsync(id);

        if (task is null)
        {
            return NotFound(new ErrorResponseDto
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"No se encontro la tarea con identificador {id}."
            });
        }

        return Ok(task);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> CreateAsync([FromBody] CreateTaskDto dto)
    {
        var task = await _taskService.CreateAsync(dto);

        return Created(
            $"/api/tasks/{task.TaskId}",
            task);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateTaskDto dto)
    {
        var updated = await _taskService.UpdateAsync(id, dto);

        if (!updated)
        {
            return NotFound(new ErrorResponseDto
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"No se encontro la tarea con identificador {id}."
            });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var deleted = await _taskService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ErrorResponseDto
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"No se encontro la tarea con identificador {id}."
            });
        }

        return NoContent();
    }

}
