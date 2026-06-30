namespace GestionVentasInventario.Models
{
    public class DetalleVenta
    {
        public int ID { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Llave foránea Venta
        public int VentaID { get; set; }
        public Venta? Venta { get; set; }

        // Llave foránea Producto
        public int ProductoID { get; set; }
        public Producto? Producto { get; set; }
    }
}
