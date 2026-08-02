using MesaSitec.Domain.Enums;

namespace MesaSitec.Domain.Services;

public static class SolicitudStateMachine
{
    private static readonly Dictionary<(EstadoSolicitud, string), EstadoSolicitud> Transiciones = new()
    {
        { (EstadoSolicitud.Nueva, "asignar"), EstadoSolicitud.Asignada },
        { (EstadoSolicitud.Nueva, "cancelar"), EstadoSolicitud.Cancelada },
        { (EstadoSolicitud.Asignada, "iniciar"), EstadoSolicitud.EnProceso },
        { (EstadoSolicitud.Asignada, "asignar"), EstadoSolicitud.Asignada },
        { (EstadoSolicitud.Asignada, "cancelar"), EstadoSolicitud.Cancelada },
        { (EstadoSolicitud.EnProceso, "resolver"), EstadoSolicitud.Resuelta },
        { (EstadoSolicitud.EnProceso, "asignar"), EstadoSolicitud.Asignada },
        { (EstadoSolicitud.EnProceso, "cancelar"), EstadoSolicitud.Cancelada },
        { (EstadoSolicitud.Resuelta, "cerrar"), EstadoSolicitud.Cerrada },
        { (EstadoSolicitud.Resuelta, "reabrir"), EstadoSolicitud.EnProceso },
    };

    public static bool EsTransicionValida(EstadoSolicitud estadoActual, string accion)
    {
        return Transiciones.ContainsKey((estadoActual, accion.ToLowerInvariant()));
    }

    public static EstadoSolicitud ObtenerNuevoEstado(EstadoSolicitud estadoActual, string accion)
    {
        var clave = (estadoActual, accion.ToLowerInvariant());
        if (!Transiciones.TryGetValue(clave, out var nuevoEstado))
            throw new InvalidOperationException($"Transición inválida: {accion} desde {estadoActual}");

        return nuevoEstado;
    }
}
