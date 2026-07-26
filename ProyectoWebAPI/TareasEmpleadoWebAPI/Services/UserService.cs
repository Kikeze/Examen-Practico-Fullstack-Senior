using Microsoft.EntityFrameworkCore;

using TareasEmpleadoWebAPI.Data;
using TareasEmpleadoWebAPI.DTOs;
using TareasEmpleadoWebAPI.Interfaces;

namespace TareasEmpleadoWebAPI.Services;

public sealed class UserService(ApplicationDbContext context) : IUserService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IReadOnlyList<ActiveUserDto>> GetActiveAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(user => user.IsActive)
            .OrderBy(user => user.FullName)
            .Select(user => new ActiveUserDto
            {
                UserId = user.UserId,
                FullName = user.FullName
            })
            .ToListAsync();
    }
}