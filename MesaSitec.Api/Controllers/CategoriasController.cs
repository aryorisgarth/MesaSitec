using MesaSitec.Api.Extensions;
using MesaSitec.Application.DTOs;
using MesaSitec.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Route("api/v1/categorias")]
[Authorize]
public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Listar()
    {
        var user = User.GetCurrentUser();
        return Ok(await categoriaService.ListarActivasAsync(user));
    }
}
