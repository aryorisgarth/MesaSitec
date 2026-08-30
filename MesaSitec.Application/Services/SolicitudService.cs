using MesaSitec.Application.DTOs;
using MesaSitec.Application.Exceptions;
using MesaSitec.Application.Services;
using MesaSitec.Domain.Entities;
using MesaSitec.Domain.Enums;
using MesaSitec.Domain.Services;
using MesaSitec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Application.Services;

public interface ISolicitudService
{
    Task<PaginatedResponseDto<SolicitudListItemDto>> ListarAsync(CurrentUser user, SolicitudQuery query);
    Task<SolicitudDetalleDto> ObtenerAsync(CurrentUser user, Guid id);
    Task<SolicitudDetalleDto> CrearAsync(CurrentUser user, CrearSolicitudDto dto);
    Task<SolicitudDetalleDto> EditarAsync(CurrentUser user, Guid id, EditarSolicitudDto dto);
    Task<SolicitudDetalleDto> TransicionarAsync(CurrentUser user, Guid id, TransicionDto dto);
}

public record SolicitudQuery(
    string? Estado,
    string? Prioridad,
    Guid? CategoriaId,
    Guid? AgenteId,
    string? Q,
    bool? Vencidas,
    int Page,
    int PageSize,
    string? Sort);

public class SolicitudService(AppDbContext context) : ISolicitudService
{
    public async Task<PaginatedResponseDto<SolicitudListItemDto>> ListarAsync(CurrentUser user, SolicitudQuery query)
    {
        ValidarPaginacion(query.Page, query.PageSize);

        var q = context.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Where(s => s.TenantId == user.TenantId);

        if (!SolicitudPermissions.PuedeListarTodas(user.Rol))
            q = q.Where(s => s.SolicitanteId == user.Id);

        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            var estado = EnumMapper.ParseEstado(query.Estado)
                ?? throw AppException.ParametroInvalido("El parámetro estado no es válido.");
            q = q.Where(s => s.Estado == estado);
        }

        if (!string.IsNullOrWhiteSpace(query.Prioridad))
        {
            if (!Enum.TryParse<Prioridad>(query.Prioridad, ignoreCase: true, out var prioridad))
                throw AppException.ParametroInvalido("El parámetro prioridad no es válido.");
            q = q.Where(s => s.Prioridad == prioridad);
        }

        if (query.CategoriaId.HasValue)
            q = q.Where(s => s.CategoriaId == query.CategoriaId.Value);

        if (query.AgenteId.HasValue)
            q = q.Where(s => s.AgenteId == query.AgenteId.Value);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim().ToLower();
            q = q.Where(s =>
                s.Titulo.ToLower().Contains(term) ||
                s.Descripcion.ToLower().Contains(term) ||
                s.Agente.Nombre.ToLower().Contains(term) ||
                s.Codigo.ToLower().Contains(term));
        }

  

        if (query.Vencidas == true)
        {
            var now = DateTime.UtcNow;
            q = q.Where(s =>
                s.FechaLimiteSla < now &&
                s.Estado != EstadoSolicitud.Resuelta &&
                s.Estado != EstadoSolicitud.Cerrada &&
                s.Estado != EstadoSolicitud.Cancelada);
        }

        var total = await q.CountAsync();
        q = AplicarOrden(q, query.Sort ?? "-fechaCreacion");

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = items.Select(s => MapListItem(s, DateTime.UtcNow)).ToList();
        var totalPaginas = (int)Math.Ceiling(total / (double)query.PageSize);

        return new PaginatedResponseDto<SolicitudListItemDto>(
            dtos, query.Page, query.PageSize, total, totalPaginas);
    }

    public async Task<SolicitudDetalleDto> ObtenerAsync(CurrentUser user, Guid id)
    {
        var solicitud = await ObtenerSolicitudTenantAsync(user, id, incluirSolicitante: true);
        return MapDetalle(solicitud, DateTime.UtcNow);
    }

    public async Task<SolicitudDetalleDto> CrearAsync(CurrentUser user, CrearSolicitudDto dto)
    {
        if (!SolicitudPermissions.PuedeCrear(user.Rol))
            throw AppException.OperacionNoPermitida("No puede crear solicitudes.");

        ValidarCampos(dto.Titulo, dto.Descripcion, dto.CategoriaId, dto.Prioridad, out var prioridad);
        var titulo = dto.Titulo?.Trim() ?? string.Empty;
        var descripcion = dto.Descripcion?.Trim() ?? string.Empty;

        var categoria = await context.Categorias
            .FirstOrDefaultAsync(c => c.Id == dto.CategoriaId && c.TenantId == user.TenantId && c.Activo)
            ?? throw AppException.RecursoNoEncontrado("La categoría no existe.");

        var ahora = DateTime.UtcNow;
        var correlativo = await ObtenerSiguienteCorrelativoAsync(user.TenantId, ahora.Year);

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = user.TenantId,
            Codigo = CodigoGenerator.Generar(correlativo, ahora),
            Titulo = titulo,
            Descripcion = descripcion,
            CategoriaId = categoria.Id,
            Prioridad = prioridad,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = user.Id,
            FechaCreacion = ahora,
            FechaLimiteSla = SlaCalculator.CalcularFechaLimite(ahora, categoria.SlaHoras, prioridad),
        };

        context.Solicitudes.Add(solicitud);

        for (var intento = 0; intento < 3; intento++)
        {
            try
            {
                await context.SaveChangesAsync();
                break;
            }
            catch (DbUpdateException) when (intento < 2)
            {
                correlativo = await ObtenerSiguienteCorrelativoAsync(user.TenantId, ahora.Year);
                solicitud.Codigo = CodigoGenerator.Generar(correlativo, ahora);
            }
        }

        return MapDetalle(await CargarDetalleAsync(solicitud.Id), DateTime.UtcNow);
    }

    public async Task<SolicitudDetalleDto> EditarAsync(CurrentUser user, Guid id, EditarSolicitudDto dto)
    {
        var solicitud = await ObtenerSolicitudTenantAsync(user, id);

        if (!SolicitudPermissions.PuedeEditar(user.Rol, solicitud.SolicitanteId, user.Id, solicitud.Estado))
            throw AppException.OperacionNoPermitida("No puede editar esta solicitud.");

        ValidarCampos(dto.Titulo, dto.Descripcion, dto.CategoriaId, dto.Prioridad, out var prioridad);

        var categoria = await context.Categorias
            .FirstOrDefaultAsync(c => c.Id == dto.CategoriaId && c.TenantId == user.TenantId && c.Activo)
            ?? throw AppException.RecursoNoEncontrado("La categoría no existe.");

        var recalcularSla = solicitud.Estado is not (EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada or EstadoSolicitud.Cancelada)
            && (solicitud.Prioridad != prioridad || solicitud.CategoriaId != categoria.Id);

        solicitud.Titulo = dto.Titulo.Trim();
        solicitud.Descripcion = dto.Descripcion.Trim();
        solicitud.CategoriaId = categoria.Id;
        solicitud.Prioridad = prioridad;

        if (recalcularSla)
            solicitud.FechaLimiteSla = SlaCalculator.CalcularFechaLimite(solicitud.FechaCreacion, categoria.SlaHoras, prioridad);

        await context.SaveChangesAsync();
        return MapDetalle(await CargarDetalleAsync(solicitud.Id), DateTime.UtcNow);
    }

    public async Task<SolicitudDetalleDto> TransicionarAsync(CurrentUser user, Guid id, TransicionDto dto)
    {
        var solicitud = await ObtenerSolicitudTenantAsync(user, id);
        var accion = dto.Accion.Trim().ToLowerInvariant();

        if (!SolicitudPermissions.PuedeEjecutarAccion(user.Rol, accion))
            throw AppException.OperacionNoPermitida($"El rol no permite la acción '{accion}'.");

        if (!SolicitudPermissions.PuedeVerDetalle(user.Rol, solicitud.SolicitanteId, user.Id) && user.Rol == Rol.Solicitante)
            throw AppException.OperacionNoPermitida("No puede actuar sobre esta solicitud.");

        if (accion == "cerrar" && user.Rol == Rol.Solicitante && solicitud.SolicitanteId != user.Id)
            throw AppException.OperacionNoPermitida("Solo puede cerrar sus propias solicitudes.");

        if (!SolicitudStateMachine.EsTransicionValida(solicitud.Estado, accion))
            throw AppException.TransicionInvalida($"No se puede aplicar '{accion}' sobre una solicitud en estado '{EnumMapper.ToApiString(solicitud.Estado)}'.");

        switch (accion)
        {
            case "asignar":
                await AsignarAsync(solicitud, dto.AgenteId);
                break;
            case "resolver":
                ValidarMotivo(dto.Motivo, 20, "resolver");
                solicitud.MotivoResolucion = dto.Motivo!.Trim();
                solicitud.FechaResolucion = DateTime.UtcNow;
                break;
            case "cancelar":
                ValidarMotivo(dto.Motivo, 10, "cancelar");
                solicitud.MotivoCancelacion = dto.Motivo!.Trim();
                break;
        }

        solicitud.Estado = SolicitudStateMachine.ObtenerNuevoEstado(solicitud.Estado, accion);
        await context.SaveChangesAsync();

        return MapDetalle(await CargarDetalleAsync(solicitud.Id), DateTime.UtcNow);
    }

    private async Task AsignarAsync(Solicitud solicitud, Guid? agenteId)
    {
        if (!agenteId.HasValue)
            throw AppException.AgenteInvalido("Debe indicar un agenteId.");

        var agente = await context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == agenteId.Value);

        if (agente is null
            || !agente.Activo
            || agente.TenantId != solicitud.TenantId
            || agente.Rol is not (Rol.Agente or Rol.Admin))
            throw AppException.AgenteInvalido();

        solicitud.AgenteId = agente.Id;
    }

    private static void ValidarMotivo(string? motivo, int minLength, string accion)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < minLength)
            throw AppException.MotivoRequerido($"La acción '{accion}' requiere un motivo de al menos {minLength} caracteres.");
    }

    private static void ValidarCampos(string? titulo, string? descripcion, Guid categoriaId, string? prioridadStr, out Prioridad prioridad)
    {
        var errores = new Dictionary<string, string[]>();
        titulo = titulo?.Trim() ?? string.Empty;
        descripcion = descripcion?.Trim() ?? string.Empty;
        prioridadStr ??= string.Empty;

        if (titulo.Length < 5 || titulo.Length > 120)
            errores["titulo"] = ["El título debe tener entre 5 y 120 caracteres."];

        if (descripcion.Length < 10 || descripcion.Length > 4000)
            errores["descripcion"] = ["La descripción debe tener entre 10 y 4000 caracteres."];

        if (categoriaId == Guid.Empty)
            errores["categoriaId"] = ["Debe indicar una categoría válida."];

        if (!Enum.TryParse<Prioridad>(prioridadStr, ignoreCase: true, out prioridad))
            errores["prioridad"] = ["La prioridad no es válida."];

        if (errores.Count > 0)
            throw AppException.Validacion("Hay errores de validación en la solicitud.", errores);
    }

    private static void ValidarPaginacion(int page, int pageSize)
    {
        if (page < 1 || pageSize > 100)
            throw AppException.ParametroInvalido("Los parámetros page y pageSize están fuera de rango.");
    }

    private async Task<int> ObtenerSiguienteCorrelativoAsync(Guid tenantId, int year)
    {
        var codigos = await context.Solicitudes
            .Where(s => s.TenantId == tenantId && s.Codigo.StartsWith($"SOL-{year}-"))
            .Select(s => s.Codigo)
            .ToListAsync();

        var max = 0;
        foreach (var codigo in codigos)
        {
            var partes = codigo.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var numero))
                max = Math.Max(max, numero);
        }

        return max + 1;
    }

    private async Task<Solicitud> ObtenerSolicitudTenantAsync(CurrentUser user, Guid id, bool incluirSolicitante = false)
    {
        var solicitud = await context.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Include(s => s.Solicitante)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud is null || solicitud.TenantId != user.TenantId)
            throw AppException.RecursoNoEncontrado();

        if (!SolicitudPermissions.PuedeVerDetalle(user.Rol, solicitud.SolicitanteId, user.Id))
            throw AppException.RecursoNoEncontrado();

        return solicitud;
    }

    private async Task<Solicitud> CargarDetalleAsync(Guid id) =>
        await context.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Include(s => s.Solicitante).ThenInclude(u => u!.Tenant)
            .FirstAsync(s => s.Id == id);

    private static IQueryable<Solicitud> AplicarOrden(IQueryable<Solicitud> q, string sort)
    {
        return sort.ToLowerInvariant() switch
        {
            "fechacreacion" => q.OrderBy(s => s.FechaCreacion),
            "-fechacreacion" => q.OrderByDescending(s => s.FechaCreacion),
            "codigo" => q.OrderBy(s => s.Codigo),
            "prioridad" => q.OrderBy(s => s.Prioridad),
            "-prioridad" => q.OrderByDescending(s => s.Prioridad),
            _ => q.OrderByDescending(s => s.FechaCreacion),
        };
    }

    private static SolicitudListItemDto MapListItem(Solicitud s, DateTime now) =>
        new(
            s.Id,
            s.Codigo,
            s.Titulo,
            EnumMapper.ToApiString(s.Estado),
            EnumMapper.ToApiString(s.Prioridad),
            new CategoriaResumenDto(s.Categoria.Id, s.Categoria.Nombre),
            s.Agente is null ? null : new AgenteResumenDto(s.Agente.Id, s.Agente.Nombre),
            s.FechaCreacion,
            s.FechaLimiteSla,
            SlaCalculator.EstaVencida(s, now));

    private static SolicitudDetalleDto MapDetalle(Solicitud s, DateTime now) =>
        new(
            s.Id,
            s.Codigo,
            s.Titulo,
            s.Descripcion,
            EnumMapper.ToApiString(s.Estado),
            EnumMapper.ToApiString(s.Prioridad),
            new CategoriaResumenDto(s.Categoria.Id, s.Categoria.Nombre),
            s.Agente is null ? null : new AgenteResumenDto(s.Agente.Id, s.Agente.Nombre),
            new UsuarioDto(
                s.Solicitante.Id,
                s.Solicitante.Nombre,
                s.Solicitante.Email,
                EnumMapper.ToApiString(s.Solicitante.Rol),
                s.Solicitante.TenantId,
                s.Solicitante.Tenant?.Nombre ?? string.Empty),
            s.FechaCreacion,
            s.FechaLimiteSla,
            s.FechaResolucion,
            s.MotivoResolucion,
            s.MotivoCancelacion,
            SlaCalculator.EstaVencida(s, now));
}
