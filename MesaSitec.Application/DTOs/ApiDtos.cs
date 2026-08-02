using System.Text.Json.Serialization;
using MesaSitec.Domain.Enums;

namespace MesaSitec.Application.DTOs;

public record LoginRequestDto(string Email, string Password);

public record LoginResponseDto(string AccessToken, int ExpiraEn, UsuarioDto Usuario);

public record UsuarioDto(
    Guid Id,
    string Nombre,
    string Email,
    string Rol,
    Guid TenantId,
    string TenantNombre);

public record CategoriaDto(Guid Id, string Nombre, int SlaHoras);

public record AgenteResumenDto(Guid Id, string Nombre);

public record CategoriaResumenDto(Guid Id, string Nombre);

public record SolicitudListItemDto(
    Guid Id,
    string Codigo,
    string Titulo,
    string Estado,
    string Prioridad,
    CategoriaResumenDto Categoria,
    AgenteResumenDto? Agente,
    DateTime FechaCreacion,
    DateTime FechaLimiteSla,
    bool Vencida);

public record SolicitudDetalleDto(
    Guid Id,
    string Codigo,
    string Titulo,
    string Descripcion,
    string Estado,
    string Prioridad,
    CategoriaResumenDto Categoria,
    AgenteResumenDto? Agente,
    UsuarioDto Solicitante,
    DateTime FechaCreacion,
    DateTime FechaLimiteSla,
    DateTime? FechaResolucion,
    string? MotivoResolucion,
    string? MotivoCancelacion,
    bool Vencida);

public record PaginatedResponseDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total,
    int TotalPaginas);

public record CrearSolicitudDto(string Titulo, string Descripcion, Guid CategoriaId, string Prioridad);

public record EditarSolicitudDto(string Titulo, string Descripcion, Guid CategoriaId, string Prioridad);

public record TransicionDto(string Accion, Guid? AgenteId, string? Motivo);

public record HealthResponseDto(string Estado);

public static class EnumMapper
{
    public static string ToApiString(Rol rol) => rol switch
    {
        Rol.Admin => "Admin",
        Rol.Agente => "Agente",
        Rol.Solicitante => "Solicitante",
        _ => rol.ToString()
    };

    public static string ToApiString(EstadoSolicitud estado) => estado switch
    {
        EstadoSolicitud.EnProceso => "EnProceso",
        _ => estado.ToString()
    };

    public static string ToApiString(Prioridad prioridad) => prioridad.ToString();

    public static Prioridad ParsePrioridad(string value) =>
        Enum.Parse<Prioridad>(value, ignoreCase: true);

    public static EstadoSolicitud? ParseEstado(string? value) =>
        value is null ? null : Enum.Parse<EstadoSolicitud>(value, ignoreCase: true);
}
