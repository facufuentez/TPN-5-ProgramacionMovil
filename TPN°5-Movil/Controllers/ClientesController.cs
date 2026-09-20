using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPN_5_MOVIL.Data;
using TPN_5_MOVIL.DTOs;
using TPN_5_MOVIL.Entities;
using TPN_5_MOVIL.Exceptions;

namespace TPN_5_MOVIL.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClientesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ClienteResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? busqueda = null)
    {
        var query = _db.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(c => c.Nombre.Contains(busqueda) || c.CUIT.Contains(busqueda) || c.Email!.Contains(busqueda));

        var result = await PaginatedResult<ClienteResponse>.CreateAsync(
            query.Select(c => new ClienteResponse(c.Id, c.Nombre, c.CUIT, c.Email, c.Telefono, c.Direccion)),
            page,
            pageSize);
        return Ok(result);
    }

    [HttpGet("catalogo")]
    public async Task<ActionResult<IEnumerable<ClienteResponse>>> GetCatalogo()
    {
        var clientes = await _db.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new ClienteResponse(c.Id, c.Nombre, c.CUIT, c.Email, c.Telefono, c.Direccion))
            .ToListAsync();
        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> GetById(int id)
    {
        var cliente = await _db.Clientes
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ClienteResponse(c.Id, c.Nombre, c.CUIT, c.Email, c.Telefono, c.Direccion))
            .FirstOrDefaultAsync();

        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<ActionResult<ClienteResponse>> Create([FromBody] ClienteRequest request)
    {
        if (await _db.Clientes.AnyAsync(c => c.CUIT == request.CUIT))
            throw new ApiException("Ya existe un cliente con ese CUIT.", StatusCodes.Status409Conflict);

        var cliente = new Cliente
        {
            Nombre = request.Nombre,
            CUIT = request.CUIT,
            Email = request.Email,
            Telefono = request.Telefono,
            Direccion = request.Direccion
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cliente.Id },
            new ClienteResponse(cliente.Id, cliente.Nombre, cliente.CUIT, cliente.Email, cliente.Telefono, cliente.Direccion));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<IActionResult> Update(int id, [FromBody] ClienteRequest request)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound();

        if (await _db.Clientes.AnyAsync(c => c.CUIT == request.CUIT && c.Id != id))
            throw new ApiException("Ya existe un cliente con ese CUIT.", StatusCodes.Status409Conflict);

        cliente.Nombre = request.Nombre;
        cliente.CUIT = request.CUIT;
        cliente.Email = request.Email;
        cliente.Telefono = request.Telefono;
        cliente.Direccion = request.Direccion;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound();

        if (await _db.Salidas.AnyAsync(s => s.ClienteId == id))
            throw new ApiException("No se puede eliminar: el cliente tiene ventas asociadas.", StatusCodes.Status409Conflict);

        _db.Clientes.Remove(cliente);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}