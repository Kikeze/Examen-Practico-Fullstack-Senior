using TareasEmpleadoWebAPI.DTOs;
using TareasEmpleadoWebAPI.Enums;

namespace TareasEmpleadoWebAPI.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync(
        TaskItemPriority? priority,
        TaskItemStatus? status,
        int? userId,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize);

    Task<TaskDto?> GetByIdAsync(int id);

    Task<TaskDto> CreateAsync(CreateTaskDto dto);

    Task<bool> UpdateAsync(int id, UpdateTaskDto dto);

    Task<bool> DeleteAsync(int id);
}
