using System.ComponentModel.DataAnnotations;

using TareasEmpleadoWebAPI.Enums;

namespace TareasEmpleadoWebAPI.DTOs;

public sealed class TaskQueryDto : IValidatableObject
{
    public TaskItemPriority? Priority { get; init; }

    public TaskItemStatus? Status { get; init; }

    [Range(1, int.MaxValue)]
    public int? UserId { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
        {
            yield return new ValidationResult(
                "La fecha inicial no puede ser mayor que la fecha final.",
                [
                    nameof(StartDate),
                    nameof(EndDate)
                ]
            );
        }
    }
}
