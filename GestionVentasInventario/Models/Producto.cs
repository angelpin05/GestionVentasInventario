namespace GestionVentasInventario.Models
{
    public class Producto
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? ImagenUrl { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Llave foránea
        public int CategoriaID { get; set; }
        public Categoria? Categoria { get; set; }
    }
}
