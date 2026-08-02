using MesaSitec.Domain.Enums;
namespace MesaSitec.Domain.Entities; 

public class Usuario
{
    public Guid Id {get; set;}

    public Guid TenantId {get; set;}

    public string Email {get; set;} = string.Empty;

    public string PasswordHash {get; set;} = string.Empty;

    public String Nombre {get; set;} = string.Empty;

    public Rol Rol {get; set;}

    public bool Activo {get; set;}

    public Tenant Tenant {get; set;} = null!;
}