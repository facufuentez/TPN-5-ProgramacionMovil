namespace TPN_5_MOVIL.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public List<Ingreso> Ingresos { get; set; } = new();
    public List<Salida> Salidas { get; set; } = new();
}