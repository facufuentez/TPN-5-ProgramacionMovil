namespace TPN_5_MOVIL.Entities;

public class Ingreso
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }
}