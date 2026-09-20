namespace TPN_5_MOVIL.Entities;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CUIT { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }

    public List<Producto> Productos { get; set; } = new();
    public List<Ingreso> Ingresos { get; set; } = new();
}