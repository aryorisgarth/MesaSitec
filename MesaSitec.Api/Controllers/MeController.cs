using MesaSitec.Api.Extensions;
using MesaSitec.Application.DTOs;
using MesaSitec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class MeController(IAuthService authService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioDto>> GetMe()
    {
        var user = User.GetCurrentUser();
        return Ok(await authService.GetMeAsync(user));
    }
}
