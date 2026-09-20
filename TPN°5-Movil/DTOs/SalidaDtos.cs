namespace TPN_5_MOVIL.DTOs;

public record SalidaResponse(
    int Id,
    int ProductoId,
    string ProductoNombre,
    int ClienteId,
    string ClienteNombre,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Total,
    DateTime Fecha,
    string? Observaciones);

public record SalidaRequest(
    int ProductoId,
    int ClienteId,
    int Cantidad,
    string? Observaciones);