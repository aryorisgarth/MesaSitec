using MesaSitec.Api.Extensions;
using MesaSitec.Application.DTOs;
using MesaSitec.Application.Exceptions;
using MesaSitec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Route("api/v1/solicitudes")]
[Authorize]
public class SolicitudesController(ISolicitudService solicitudService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponseDto<SolicitudListItemDto>>> Listar(
        [FromQuery] string? estado,
        [FromQuery] string? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? agenteNombre,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = null)
    {
        var user = User.GetCurrentUser();
        var query = new SolicitudQuery(estado, prioridad, categoriaId, agenteId, agenteNombre, q, vencidas, page, pageSize, sort);
        return Ok(await solicitudService.ListarAsync(user, query));
    }

    [HttpPost]
    public async Task<ActionResult<SolicitudDetalleDto>> Crear([FromBody] CrearSolicitudDto? dto)
    {
        if (dto is null)
            throw AppException.Validacion("Hay errores de validación en la solicitud.", new Dictionary<string, string[]>
            {
                ["titulo"] = ["El título es requerido."],
            });

        var user = User.GetCurrentUser();
        var result = await solicitudService.CrearAsync(user, dto);
        return Created($"/api/v1/solicitudes/{result.Id}", result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SolicitudDetalleDto>> Obtener(Guid id)
    {
        var user = User.GetCurrentUser();
        return Ok(await solicitudService.ObtenerAsync(user, id));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SolicitudDetalleDto>> Editar(Guid id, [FromBody] EditarSolicitudDto dto)
    {
        var user = User.GetCurrentUser();
        return Ok(await solicitudService.EditarAsync(user, id, dto));
    }

    [HttpPost("{id:guid}/transiciones")]
    public async Task<ActionResult<SolicitudDetalleDto>> Transicionar(Guid id, [FromBody] TransicionDto dto)
    {
        var user = User.GetCurrentUser();
        return Ok(await solicitudService.TransicionarAsync(user, id, dto));
    }
}
