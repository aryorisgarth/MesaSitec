# MesaSitec

Mesa de servicio SaaS multi-tenant — prueba técnica Sitecpro.

## Requisitos previos

- .NET 8 SDK
- Node.js 20+
- npm 10+

## Arranque rápido

### 1. Variables de entorno (opcional)

Copia `.env.example` y ajusta si lo necesitas. Por defecto la API usa los valores de `appsettings.json`.

```bash
# Windows PowerShell
$env:JWT_SECRET="MesaSitecClaveSuperSecreta2026ParaJWT!"
$env:SEED_FECHA_BASE="2026-01-15T08:00:00Z"
```

### 2. Backend (puerto 5080)

```bash
cd MesaSitec.Api
dotnet run
```

- API: http://localhost:5080/api/v1
- Swagger: http://localhost:5080/swagger
- Health: http://localhost:5080/api/v1/health

La base de datos SQLite se migra y se siembra automáticamente al arrancar.

### 3. Frontend (puerto 5173)

En otra terminal:

```bash
cd frontend
npm install
npm run dev
```

- App: http://localhost:5173

## Credenciales de prueba

Contraseña de todos: `Sitec.2026`

| Email | Organización | Rol |
|-------|--------------|-----|
| admin@norte.test | Cooperativa Norte | Admin |
| agente1@norte.test | Cooperativa Norte | Agente |
| agente2@norte.test | Cooperativa Norte | Agente |
| user1@norte.test | Cooperativa Norte | Solicitante |
| user2@norte.test | Cooperativa Norte | Solicitante |
| admin@sur.test | Bufete Sur | Admin |
| user1@sur.test | Bufete Sur | Solicitante |

## Tests

Desde la raíz del proyecto:

```bash
dotnet test
cd frontend && npm run typecheck
```

## Implementado

- Backend: 9 endpoints, JWT, EF Core + SQLite, semilla automática
- Reglas de negocio RN-01 a RN-07
- 12 tests unitarios (estados, SLA, permisos)
- Frontend Vue 3: login, listado, detalle, crear/editar
- `data-testid` según enunciado
- `DECISIONES.md` con decisiones técnicas y uso de IA

## Documentación adicional

Ver `DECISIONES.md` para decisiones técnicas, uso de IA y puntos de mejora.
