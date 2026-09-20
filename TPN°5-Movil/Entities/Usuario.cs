namespace TPN_5_MOVIL.Entities;

public class Usuario
{
    public const string RolAdmin = "Admin";
    public const string RolUsuario = "Usuario";

    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = RolUsuario;
    public bool Activo { get; set; } = true;
}