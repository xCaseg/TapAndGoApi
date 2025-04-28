using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapAndGo.Api.Data;
using TapAndGo.Api.Models;

namespace TapAndGo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "mesero,cliente,cocina")]
        [HttpPost]
        public IActionResult CrearPedido([FromBody] CrearPedidoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Cliente) || dto.Detalles == null || !dto.Detalles.Any())
                return BadRequest("El cliente y los detalles del pedido son obligatorios.");

            // Buscar o crear el cliente
            var cliente = _context.Clientes.FirstOrDefault(c => c.Nombre == dto.Cliente);
            if (cliente == null)
            {
                cliente = new Cliente { Nombre = dto.Cliente };
                _context.Clientes.Add(cliente);
                _context.SaveChanges();
            }

            decimal total = 0;
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Fecha = DateTime.UtcNow,
                Detalles = new List<PedidoDetalle>()
            };

            foreach (var d in dto.Detalles)
            {
                var menuItem = _context.MenuItems.Find(d.MenuItemId);
                if (menuItem == null)
                    return BadRequest($"MenuItemId inválido: {d.MenuItemId}");

                var precioUnitario = d.Tamano.ToLower() switch
                {
                    "chico" => menuItem.PrecioChico,
                    "grande" => menuItem.PrecioGrande,
                    _ => menuItem.PrecioMediano
                };

                total += d.Cantidad * precioUnitario;

                pedido.Detalles.Add(new PedidoDetalle
                {
                    MenuItemId = d.MenuItemId,
                    Cantidad = d.Cantidad,
                    Tamano = d.Tamano
                });
            }

            pedido.Total = total;

            _context.Pedidos.Add(pedido);
            _context.SaveChanges();

            var response = new PedidoResponseDto
            {
                Id = pedido.Id,
                Cliente = cliente.Nombre, // usamos el nombre del cliente desde la entidad
                Total = pedido.Total,
                Fecha = pedido.Fecha,
                Detalles = pedido.Detalles.Select(det => new PedidoItemDto
                {
                    MenuItemId = det.MenuItemId,
                    Nombre = det.MenuItem?.Nombre,
                    Precio = CalcularPrecioPorTamano(det),
                    Cantidad = det.Cantidad,
                    Tamano = det.Tamano
                }).ToList()
            };

            return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, response);
        }


        [Authorize(Roles = "admin,mesero,cliente,cocina")]
        [HttpGet("{id}")]
        public IActionResult GetPedido(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.MenuItem)
                .FirstOrDefault(p => p.Id == id);

            if (pedido == null)
                return NotFound();

            var dto = new PedidoResponseDto
            {
                Id = pedido.Id,
                Cliente = pedido.Cliente.Nombre,
                Total = pedido.Total,
                Fecha = pedido.Fecha,
                Detalles = pedido.Detalles.Select(det => new PedidoItemDto
                {
                    MenuItemId = det.MenuItemId,
                    Nombre = det.MenuItem?.Nombre,
                    Precio = CalcularPrecioPorTamano(det), 
                    Cantidad = det.Cantidad,
                }).ToList()
            };

            return Ok(dto);
        }

        // bearer + token 
        [Authorize(Roles = "mesero,cocina,admin")]
        [HttpGet]
        public IActionResult GetTodos()
        {
            var pedidos = _context.Pedidos
                 .Include(p => p.Cliente) 
                .Include(p => p.Detalles)
                .ThenInclude(d => d.MenuItem)
                .ToList();

            var result = pedidos.Select(p => new PedidoResponseDto
            {
                Id = p.Id,
                Cliente = p.Cliente.Nombre,
                Total = p.Total,
                Fecha = p.Fecha,
                Estado = p.Estado, 
                Detalles = p.Detalles.Select(d => new PedidoItemDto
                {
                    MenuItemId = d.MenuItemId,
                    Nombre = d.MenuItem?.Nombre,
                    Tamano = d.Tamano,
                    Precio = CalcularPrecioPorTamano(d),
                    Cantidad = d.Cantidad
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [Authorize(Roles = "admin,mesero,cocina")]
        [HttpDelete("{id}")]
        public IActionResult DeletePedido(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Detalles) 
                .FirstOrDefault(p => p.Id == id);

            if (pedido == null)
                return NotFound("Pedido no encontrado.");

            _context.PedidoDetalles.RemoveRange(pedido.Detalles); 
            _context.Pedidos.Remove(pedido);                      

            _context.SaveChanges();

            return NoContent(); // 204
        }


        [Authorize(Roles = "admin,mesero,cliente,cocina")]
        private decimal CalcularPrecioPorTamano(PedidoDetalle detalle)
        {
            if (detalle.MenuItem == null || string.IsNullOrWhiteSpace(detalle.Tamano))
                return detalle.MenuItem?.PrecioMediano ?? 0;

            return detalle.Tamano.ToLower() switch
            {
                "chico" => detalle.MenuItem.PrecioChico,
                "grande" => detalle.MenuItem.PrecioGrande,
                _ => detalle.MenuItem.PrecioMediano
            };
        }

        [Authorize(Roles = "admin,cocina,mesero")]
        [HttpGet("filtrar")]
        public IActionResult FiltrarPedidos(
        [FromQuery] string? cliente,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] decimal? minTotal,
        [FromQuery] decimal? maxTotal)
        {
            var query = _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.MenuItem)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(cliente))
            {
                query = query.Where(p => p.Cliente.Nombre.Contains(cliente));
            }

            if (desde.HasValue)
            {
                query = query.Where(p => p.Fecha >= desde.Value);
            }

            if (hasta.HasValue)
            {
                query = query.Where(p => p.Fecha <= hasta.Value);
            }

            if (minTotal.HasValue)
            {
                query = query.Where(p => p.Total >= minTotal.Value);
            }

            if (maxTotal.HasValue)
            {
                query = query.Where(p => p.Total <= maxTotal.Value);
            }

            var pedidos = query.ToList();

            var result = pedidos.Select(p => new PedidoResponseDto
            {
                Id = p.Id,
                Cliente = p.Cliente.Nombre,
                Total = p.Total,
                Fecha = p.Fecha,
                Detalles = p.Detalles.Select(d => new PedidoItemDto
                {
                    MenuItemId = d.MenuItemId,
                    Nombre = d.MenuItem?.Nombre,
                    Precio = CalcularPrecioPorTamano(d),
                    Cantidad = d.Cantidad,
                    Tamano = d.Tamano
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [Authorize(Roles = "cocina")]
        [HttpPut("{id}/estado")]
        public IActionResult ActualizarEstado(int id, [FromBody] EstadoDto dto)
        {
            var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == id);

            if (pedido == null)
                return NotFound("Pedido no encontrado.");

            pedido.Estado = dto.Estado;
            _context.SaveChanges();

            return NoContent();
        }


    }
}
