using Microsoft.AspNetCore.Mvc;
using TapAndGo.Api.Data;
using TapAndGo.Api.Models;
using TapAndGo.Api.Models.Dto;

namespace TapAndGo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var items = _context.MenuItems.ToList();
            return Ok(items);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CrearMenuItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                dto.PrecioChico <= 0 || dto.PrecioMediano <= 0 || dto.PrecioGrande <= 0)
            {
                return BadRequest("Nombre y precios válidos son obligatorios.");
            }

            var item = new MenuItem
            {
                Nombre = dto.Nombre,
                Categoria = dto.Categoria,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                Imagen = dto.Imagen,
                Stock = dto.Stock,
                Calorias = dto.Calorias,
                PrecioChico = dto.PrecioChico,
                PrecioMediano = dto.PrecioMediano,
                PrecioGrande = dto.PrecioGrande
            };

            _context.MenuItems.Add(item);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { id = item.Id }, item);
        }



        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] CrearMenuItemDto dto)
        {
            var existing = _context.MenuItems.Find(id);
            if (existing == null)
                return NotFound();

            existing.Nombre = dto.Nombre;
            existing.Categoria = dto.Categoria;
            existing.Descripcion = dto.Descripcion;
            existing.Tipo = dto.Tipo;
            existing.Imagen = dto.Imagen;
            existing.Stock = dto.Stock;
            existing.Calorias = dto.Calorias;
            existing.PrecioChico = dto.PrecioChico;
            existing.PrecioMediano = dto.PrecioMediano;
            existing.PrecioGrande = dto.PrecioGrande;

            _context.SaveChanges();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var item = _context.MenuItems.Find(id);
            if (item == null)
                return NotFound();

            _context.MenuItems.Remove(item);
            _context.SaveChanges();
            return NoContent(); // 204
        }


    }
}
