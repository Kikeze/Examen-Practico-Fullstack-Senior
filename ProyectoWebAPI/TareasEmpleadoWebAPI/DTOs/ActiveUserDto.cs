namespace TareasEmpleadoWebAPI.DTOs;

public sealed class ActiveUserDto
{
    public int UserId { get; init; }

    public string FullName { get; init; } = string.Empty;
}
