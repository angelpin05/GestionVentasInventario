namespace GestionVentasInventario.Models
{
    public class Cliente
    {
        public int ID { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Navegación
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
