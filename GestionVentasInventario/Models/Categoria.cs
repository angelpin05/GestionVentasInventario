namespace GestionVentasInventario.Models
{
    public class Categoria
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; } = true;

        // Navegación
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}