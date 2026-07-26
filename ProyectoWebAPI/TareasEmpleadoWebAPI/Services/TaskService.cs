using Microsoft.EntityFrameworkCore;

using TareasEmpleadoWebAPI.Data;
using TareasEmpleadoWebAPI.DTOs;
using TareasEmpleadoWebAPI.Entities;
using TareasEmpleadoWebAPI.Enums;
using TareasEmpleadoWebAPI.Interfaces;

namespace TareasEmpleadoWebAPI.Services;

public class TaskService(ApplicationDbContext context) : ITaskService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PagedResultDto<TaskDto>> GetAllAsync(TaskQueryDto query)
    {
        var tasksQuery = _context.Tasks
            .Include(t => t.User)
            .AsNoTracking()
            .AsQueryable();

        if (query.Priority.HasValue)
        {
            tasksQuery = tasksQuery.Where(task => task.Priority == query.Priority.Value);
        }

        if (query.Status.HasValue)
        {
            tasksQuery = tasksQuery.Where(task => task.Status == query.Status.Value);
        }

        if (query.UserId.HasValue)
        {
            tasksQuery = tasksQuery.Where(task => task.UserId == query.UserId.Value);
        }

        if (query.StartDate.HasValue)
        {
            var startDate = query.StartDate.Value.ToDateTime(TimeOnly.MinValue);

            tasksQuery = tasksQuery.Where(task => task.CreatedDate >= startDate);
        }

        if (query.EndDate.HasValue)
        {
            var exclusiveEndDate = query.EndDate.Value
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            tasksQuery = tasksQuery.Where(task => task.CreatedDate < exclusiveEndDate);
        }

        var totalRecords = await tasksQuery.CountAsync();

        var totalPages = totalRecords == 0
            ? 0
            : (int)Math.Ceiling(totalRecords / (double)query.PageSize);

        var items = await tasksQuery
            .OrderByDescending(task => task.TaskId)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(task => MapToDto(task))
            .ToListAsync();

        return new PagedResultDto<TaskDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    public async Task<TaskDto?> GetByIdAsync(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TaskId == id);

        return task is null ? null : MapToDto(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto)
    {
        await ValidateUserAsync(dto.UserId);
        await ValidateDueDateAsync(dto.DueDate);
        await ValidateActiveUserAsync(dto.UserId);
        await ValidateDuplicateTitleAsync(dto.Title, dto.UserId);

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Priority = dto.Priority,
            CreatedDate = DateTime.Now,
            DueDate = dto.DueDate,
            Status = TaskItemStatus.Pending,
            UserId = dto.UserId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        await _context.Entry(task)
            .Reference(t => t.User)
            .LoadAsync();

        return MapToDto(task);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskDto dto)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.TaskId == id);

        if (task is null)
        {
            return false;
        }

        await ValidateUserAsync(dto.UserId);
        await ValidateDueDateAsync(dto.DueDate);
        await ValidateActiveUserAsync(dto.UserId);
        await ValidateDuplicateTitleAsync(dto.Title, dto.UserId, id);
        await ValidateStatusTransitionAsync(task.Status, dto.Status);

        var previousStatus = task.Status;
        var statusChangeDate = DateTime.Now;

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.Status = dto.Status;
        task.UserId = dto.UserId;

        if (previousStatus != dto.Status)
        {
            if (dto.Status == TaskItemStatus.InProgress &&
                task.StartDate is null)
            {
                task.StartDate = statusChangeDate;
            }

            if (dto.Status == TaskItemStatus.Completed)
            {
                task.StartDate ??= statusChangeDate;
                task.EndDate = statusChangeDate;
            }

            if (previousStatus == TaskItemStatus.Completed &&
                dto.Status == TaskItemStatus.InProgress)
            {
                task.EndDate = null;
            }
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(task => task.TaskId == id);

        if (task is null)
        {
            return false;
        }

        task.IsDeleted = true;
        task.DeletedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }

    private static TaskDto MapToDto(TaskItem entity)
    {
        return new TaskDto
        {
            TaskId = entity.TaskId,
            Title = entity.Title,
            Description = entity.Description,
            Priority = entity.Priority,
            CreatedDate = entity.CreatedDate,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            DueDate = entity.DueDate,
            Status = entity.Status,
            UserId = entity.UserId,
            UserName = entity.User?.FullName ?? string.Empty
        };
    }

    private Task ValidateDueDateAsync(DateTime dueDate)
    {
        if (dueDate.Date < DateTime.Now.Date)
        {
            throw new ArgumentException(
                "La fecha limite no puede ser menor a la fecha actual.");
        }

        return Task.CompletedTask;
    }

    private async Task ValidateUserAsync(int userId)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserId == userId && u.IsActive);

        if (!userExists)
        {
            throw new ArgumentException(
                "El usuario seleccionado no existe o esta inactivo.");
        }
    }

    private async Task ValidateDuplicateTitleAsync(string title, int userId, int? taskId = null)
    {
        var normalizedTitle = title.Trim();

        var duplicateExists = await _context.Tasks
            .AsNoTracking()
            .AnyAsync(t =>
                t.UserId == userId &&
                t.Title == normalizedTitle &&
                (!taskId.HasValue || t.TaskId != taskId.Value));

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "Una tarea con el mismo titulo ya existe para este usuario.");
        }
    }

    private Task ValidateStatusTransitionAsync(TaskItemStatus currentStatus, TaskItemStatus newStatus)
    {
        var isValidTransition = currentStatus switch
        {
            TaskItemStatus.Pending =>
                newStatus is TaskItemStatus.Pending
                    or TaskItemStatus.InProgress
                    or TaskItemStatus.Completed,
            TaskItemStatus.InProgress =>
                newStatus is TaskItemStatus.InProgress
                    or TaskItemStatus.Completed,
            TaskItemStatus.Completed =>
                newStatus is TaskItemStatus.Completed
                    or TaskItemStatus.InProgress,

            _ => false
        };

        if (!isValidTransition)
        {
            throw new InvalidOperationException(
                $"El estado no puede cambiar de {currentStatus} a {newStatus}.");
        }

        return Task.CompletedTask;
    }

    private async Task ValidateActiveUserAsync(int userId)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(user => user.UserId == userId && user.IsActive);

        if (!userExists)
        {
            throw new InvalidOperationException(
                "El usuario seleccionado no existe o se encuentra inactivo.");
        }
    }
}
