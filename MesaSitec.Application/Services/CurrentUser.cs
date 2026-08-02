using MesaSitec.Domain.Enums;

namespace MesaSitec.Application.Services;

public record CurrentUser(Guid Id, Guid TenantId, Rol Rol, string Email);
