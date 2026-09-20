namespace TPN_5_MOVIL.DTOs;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string NombreCompleto, string Email, string Password, string Rol);

public record LoginResponse(string Token, DateTime ExpiresAt, UsuarioDto Usuario);