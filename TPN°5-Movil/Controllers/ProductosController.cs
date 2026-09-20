using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPN_5_MOVIL.Data;
using TPN_5_MOVIL.DTOs;
using TPN_5_MOVIL.Entities;
using TPN_5_MOVIL.Exceptions;

namespace TPN_5_MOVIL.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize]
public class ProductosController : ControllerBase
{
    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
    private const int MaxTamanoImagen = 5 * 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductosController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ProductoResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? busqueda = null,
        [FromQuery] int? categoriaId = null,
        [FromQuery] int? proveedorId = null)
    {
        var query = _db.Productos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p => p.Nombre.Contains(busqueda) || p.Descripcion!.Contains(busqueda));

        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaId == categoriaId);

        if (proveedorId.HasValue)
            query = query.Where(p => p.ProveedorId == proveedorId);

        query = query.OrderBy(p => p.Nombre);

        var result = await PaginatedResult<ProductoResponse>.CreateAsync(
            query.Select(p => new ProductoResponse(
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.ImagenUrl,
                p.CategoriaId,
                p.Categoria.Nombre,
                p.ProveedorId,
                p.Proveedor.Nombre)),
            page,
            pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoResponse>> GetById(int id)
    {
        var producto = await _db.Productos
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductoResponse(
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.ImagenUrl,
                p.CategoriaId,
                p.Categoria.Nombre,
                p.ProveedorId,
                p.Proveedor.Nombre))
            .FirstOrDefaultAsync();

        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpGet("{id:int}/movimientos")]
    public async Task<ActionResult<PaginatedResult<MovimientoItem>>> GetMovimientos(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!await _db.Productos.AnyAsync(p => p.Id == id))
            return NotFound();

        var ingresos = _db.Ingresos
            .Where(i => i.ProductoId == id)
            .Select(i => new
            {
                i.Id,
                Tipo = "Ingreso",
                Fecha = i.Fecha,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.Producto.Precio,
                Contraparte = i.Proveedor.Nombre,
                Observaciones = i.Observaciones
            });

        var salidas = _db.Salidas
            .Where(s => s.ProductoId == id)
            .Select(s => new
            {
                s.Id,
                Tipo = "Salida",
                Fecha = s.Fecha,
                Cantidad = -s.Cantidad,
                PrecioUnitario = s.PrecioUnitario,
                Contraparte = s.Cliente.Nombre,
                Observaciones = s.Observaciones
            });

        var combinados = ingresos
            .Concat(salidas)
            .OrderByDescending(m => m.Fecha)
            .Select(m => new MovimientoItem(
                m.Id,
                m.Tipo,
                m.Fecha,
                m.Cantidad,
                m.PrecioUnitario,
                m.Contraparte,
                m.Observaciones));

        var result = await PaginatedResult<MovimientoItem>.CreateAsync(combinados, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<ActionResult<ProductoResponse>> Create([FromBody] ProductoCreateRequest request)
    {
        if (request.Precio < 0)
            throw new ApiException("El precio no puede ser negativo.");

        if (!await _db.Categorias.AnyAsync(c => c.Id == request.CategoriaId))
            throw new ApiException("La categoría indicada no existe.", StatusCodes.Status404NotFound);

        if (!await _db.Proveedores.AnyAsync(p => p.Id == request.ProveedorId))
            throw new ApiException("El proveedor indicado no existe.", StatusCodes.Status404NotFound);

        var producto = new Producto
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Stock = 0,
            CategoriaId = request.CategoriaId,
            ProveedorId = request.ProveedorId,
            FechaCreacion = DateTime.UtcNow
        };

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = producto.Id }, await ConstruirResponseAsync(producto.Id));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<IActionResult> Update(int id, [FromBody] ProductoUpdateRequest request)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto is null)
            return NotFound();

        if (request.Precio < 0)
            throw new ApiException("El precio no puede ser negativo.");

        if (!await _db.Categorias.AnyAsync(c => c.Id == request.CategoriaId))
            throw new ApiException("La categoría indicada no existe.", StatusCodes.Status404NotFound);

        if (!await _db.Proveedores.AnyAsync(p => p.Id == request.ProveedorId))
            throw new ApiException("El proveedor indicado no existe.", StatusCodes.Status404NotFound);

        producto.Nombre = request.Nombre;
        producto.Descripcion = request.Descripcion;
        producto.Precio = request.Precio;
        producto.CategoriaId = request.CategoriaId;
        producto.ProveedorId = request.ProveedorId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto is null)
            return NotFound();

        if (await _db.Ingresos.AnyAsync(i => i.ProductoId == id) || await _db.Salidas.AnyAsync(s => s.ProductoId == id))
            throw new ApiException("No se puede eliminar: el producto tiene movimientos asociados.", StatusCodes.Status409Conflict);

        if (!string.IsNullOrWhiteSpace(producto.ImagenUrl))
            EliminarArchivoImagen(producto.ImagenUrl);

        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/imagen")]
    [Authorize(Roles = Usuario.RolAdmin)]
    [RequestSizeLimit(MaxTamanoImagen + 1024)]
    public async Task<IActionResult> UploadImagen(int id, IFormFile file)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto is null)
            return NotFound();

        if (file is null || file.Length == 0)
            throw new ApiException("Debe enviar un archivo de imagen.");

        if (file.Length > MaxTamanoImagen)
            throw new ApiException("La imagen no puede superar los 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
            throw new ApiException("Formato no permitido. Use .jpg, .jpeg, .png o .webp.");

        var rutaWebRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var carpetaUploads = Path.Combine(rutaWebRoot, "uploads");
        Directory.CreateDirectory(carpetaUploads);

        var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
        var rutaCompleta = Path.Combine(carpetaUploads, nombreArchivo);

        await using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (!string.IsNullOrWhiteSpace(producto.ImagenUrl) && producto.ImagenUrl.StartsWith("/uploads/"))
            EliminarArchivoImagen(producto.ImagenUrl);

        producto.ImagenUrl = $"/uploads/{nombreArchivo}";
        await _db.SaveChangesAsync();

        return Ok(new { producto.ImagenUrl });
    }

    private async Task<ProductoResponse?> ConstruirResponseAsync(int id)
    {
        return await _db.Productos
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductoResponse(
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.ImagenUrl,
                p.CategoriaId,
                p.Categoria.Nombre,
                p.ProveedorId,
                p.Proveedor.Nombre))
            .FirstOrDefaultAsync();
    }

    private static void EliminarArchivoImagen(string url)
    {
        var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(ruta))
            System.IO.File.Delete(ruta);
    }
}