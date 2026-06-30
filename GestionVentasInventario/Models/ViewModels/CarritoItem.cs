namespace GestionVentasInventario.Models.ViewModels
{
    public class CarritoItem
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public string? ImagenUrl { get; set; }
        public decimal Subtotal => PrecioUnitario * Cantidad;
    }
}