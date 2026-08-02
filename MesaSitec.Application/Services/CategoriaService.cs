using MesaSitec.Application.DTOs;
using MesaSitec.Application.Exceptions;
using MesaSitec.Application.Services;
using MesaSitec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Application.Services;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaDto>> ListarActivasAsync(CurrentUser user);
}

public class CategoriaService(AppDbContext context) : ICategoriaService
{
    public async Task<IReadOnlyList<CategoriaDto>> ListarActivasAsync(CurrentUser user)
    {
        return await context.Categorias
            .Where(c => c.TenantId == user.TenantId && c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto(c.Id, c.Nombre, c.SlaHoras))
            .ToListAsync();
    }
}
