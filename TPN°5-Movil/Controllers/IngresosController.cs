using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPN_5_MOVIL.Data;
using TPN_5_MOVIL.DTOs;
using TPN_5_MOVIL.Entities;
using TPN_5_MOVIL.Services;

namespace TPN_5_MOVIL.Controllers;

[ApiController]
[Route("api/ingresos")]
[Authorize]
public class IngresosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MovimientoService _movimientoService;

    public IngresosController(AppDbContext db, MovimientoService movimientoService)
    {
        _db = db;
        _movimientoService = movimientoService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<IngresoResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? productoId = null,
        [FromQuery] int? proveedorId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        var query = _db.Ingresos
            .AsNoTracking()
            .AsQueryable();

        if (productoId.HasValue)
            query = query.Where(i => i.ProductoId == productoId);

        if (proveedorId.HasValue)
            query = query.Where(i => i.ProveedorId == proveedorId);

        if (desde.HasValue)
            query = query.Where(i => i.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(i => i.Fecha <= hasta.Value);

        query = query.OrderByDescending(i => i.Fecha);

        var result = await PaginatedResult<IngresoResponse>.CreateAsync(
            query.Select(i => new IngresoResponse(
                i.Id,
                i.ProductoId,
                i.Producto.Nombre,
                i.ProveedorId,
                i.Proveedor.Nombre,
                i.Cantidad,
                i.Fecha,
                i.Observaciones)),
            page,
            pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IngresoResponse>> GetById(int id)
    {
        var ingreso = await _db.Ingresos
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
            .FirstOrDefaultAsync();

        return ingreso is null ? NotFound() : Ok(ingreso);
    }

    [HttpPost]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<ActionResult<IngresoResponse>> Create([FromBody] IngresoRequest request)
    {
        var ingreso = await _movimientoService.CrearIngresoAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = ingreso.Id }, ingreso);
    }
}