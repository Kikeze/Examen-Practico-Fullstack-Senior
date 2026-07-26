using Microsoft.AspNetCore.Mvc;

using TareasEmpleadoWebAPI.DTOs;
using TareasEmpleadoWebAPI.Interfaces;

namespace TareasEmpleadoWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("active")]
    [ProducesResponseType(typeof(IReadOnlyList<ActiveUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActiveUserDto>>>GetActive()
    {
        var users = await _userService.GetActiveAsync();

        return Ok(users);
    }
}
