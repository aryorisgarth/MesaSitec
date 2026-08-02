using MesaSitec.Domain.Entities;
using MesaSitec.Domain.Enums;

namespace MesaSitec.Domain.Services;

public static class SlaCalculator
{
    private static readonly Dictionary<Prioridad, double> Factores = new()
    {
        { Prioridad.Critica, 0.5 },
        { Prioridad.Alta, 0.75 },
        { Prioridad.Media, 1.0 },
        { Prioridad.Baja, 2.0 }
    };

    public static DateTime CalcularFechaLimite(DateTime fechaCreacion, int slaHoras, Prioridad prioridad)
    {
        var horas = slaHoras * Factores[prioridad];
        return fechaCreacion.AddHours(horas);
    }

    public static bool EstaVencida(Solicitud solicitud, DateTime utcNow)
    {
        if (solicitud.Estado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada or EstadoSolicitud.Cancelada)
            return false;

        return solicitud.FechaLimiteSla < utcNow;
    }
}
