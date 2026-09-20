namespace TPN_5_MOVIL.DTOs;

public record IngresoResponse(
    int Id,
    int ProductoId,
    string ProductoNombre,
    int ProveedorId,
    string ProveedorNombre,
    int Cantidad,
    DateTime Fecha,
    string? Observaciones);

public record IngresoRequest(
    int ProductoId,
    int ProveedorId,
    int Cantidad,
    string? Observaciones);