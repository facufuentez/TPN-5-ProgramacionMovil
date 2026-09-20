using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPN_5_MOVIL.Data;
using TPN_5_MOVIL.DTOs;
using TPN_5_MOVIL.Services;

namespace TPN_5_MOVIL.Controllers;

[ApiController]
[Route("api/salidas")]
[Authorize]
public class SalidasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MovimientoService _movimientoService;

    public SalidasController(AppDbContext db, MovimientoService movimientoService)
    {
        _db = db;
        _movimientoService = movimientoService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<SalidaResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? productoId = null,
        [FromQuery] int? clienteId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        var query = _db.Salidas
            .AsNoTracking()
            .AsQueryable();

        if (productoId.HasValue)
            query = query.Where(s => s.ProductoId == productoId);

        if (clienteId.HasValue)
            query = query.Where(s => s.ClienteId == clienteId);

        if (desde.HasValue)
            query = query.Where(s => s.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(s => s.Fecha <= hasta.Value);

        query = query.OrderByDescending(s => s.Fecha);

        var result = await PaginatedResult<SalidaResponse>.CreateAsync(
            query.Select(s => new SalidaResponse(
                s.Id,
                s.ProductoId,
                s.Producto.Nombre,
                s.ClienteId,
                s.Cliente.Nombre,
                s.Cantidad,
                s.PrecioUnitario,
                s.Cantidad * s.PrecioUnitario,
                s.Fecha,
                s.Observaciones)),
            page,
            pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SalidaResponse>> GetById(int id)
    {
        var salida = await _db.Salidas
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
            .FirstOrDefaultAsync();

        return salida is null ? NotFound() : Ok(salida);
    }

    [HttpPost]
    public async Task<ActionResult<SalidaResponse>> Create([FromBody] SalidaRequest request)
    {
        var salida = await _movimientoService.CrearSalidaAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = salida.Id }, salida);
    }
}