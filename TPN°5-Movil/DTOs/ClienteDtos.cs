namespace TPN_5_MOVIL.DTOs;

public record ClienteResponse(
    int Id,
    string Nombre,
    string CUIT,
    string? Email,
    string? Telefono,
    string? Direccion);

public record ClienteRequest(
    string Nombre,
    string CUIT,
    string? Email,
    string? Telefono,
    string? Direccion);