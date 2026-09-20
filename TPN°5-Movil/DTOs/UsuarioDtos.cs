namespace TPN_5_MOVIL.DTOs;

public record UsuarioDto(int Id, string NombreCompleto, string Email, string Rol, bool Activo);

public record UsuarioCreateRequest(string NombreCompleto, string Email, string Password, string Rol);

public record UsuarioUpdateRequest(string NombreCompleto, string Email, string Rol, bool Activo, string? Password);