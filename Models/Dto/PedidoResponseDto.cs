namespace TapAndGo.Api.Models
{
    public class PedidoResponseDto
    {
        public int Id { get; set; }
        public string Cliente { get; set; }
        public decimal Total { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;

        public List<PedidoItemDto> Detalles { get; set; }
    }

    public class PedidoItemDto
    {
        public int MenuItemId { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string Tamano { get; set; }
    }
}
