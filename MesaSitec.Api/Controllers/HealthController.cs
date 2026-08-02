using MesaSitec.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class HealthController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("health")]
    public ActionResult<HealthResponseDto> Health() => Ok(new HealthResponseDto("ok"));
}
