using Microsoft.AspNetCore.Mvc;

using TareasEmpleadoWebAPI.DTOs;
using TareasEmpleadoWebAPI.Interfaces;

namespace TareasEmpleadoWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<TaskDto>>> GetAllAsync([FromQuery] TaskQueryDto query)
    {
        var tasks = await _taskService.GetAllAsync(query);

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
