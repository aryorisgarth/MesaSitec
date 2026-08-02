using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MesaSitec.Application.DTOs;
using MesaSitec.Application.Exceptions;
using MesaSitec.Application.Services;
using MesaSitec.Domain.Enums;
using MesaSitec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MesaSitec.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UsuarioDto> GetMeAsync(CurrentUser user);
}

public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
{
    private const int ExpiraEnSegundos = 28800;

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var usuario = await context.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw AppException.NoAutenticado("Credenciales incorrectas.");

        if (!usuario.Activo)
            throw AppException.NoAutenticado("Credenciales incorrectas.");

        var token = GenerarToken(usuario);
        return new LoginResponseDto(token, ExpiraEnSegundos, MapUsuario(usuario));
    }

    public async Task<UsuarioDto> GetMeAsync(CurrentUser user)
    {
        var usuario = await context.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == user.Id && u.TenantId == user.TenantId)
            ?? throw AppException.RecursoNoEncontrado();

        return MapUsuario(usuario);
    }

    private string GenerarToken(Domain.Entities.Usuario usuario)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT_SECRET no configurado");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim("tenantId", usuario.TenantId.ToString()),
            new Claim("rol", EnumMapper.ToApiString(usuario.Rol)),
            new Claim("email", usuario.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(ExpiraEnSegundos),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    internal static UsuarioDto MapUsuario(Domain.Entities.Usuario usuario) =>
        new(
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            EnumMapper.ToApiString(usuario.Rol),
            usuario.TenantId,
            usuario.Tenant.Nombre);
}
