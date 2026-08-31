using MesaSitec.Application.DTOs;
using MesaSitec.Domain.Enums;
using MesaSitec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Application.Services;

public interface IAgenteService
{
    Task<IReadOnlyList<AgenteResumenDto>> ListarAsync(CurrentUser user, string? q);
}

public class AgenteService(AppDbContext context) : IAgenteService
{
    public async Task<IReadOnlyList<AgenteResumenDto>> ListarAsync(CurrentUser user, string? q)
    {
        var query = context.Usuarios
            .Where(u => u.TenantId == user.TenantId)
            .Where(u => u.Activo)
            .Where(u => u.Rol == Rol.Admin || u.Rol == Rol.Agente);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(u => u.Nombre.ToLower().Contains(term));
        }

        var agentes = await query
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        return agentes.Select(u => new AgenteResumenDto(u.Id, u.Nombre)).ToList();
    }
}
