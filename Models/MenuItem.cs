namespace TapAndGo.Api.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public string Imagen { get; set; }
        public decimal Stock { get; set; }
        public decimal Calorias { get; set; }
        public decimal PrecioChico { get; set; }
        public decimal PrecioMediano { get; set; }
        public decimal PrecioGrande { get; set; }

        public ICollection<PedidoDetalle> Detalles { get; set; }

    }

}
