# DECISIONES.md — MesaSitec

## 1. Decisiones técnicas

### a) Arquitectura en capas (Api / Application / Domain / Infrastructure)

**Elegí:** separar dominio, aplicación, infraestructura y API en proyectos distintos.

**Descarté:** poner toda la lógica dentro de los controllers en un solo proyecto.

**Por qué:** el enunciado pide que la máquina de estados, el SLA y los permisos se puedan probar sin levantar la app completa. Con capas, las entidades y reglas viven en `Domain`, EF Core en `Infrastructure`, y los controllers solo delegan.

### b) SQLite con migraciones y semilla al arrancar

**Elegí:** EF Core + SQLite, `Database.Migrate()` y `DbSeeder` en `Program.cs` al iniciar la API.

**Descarté:** scripts SQL manuales o crear la BD a mano antes de correr el proyecto.

**Por qué:** el PDF exige SQLite sin instalar nada extra y que el evaluador levante todo en menos de 5 minutos. Automatizar migración y seed evita pasos manuales y garantiza datos idénticos con `SEED_FECHA_BASE`.

### c) Aislamiento multi-tenant con filtro por `tenantId` y respuesta 404

**Elegí:** filtrar siempre por `tenantId` del token JWT y devolver `404 RECURSO_NO_ENCONTRADO` cuando el recurso no existe o pertenece a otra organización.

**Descarté:** devolver `403 Forbidden` cuando el ID existe pero es de otro tenant.

**Por qué:** RN-01 lo exige explícitamente — un 403 confirmaría que el recurso existe. Lo apliqué en consultas EF (`Where(s => s.TenantId == user.TenantId)`) para que no dependa de un filtro olvidado en un solo endpoint.

---

## 2. Uso de IA vs código escrito a mano

Usé **Cursor (IA)** por el plazo de una semana, principalmente para acelerar lo repetitivo del contrato de la API.

**Escribí yo:**

- Entidades del dominio (`Tenant`, `Usuario`, `Categoria`, `Solicitud`) y enums
- `AppDbContext` con índices (email único, tenant+código único)
- Instalación de dependencias, migraciones EF Core
- `Program.cs` completo: DI, JWT, CORS, Swagger, migrate + seed al arrancar
- `AuthController` (login)

**Con ayuda de IA (revisé e integré en `Program.cs`):**

- Servicios de dominio (`SolicitudStateMachine`, `SlaCalculator`, `SolicitudPermissions`, `CodigoGenerator`)
- Capa Application (`AuthService`, `SolicitudService`, DTOs, excepciones)
- Controllers restantes, middleware de errores, `DbSeeder`, tests xUnit
- Frontend completo (Vue 3, Pinia, vistas, `data-testid`, cliente HTTP)
- `README.md` inicial

La IA me ahorró tiempo en boilerplate; yo me enfoqué en el modelo de datos, la persistencia y el arranque de la aplicación porque es lo que todo lo demás necesita.

---

## 3. Qué haría distinto con una semana más

1. **Revisar línea por línea** el código generado por IA, especialmente `SolicitudService` y el frontend, para entenderlo tan bien como lo que escribí yo.
2. **Escribir más tests a mano** — hoy cubren RN-02, RN-03 y RN-04, pero quiero casos de integración para RN-01 (404 cross-tenant).
3. **Historial de git** con commits incrementales en lugar de entregar bloques grandes.
4. **Generar tipos TypeScript desde OpenAPI** en lugar de mantener `types/api.ts` a mano.
5. **Probar manualmente** todos los `data-testid` y la visibilidad de botones según rol/estado antes de entregar.

---

## 4. Dónde me atasqué y cómo lo resolví

**JWT y claims personalizados.** Al principio el token se generaba pero `[Authorize]` no resolvía bien `sub`, `tenantId` y `rol`. Tuve que configurar `MapInboundClaims = false` y mapear `NameClaimType = "sub"` y `RoleClaimType = "rol"` en `Program.cs` para que coincidieran con los claims que emite el login.

**Integrar código de IA con mi `Program.cs`.** Generé servicios y controllers con IA, pero fallaban al arrancar por dependencias no registradas en DI. Lo resolví registrando manualmente `IAuthService`, `ICategoriaService` e `ISolicitudService` y verificando que cada controller usara interfaces ya registradas.

**Índice único tenant + código (RN-07).** Al sembrar solicitudes, tuve que definir en `AppDbContext` el índice compuesto `{ TenantId, Codigo }` para reflejar que el correlativo es independiente por organización y evitar duplicados en la misma org.
