using TareasEmpleadoWebAPI.DTOs;

namespace TareasEmpleadoWebAPI.Interfaces;

public interface ITaskService
{
    Task<PagedResultDto<TaskDto>> GetAllAsync(TaskQueryDto query);

    Task<TaskDto?> GetByIdAsync(int id);

    Task<TaskDto> CreateAsync(CreateTaskDto dto);

    Task<bool> UpdateAsync(int id, UpdateTaskDto dto);

    Task<bool> DeleteAsync(int id);
}
