using TapAndGo.Api.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relación: un cliente puede tener muchos pedidos
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
