namespace TPN_5_MOVIL.DTOs;

public record CategoriaResponse(int Id, string Nombre, string? Descripcion, DateTime FechaCreacion);

public record CategoriaCreateRequest(string Nombre, string? Descripcion);

public record CategoriaUpdateRequest(string Nombre, string? Descripcion);