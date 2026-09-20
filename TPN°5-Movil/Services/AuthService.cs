using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TPN_5_MOVIL.Data;
using TPN_5_MOVIL.DTOs;
using TPN_5_MOVIL.Entities;
using TPN_5_MOVIL.Exceptions;

namespace TPN_5_MOVIL.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<Usuario> _passwordHasher;

    public AuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<Usuario>();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario is null || !usuario.Activo)
            throw new ApiException("Credenciales inválidas o usuario inactivo.", StatusCodes.Status401Unauthorized);

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);
        if (resultado == PasswordVerificationResult.Failed)
            throw new ApiException("Credenciales inválidas o usuario inactivo.", StatusCodes.Status401Unauthorized);

        var token = GenerarToken(usuario);
        return new LoginResponse(
            token,
            DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "120")),
            new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Email, usuario.Rol, usuario.Activo));
    }

    public async Task<UsuarioDto> RegisterAsync(RegisterRequest request)
    {
        var rol = request.Rol == Usuario.RolAdmin ? Usuario.RolAdmin : Usuario.RolUsuario;

        if (await _db.Usuarios.AnyAsync(u => u.Email == request.Email))
            throw new ApiException("Ya existe un usuario con ese email.", StatusCodes.Status409Conflict);

        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto,
            Email = request.Email,
            Rol = rol,
            Activo = true
        };
        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Password);

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Email, usuario.Rol, usuario.Activo);
    }

    private string GenerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "120")),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}