using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TapAndGo.Api.Data;
using TapAndGo.Api.Models;
using TapAndGo.Api.Models.Dto;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        if (_context.Usuarios.Any(u => u.Email == dto.Email))
            return BadRequest("El usuario ya existe.");

        var user = new Usuario
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash),
            Rol = string.IsNullOrWhiteSpace(dto.Rol) ? "cliente" : dto.Rol
        };

        _context.Usuarios.Add(user);
        _context.SaveChanges();

        return Ok("Usuario registrado");
    }



    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto login)
    {
        var existingUser = _context.Usuarios.FirstOrDefault(u => u.Email == login.Email);
        if (existingUser == null || !BCrypt.Net.BCrypt.Verify(login.PasswordHash, existingUser.PasswordHash))
            return Unauthorized("Credenciales incorrectas");

        var claims = new[]
        {
                new Claim(ClaimTypes.Name, existingUser.Email),
                new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                new Claim(ClaimTypes.Role, existingUser.Rol) 
         };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;

        if (identity == null)
            return Unauthorized();

        var id = int.Parse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = _context.Usuarios.FirstOrDefault(u => u.Id == id);

        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            nombre = user.Name, 
            rol = user.Rol
        });
    }

    [Authorize(Roles = "admin")]
    [HttpGet("users")]
    public IActionResult GetUsuarios()
    {
        var usuarios = _context.Usuarios
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Rol
            })
            .ToList();

        return Ok(usuarios);
    }


    [Authorize(Roles = "admin")]
    [HttpPut("users/{id}")]
    public IActionResult UpdateUsuario(int id, [FromBody] ActualizarUsuarioDto dto)
    {
        var user = _context.Usuarios.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound();

        user.Email = dto.Email;
        user.Rol = dto.Rol;
        user.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);

        _context.SaveChanges();
        return NoContent();
    }



    [Authorize(Roles = "admin")]
    [HttpDelete("users/{id}")]
    public IActionResult DeleteUsuario(int id)
    {
        var user = _context.Usuarios.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound();

        _context.Usuarios.Remove(user);
        _context.SaveChanges();
        return NoContent();
    }

    [Authorize]
    [HttpPut("me/password")]
    public IActionResult CambiarContraseña([FromBody] CambiarPasswordDto datos)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = _context.Usuarios.FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return Unauthorized("Usuario no encontrado.");

        // Verificar si la contraseña actual es correcta
        if (!BCrypt.Net.BCrypt.Verify(datos.PasswordActual, user.PasswordHash))
            return BadRequest("Contraseña actual incorrecta.");

        // Si la contraseña nueva es válida, la actualizamos
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(datos.NuevaPassword);
        _context.SaveChanges();

        return NoContent();
    }


    [HttpPost("cliente")]
    public IActionResult LoginCliente([FromBody] LoginClienteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("El nombre es obligatorio.");

        // Buscar cliente existente
        var cliente = _context.Clientes.FirstOrDefault(c => c.Nombre == dto.Nombre);

        if (cliente == null)
        {
            // Si no existe, lo creamos
            cliente = new Cliente { Nombre = dto.Nombre, FechaRegistro = DateTime.UtcNow };
            _context.Clientes.Add(cliente);
            _context.SaveChanges();
        }

        // Crear token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, cliente.Nombre),
        new Claim(ClaimTypes.Role, "cliente"),
        new Claim("ClienteId", cliente.Id.ToString())
    }),
            Expires = DateTime.UtcNow.AddHours(12),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = "TapAndGo", 
            Audience = "TapAndGoClientes" 
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            id = cliente.Id,
            nombre = cliente.Nombre,
            fechaRegistro = cliente.FechaRegistro
        });
    }
}


