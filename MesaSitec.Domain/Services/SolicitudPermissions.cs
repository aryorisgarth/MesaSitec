using MesaSitec.Domain.Enums;

namespace MesaSitec.Domain.Services;

public static class SolicitudPermissions
{
    public static bool PuedeListarTodas(Rol rol) =>
        rol is Rol.Admin or Rol.Agente;

    public static bool PuedeVerDetalle(Rol rol, Guid solicitanteId, Guid usuarioId) =>
        rol is Rol.Admin or Rol.Agente || solicitanteId == usuarioId;

    public static bool PuedeCrear(Rol rol) => true;

    public static bool PuedeEditar(Rol rol, Guid solicitanteId, Guid usuarioId, EstadoSolicitud estado)
    {
        if (rol is Rol.Admin or Rol.Agente)
            return true;

        return rol == Rol.Solicitante && solicitanteId == usuarioId && estado == EstadoSolicitud.Nueva;
    }

    public static bool PuedeEjecutarAccion(Rol rol, string accion)
    {
        return accion.ToLowerInvariant() switch
        {
            "asignar" or "iniciar" or "resolver" or "reabrir" => rol is Rol.Admin or Rol.Agente,
            "cerrar" => true,
            "cancelar" => rol == Rol.Admin,
            _ => false
        };
    }
}
