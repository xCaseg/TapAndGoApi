using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TapAndGo.Api.Data;
using TapAndGo.Api.Models;

namespace TapAndGo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<IActionResult> CrearCliente([FromBody] Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
                return BadRequest("El nombre es obligatorio.");

            cliente.FechaRegistro = DateTime.UtcNow;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return Ok(cliente);
        }

        // GET: api/Clientes
        [Authorize(Roles = "mesero,cocina,admin")]
        [HttpGet]
        public IActionResult ListarClientes()
        {
            var clientes = _context.Clientes.ToList();
            return Ok(clientes);
        }
    }
}
