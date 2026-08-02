using MesaSitec.Domain.Entities;
using MesaSitec.Domain.Enums;
using MesaSitec.Domain.Services;

namespace MesaSitec.Tests;

public class SlaCalculatorTests
{
    [Fact]
    public void CalcularFechaLimite_Critica_AplicaFactor05()
    {
        var fecha = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
        var limite = SlaCalculator.CalcularFechaLimite(fecha, slaHoras: 8, Prioridad.Critica);
        Assert.Equal(fecha.AddHours(4), limite);
    }

    [Fact]
    public void CalcularFechaLimite_Baja_AplicaFactor2()
    {
        var fecha = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
        var limite = SlaCalculator.CalcularFechaLimite(fecha, slaHoras: 24, Prioridad.Baja);
        Assert.Equal(fecha.AddHours(48), limite);
    }

    [Fact]
    public void EstaVencida_RetornaTrue_SiPasóLimiteYEstaActiva()
    {
        var solicitud = new Solicitud
        {
            Estado = EstadoSolicitud.Nueva,
            FechaLimiteSla = DateTime.UtcNow.AddHours(-1),
        };

        Assert.True(SlaCalculator.EstaVencida(solicitud, DateTime.UtcNow));
    }

    [Fact]
    public void EstaVencida_RetornaFalse_SiEstaResuelta()
    {
        var solicitud = new Solicitud
        {
            Estado = EstadoSolicitud.Resuelta,
            FechaLimiteSla = DateTime.UtcNow.AddHours(-1),
        };

        Assert.False(SlaCalculator.EstaVencida(solicitud, DateTime.UtcNow));
    }
}

public class SolicitudStateMachineTests
{
    [Theory]
    [InlineData(EstadoSolicitud.Nueva, "asignar", true)]
    [InlineData(EstadoSolicitud.Nueva, "resolver", false)]
    [InlineData(EstadoSolicitud.Resuelta, "cerrar", true)]
    [InlineData(EstadoSolicitud.Cerrada, "cerrar", false)]
    public void EsTransicionValida_ValidaSegunRN02(EstadoSolicitud estado, string accion, bool esperado)
    {
        Assert.Equal(esperado, SolicitudStateMachine.EsTransicionValida(estado, accion));
    }

    [Fact]
    public void ObtenerNuevoEstado_NuevaAsignar_RetornaAsignada()
    {
        var nuevo = SolicitudStateMachine.ObtenerNuevoEstado(EstadoSolicitud.Nueva, "asignar");
        Assert.Equal(EstadoSolicitud.Asignada, nuevo);
    }
}

public class SolicitudPermissionsTests
{
    [Fact]
    public void PuedeListarTodas_Solicitante_RetornaFalse()
    {
        Assert.False(SolicitudPermissions.PuedeListarTodas(Rol.Solicitante));
    }

    [Fact]
    public void PuedeEditar_Solicitante_SoloPropiasEnNueva()
    {
        var id = Guid.NewGuid();
        Assert.True(SolicitudPermissions.PuedeEditar(Rol.Solicitante, id, id, EstadoSolicitud.Nueva));
        Assert.False(SolicitudPermissions.PuedeEditar(Rol.Solicitante, Guid.NewGuid(), id, EstadoSolicitud.Nueva));
        Assert.False(SolicitudPermissions.PuedeEditar(Rol.Solicitante, id, id, EstadoSolicitud.Asignada));
    }

    [Fact]
    public void PuedeEjecutarAccion_Cancelar_SoloAdmin()
    {
        Assert.True(SolicitudPermissions.PuedeEjecutarAccion(Rol.Admin, "cancelar"));
        Assert.False(SolicitudPermissions.PuedeEjecutarAccion(Rol.Agente, "cancelar"));
    }
}
