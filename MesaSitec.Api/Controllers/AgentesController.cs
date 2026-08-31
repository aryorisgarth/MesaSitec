using MesaSitec.Api.Extensions;
using MesaSitec.Application.DTOs;
using MesaSitec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Route("api/v1/agentes")]
[Authorize]
public class AgentesController(IAgenteService agenteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AgenteResumenDto>>> Listar([FromQuery] string? q)
    {
        var user = User.GetCurrentUser();
        return Ok(await agenteService.ListarAsync(user, q));
    }
}
