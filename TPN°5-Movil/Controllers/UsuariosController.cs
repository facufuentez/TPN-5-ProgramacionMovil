using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPN_5_MOVIL.Data;
using TPN_5_MOVIL.DTOs;
using TPN_5_MOVIL.Entities;
using TPN_5_MOVIL.Exceptions;

namespace TPN_5_MOVIL.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = Usuario.RolAdmin)]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public UsuariosController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<UsuarioDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? busqueda = null)
    {
        var query = _db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(u => u.NombreCompleto.Contains(busqueda) || u.Email.Contains(busqueda));

        var result = await PaginatedResult<UsuarioDto>.CreateAsync(
            query.Select(u => new UsuarioDto(u.Id, u.NombreCompleto, u.Email, u.Rol, u.Activo)),
            page,
            pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UsuarioDto(u.Id, u.NombreCompleto, u.Email, u.Rol, u.Activo))
            .FirstOrDefaultAsync();

        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Create([FromBody] UsuarioCreateRequest request)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == request.Email))
            throw new ApiException("Ya existe un usuario con ese email.", StatusCodes.Status409Conflict);

        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto,
            Email = request.Email,
            Rol = request.Rol == Usuario.RolAdmin ? Usuario.RolAdmin : Usuario.RolUsuario,
            Activo = true
        };
        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Password);

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = usuario.Id },
            new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Email, usuario.Rol, usuario.Activo));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateRequest request)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null)
            return NotFound();

        if (await _db.Usuarios.AnyAsync(u => u.Email == request.Email && u.Id != id))
            throw new ApiException("Ya existe un usuario con ese email.", StatusCodes.Status409Conflict);

        usuario.NombreCompleto = request.NombreCompleto;
        usuario.Email = request.Email;
        usuario.Rol = request.Rol == Usuario.RolAdmin ? Usuario.RolAdmin : Usuario.RolUsuario;
        usuario.Activo = request.Activo;

        if (!string.IsNullOrWhiteSpace(request.Password))
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Password);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null)
            return NotFound();

        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}