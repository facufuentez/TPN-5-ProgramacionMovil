using Microsoft.EntityFrameworkCore;
using TPN_5_MOVIL.Data;
using TPN_5_MOVIL.DTOs;
using TPN_5_MOVIL.Entities;
using TPN_5_MOVIL.Exceptions;

namespace TPN_5_MOVIL.Services;

public class MovimientoService
{
    private readonly AppDbContext _db;

    public MovimientoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IngresoResponse> CrearIngresoAsync(IngresoRequest request)
    {
        if (request.Cantidad <= 0)
            throw new ApiException("La cantidad a ingresar debe ser mayor a cero.");

        if (!await _db.Productos.AnyAsync(p => p.Id == request.ProductoId))
            throw new ApiException("El producto no existe.", StatusCodes.Status404NotFound);

        if (!await _db.Proveedores.AnyAsync(p => p.Id == request.ProveedorId))
            throw new ApiException("El proveedor no existe.", StatusCodes.Status404NotFound);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var ingreso = new Ingreso
        {
            ProductoId = request.ProductoId,
            ProveedorId = request.ProveedorId,
            Cantidad = request.Cantidad,
            Fecha = DateTime.UtcNow,
            Observaciones = request.Observaciones
        };
        _db.Ingresos.Add(ingreso);

        await _db.Productos
            .Where(p => p.Id == request.ProductoId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + request.Cantidad));

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await ObtenerIngresoAsync(ingreso.Id);
    }

    public async Task<SalidaResponse> CrearSalidaAsync(SalidaRequest request)
    {
        if (request.Cantidad <= 0)
            throw new ApiException("La cantidad a vender debe ser mayor a cero.");

        var producto = await _db.Productos.FindAsync(request.ProductoId)
            ?? throw new ApiException("El producto no existe.", StatusCodes.Status404NotFound);

        if (!await _db.Clientes.AnyAsync(c => c.Id == request.ClienteId))
            throw new ApiException("El cliente no existe.", StatusCodes.Status404NotFound);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var filasActualizadas = await _db.Productos
            .Where(p => p.Id == request.ProductoId && p.Stock >= request.Cantidad)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - request.Cantidad));

        if (filasActualizadas == 0)
            throw new ApiException(
                $"Stock insuficiente. El producto \"{producto.Nombre}\" tiene {await _db.Productos.Where(p => p.Id == producto.Id).Select(p => p.Stock).FirstOrDefaultAsync()} unidades disponibles.");

        var salida = new Salida
        {
            ProductoId = request.ProductoId,
            ClienteId = request.ClienteId,
            Cantidad = request.Cantidad,
            PrecioUnitario = producto.Precio,
            Fecha = DateTime.UtcNow,
            Observaciones = request.Observaciones
        };
        _db.Salidas.Add(salida);

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await ObtenerSalidaAsync(salida.Id);
    }

    private async Task<IngresoResponse> ObtenerIngresoAsync(int id)
    {
        return await _db.Ingresos
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new IngresoResponse(
                i.Id,
                i.ProductoId,
                i.Producto.Nombre,
                i.ProveedorId,
                i.Proveedor.Nombre,
                i.Cantidad,
                i.Fecha,
                i.Observaciones))
            .FirstOrDefaultAsync()!;
    }

    private async Task<SalidaResponse> ObtenerSalidaAsync(int id)
    {
        return await _db.Salidas
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SalidaResponse(
                s.Id,
                s.ProductoId,
                s.Producto.Nombre,
                s.ClienteId,
                s.Cliente.Nombre,
                s.Cantidad,
                s.PrecioUnitario,
                s.Cantidad * s.PrecioUnitario,
                s.Fecha,
                s.Observaciones))
            .FirstOrDefaultAsync()!;
    }
}