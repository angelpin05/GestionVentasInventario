namespace GestionVentasInventario.Models
{
    public class Venta
    {
        public int ID { get; set; }
        public DateTime FechaVenta { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string Estado { get; set; } = "Completada";

        // Llave foránea Cliente
        public int ClienteID { get; set; }
        public Cliente? Cliente { get; set; }

        // Llave foránea Usuario
        public string? UsuarioID { get; set; }
        public ApplicationUser? Usuario { get; set; }

        // Navegación
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}