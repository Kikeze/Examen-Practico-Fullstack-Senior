using TareasEmpleadoWebAPI.DTOs;

namespace TareasEmpleadoWebAPI.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<ActiveUserDto>> GetActiveAsync();
}
