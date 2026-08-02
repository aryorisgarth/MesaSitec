using MesaSitec.Domain.Entities;
using MesaSitec.Domain.Enums;
using MesaSitec.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infrastructure.Persistence;

public static class DbSeeder
{
    private const string SeedPassword = "Sitec.2026";

    public static async Task SeedAsync(AppDbContext context, string? seedFechaBase = null)
    {
        if (await context.Tenants.AnyAsync())
            return;

        var baseDate = DateTime.Parse(
            seedFechaBase ?? "2026-01-15T08:00:00Z",
            null,
            System.Globalization.DateTimeStyles.RoundtripKind);

        var tenantNorte = new Tenant { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Nombre = "Cooperativa Norte", Activo = true };
        var tenantSur = new Tenant { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Nombre = "Bufete Sur", Activo = true };

        context.Tenants.AddRange(tenantNorte, tenantSur);

        var usuarios = CrearUsuarios(tenantNorte.Id, tenantSur.Id);
        context.Usuarios.AddRange(usuarios);

        var categoriasNorte = CrearCategorias(tenantNorte.Id);
        var categoriasSur = CrearCategorias(tenantSur.Id);
        context.Categorias.AddRange(categoriasNorte);
        context.Categorias.AddRange(categoriasSur);

        await context.SaveChangesAsync();

        var solicitudesNorte = CrearSolicitudesNorte(tenantNorte.Id, usuarios, categoriasNorte, baseDate);
        var solicitudesSur = CrearSolicitudesSur(tenantSur.Id, usuarios, categoriasSur, baseDate);
        context.Solicitudes.AddRange(solicitudesNorte);
        context.Solicitudes.AddRange(solicitudesSur);

        await context.SaveChangesAsync();
    }

    private static List<Usuario> CrearUsuarios(Guid tenantNorte, Guid tenantSur)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(SeedPassword);

        return
        [
            new Usuario { Id = Guid.Parse("a1000001-0000-0000-0000-000000000001"), TenantId = tenantNorte, Email = "admin@norte.test", PasswordHash = hash, Nombre = "Admin Norte", Rol = Rol.Admin, Activo = true },
            new Usuario { Id = Guid.Parse("a1000002-0000-0000-0000-000000000001"), TenantId = tenantNorte, Email = "agente1@norte.test", PasswordHash = hash, Nombre = "Agente Uno Norte", Rol = Rol.Agente, Activo = true },
            new Usuario { Id = Guid.Parse("a1000003-0000-0000-0000-000000000001"), TenantId = tenantNorte, Email = "agente2@norte.test", PasswordHash = hash, Nombre = "Agente Dos Norte", Rol = Rol.Agente, Activo = true },
            new Usuario { Id = Guid.Parse("a1000004-0000-0000-0000-000000000001"), TenantId = tenantNorte, Email = "user1@norte.test", PasswordHash = hash, Nombre = "Usuario Uno Norte", Rol = Rol.Solicitante, Activo = true },
            new Usuario { Id = Guid.Parse("a1000005-0000-0000-0000-000000000001"), TenantId = tenantNorte, Email = "user2@norte.test", PasswordHash = hash, Nombre = "Usuario Dos Norte", Rol = Rol.Solicitante, Activo = true },
            new Usuario { Id = Guid.Parse("a2000001-0000-0000-0000-000000000001"), TenantId = tenantSur, Email = "admin@sur.test", PasswordHash = hash, Nombre = "Admin Sur", Rol = Rol.Admin, Activo = true },
            new Usuario { Id = Guid.Parse("a2000002-0000-0000-0000-000000000001"), TenantId = tenantSur, Email = "user1@sur.test", PasswordHash = hash, Nombre = "Usuario Uno Sur", Rol = Rol.Solicitante, Activo = true },
        ];
    }

    private static List<Categoria> CrearCategorias(Guid tenantId)
    {
        if (tenantId == Guid.Parse("11111111-1111-1111-1111-111111111111"))
        {
            return
            [
                new Categoria { Id = Guid.Parse("c1111111-0001-0000-0000-000000000001"), TenantId = tenantId, Nombre = "Incidente", SlaHoras = 8, Activo = true },
                new Categoria { Id = Guid.Parse("c1111111-0002-0000-0000-000000000001"), TenantId = tenantId, Nombre = "Requerimiento", SlaHoras = 40, Activo = true },
                new Categoria { Id = Guid.Parse("c1111111-0003-0000-0000-000000000001"), TenantId = tenantId, Nombre = "Consulta", SlaHoras = 24, Activo = true },
                new Categoria { Id = Guid.Parse("c1111111-0004-0000-0000-000000000001"), TenantId = tenantId, Nombre = "Falla crítica", SlaHoras = 4, Activo = true },
            ];
        }

        return
        [
            new Categoria { Id = Guid.Parse("c2222222-0001-0000-0000-000000000001"), TenantId = tenantId, Nombre = "Incidente", SlaHoras = 8, Activo = true },
            new Categoria { Id = Guid.Parse("c2222222-0002-0000-0000-000000000001"), TenantId = tenantId, Nombre = "Requerimiento", SlaHoras = 40, Activo = true },
            new Categoria { Id = Guid.Parse("c2222222-0003-0000-0000-000000000001"), TenantId = tenantId, Nombre = "Consulta", SlaHoras = 24, Activo = true },
            new Categoria { Id = Guid.Parse("c2222222-0004-0000-0000-000000000001"), TenantId = tenantId, Nombre = "Falla crítica", SlaHoras = 4, Activo = true },
        ];
    }

    private static List<Solicitud> CrearSolicitudesNorte(Guid tenantId, List<Usuario> usuarios, List<Categoria> categorias, DateTime baseDate)
    {
        var norteUsers = usuarios.Where(u => u.TenantId == tenantId).ToList();
        var solicitante1 = norteUsers.First(u => u.Rol == Rol.Solicitante);
        var solicitante2 = norteUsers.Last(u => u.Rol == Rol.Solicitante);
        var agente1 = norteUsers.First(u => u.Email == "agente1@norte.test");
        var agente2 = norteUsers.First(u => u.Email == "agente2@norte.test");

        var configs = new (EstadoSolicitud estado, Prioridad prioridad, int catIdx, int dayOffset, Guid? agente, string titulo)[]
        {
            (EstadoSolicitud.Nueva, Prioridad.Alta, 0, 0, null, "No puedo acceder al portal"),
            (EstadoSolicitud.Nueva, Prioridad.Media, 2, -1, null, "Consulta sobre facturación"),
            (EstadoSolicitud.Asignada, Prioridad.Critica, 3, -2, agente1.Id, "Caída total del sistema"),
            (EstadoSolicitud.Asignada, Prioridad.Alta, 0, -3, agente2.Id, "Error al exportar reportes"),
            (EstadoSolicitud.EnProceso, Prioridad.Media, 1, -4, agente1.Id, "Solicitud de nuevo módulo"),
            (EstadoSolicitud.EnProceso, Prioridad.Baja, 2, -5, agente2.Id, "Duda sobre permisos"),
            (EstadoSolicitud.Resuelta, Prioridad.Alta, 0, -10, agente1.Id, "Restablecimiento de contraseña"),
            (EstadoSolicitud.Resuelta, Prioridad.Media, 1, -12, agente2.Id, "Instalación de software"),
            (EstadoSolicitud.Resuelta, Prioridad.Critica, 3, -14, agente1.Id, "Recuperación de servicio"),
            (EstadoSolicitud.Cerrada, Prioridad.Baja, 2, -20, agente2.Id, "Consulta de horarios"),
            (EstadoSolicitud.Cerrada, Prioridad.Media, 0, -22, agente1.Id, "Problema de impresión"),
            (EstadoSolicitud.Cancelada, Prioridad.Alta, 0, -6, null, "Solicitud duplicada"),
            (EstadoSolicitud.Nueva, Prioridad.Critica, 3, -30, null, "Servidor no responde"),
            (EstadoSolicitud.Nueva, Prioridad.Baja, 2, -35, null, "Pregunta sobre manual"),
            (EstadoSolicitud.Asignada, Prioridad.Media, 1, -8, agente1.Id, "Cambio de datos personales"),
            (EstadoSolicitud.EnProceso, Prioridad.Alta, 0, -7, agente2.Id, "VPN no conecta"),
            (EstadoSolicitud.Nueva, Prioridad.Media, 0, -40, null, "Acceso denegado en módulo"),
            (EstadoSolicitud.Nueva, Prioridad.Alta, 0, -45, null, "Correo no sincroniza"),
            (EstadoSolicitud.Nueva, Prioridad.Critica, 3, -50, null, "Base de datos lenta"),
            (EstadoSolicitud.Asignada, Prioridad.Baja, 2, -15, agente1.Id, "Actualización de datos"),
            (EstadoSolicitud.EnProceso, Prioridad.Critica, 3, -9, agente2.Id, "Intermitencia en red"),
            (EstadoSolicitud.Resuelta, Prioridad.Baja, 2, -18, agente1.Id, "Configuración de email"),
            (EstadoSolicitud.Cerrada, Prioridad.Alta, 0, -25, agente2.Id, "Error de login resuelto"),
            (EstadoSolicitud.Cancelada, Prioridad.Media, 1, -11, null, "Cancelada por usuario"),
            (EstadoSolicitud.Nueva, Prioridad.Alta, 0, -55, null, "Pantalla en blanco al ingresar"),
        };

        return CrearSolicitudes(tenantId, configs, categorias, solicitante1, solicitante2, baseDate, 1);
    }

    private static List<Solicitud> CrearSolicitudesSur(Guid tenantId, List<Usuario> usuarios, List<Categoria> categorias, DateTime baseDate)
    {
        var solicitante = usuarios.First(u => u.TenantId == tenantId);

        var configs = new (EstadoSolicitud estado, Prioridad prioridad, int catIdx, int dayOffset, Guid? agente, string titulo)[]
        {
            (EstadoSolicitud.Nueva, Prioridad.Alta, 0, 0, null, "Problema con expediente"),
            (EstadoSolicitud.Nueva, Prioridad.Media, 2, -2, null, "Consulta legal urgente"),
            (EstadoSolicitud.Asignada, Prioridad.Critica, 3, -3, solicitante.Id, "Sistema caído en oficina"),
            (EstadoSolicitud.EnProceso, Prioridad.Alta, 0, -5, solicitante.Id, "Error en firma digital"),
            (EstadoSolicitud.Resuelta, Prioridad.Media, 1, -10, solicitante.Id, "Restablecer acceso"),
            (EstadoSolicitud.Cerrada, Prioridad.Baja, 2, -15, solicitante.Id, "Consulta sobre plazos"),
            (EstadoSolicitud.Cancelada, Prioridad.Alta, 0, -7, null, "Duplicada de otra solicitud"),
            (EstadoSolicitud.Nueva, Prioridad.Baja, 2, -20, null, "Manual de usuario"),
        };

        return CrearSolicitudes(tenantId, configs, categorias, solicitante, solicitante, baseDate, 1);
    }

    private static List<Solicitud> CrearSolicitudes(
        Guid tenantId,
        (EstadoSolicitud estado, Prioridad prioridad, int catIdx, int dayOffset, Guid? agente, string titulo)[] configs,
        List<Categoria> categorias,
        Usuario solicitante1,
        Usuario solicitante2,
        DateTime baseDate,
        int correlativoInicio)
    {
        var solicitudes = new List<Solicitud>();
        var correlativo = correlativoInicio;

        foreach (var (estado, prioridad, catIdx, dayOffset, agente, titulo) in configs)
        {
            var categoria = categorias[catIdx];
            var fechaCreacion = baseDate.AddDays(dayOffset);
            var solicitud = new Solicitud
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = CodigoGenerator.Generar(correlativo, fechaCreacion),
                Titulo = titulo,
                Descripcion = $"Descripción detallada de la solicitud: {titulo}. Requiere atención del equipo de soporte.",
                CategoriaId = categoria.Id,
                Prioridad = prioridad,
                Estado = estado,
                SolicitanteId = correlativo % 2 == 0 ? solicitante1.Id : solicitante2.Id,
                AgenteId = agente,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = SlaCalculator.CalcularFechaLimite(fechaCreacion, categoria.SlaHoras, prioridad),
            };

            if (estado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada)
            {
                solicitud.FechaResolucion = fechaCreacion.AddHours(2);
                solicitud.MotivoResolucion = "Se aplicó la solución correspondiente y se validó con el usuario.";
            }

            if (estado == EstadoSolicitud.Cancelada)
                solicitud.MotivoCancelacion = "Solicitud cancelada por duplicidad o error del usuario.";

            solicitudes.Add(solicitud);
            correlativo++;
        }

        return solicitudes;
    }
}
