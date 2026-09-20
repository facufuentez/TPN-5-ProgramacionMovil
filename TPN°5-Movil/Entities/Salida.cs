namespace TPN_5_MOVIL.Entities;

public class Salida
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }
}