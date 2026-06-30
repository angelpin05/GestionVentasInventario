using GestionVentasInventario.Data;
using GestionVentasInventario.Helpers;
using GestionVentasInventario.Models;
using GestionVentasInventario.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionVentasInventario.Controllers
{
    [Authorize(Roles = "Administrador,Vendedor")]
    public class VentaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CarritoKey = "Carrito";

        public VentaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // LISTAR VENTAS
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
            return View(ventas);
        }

        // DETALLE DE VENTA
        public async Task<IActionResult> Detalle(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.ID == id);

            if (venta == null) return NotFound();
            return View(venta);
        }

        // NUEVA VENTA GET
        public IActionResult Crear()
        {
            var carrito = SessionHelper.GetObject<List<CarritoItem>>(HttpContext.Session, CarritoKey)
                          ?? new List<CarritoItem>();

            var productos = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Estado && p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToList();

            ViewBag.Productos = productos;
            ViewBag.Carrito = carrito;
            ViewBag.Total = carrito.Sum(c => c.Subtotal);
            return View();
        }

        // AGREGAR AL CARRITO
        [HttpPost]
        public IActionResult AgregarAlCarrito(int productoId, int cantidad)
        {
            var producto = _context.Productos.Find(productoId);
            if (producto == null || cantidad <= 0)
                return Json(new { exito = false, mensaje = "Producto no válido." });

            if (cantidad > producto.Stock)
                return Json(new { exito = false, mensaje = $"Stock insuficiente. Disponible: {producto.Stock}" });

            var carrito = SessionHelper.GetObject<List<CarritoItem>>(HttpContext.Session, CarritoKey)
                          ?? new List<CarritoItem>();

            var itemExistente = carrito.FirstOrDefault(c => c.ProductoID == productoId);
            if (itemExistente != null)
            {
                int nuevaCantidad = itemExistente.Cantidad + cantidad;
                if (nuevaCantidad > producto.Stock)
                    return Json(new { exito = false, mensaje = $"Stock insuficiente. Disponible: {producto.Stock}" });
                itemExistente.Cantidad = nuevaCantidad;
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    ProductoID = producto.ID,
                    Nombre = producto.Nombre,
                    PrecioUnitario = producto.Precio,
                    Cantidad = cantidad,
                    ImagenUrl = producto.ImagenUrl
                });
            }

            SessionHelper.SetObject(HttpContext.Session, CarritoKey, carrito);
            return Json(new { exito = true, totalItems = carrito.Count, total = carrito.Sum(c => c.Subtotal) });
        }

        // QUITAR DEL CARRITO
        [HttpPost]
        public IActionResult QuitarDelCarrito(int productoId)
        {
            var carrito = SessionHelper.GetObject<List<CarritoItem>>(HttpContext.Session, CarritoKey)
                          ?? new List<CarritoItem>();

            carrito.RemoveAll(c => c.ProductoID == productoId);
            SessionHelper.SetObject(HttpContext.Session, CarritoKey, carrito);
            return Json(new { exito = true, totalItems = carrito.Count, total = carrito.Sum(c => c.Subtotal) });
        }

        // LIMPIAR CARRITO
        [HttpPost]
        public IActionResult LimpiarCarrito()
        {
            HttpContext.Session.Remove(CarritoKey);
            return Json(new { exito = true });
        }

        // CONFIRMAR VENTA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int clienteId, string metodoPago)
        {
            var carrito = SessionHelper.GetObject<List<CarritoItem>>(HttpContext.Session, CarritoKey)
                          ?? new List<CarritoItem>();

            if (!carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío.";
                return RedirectToAction(nameof(Crear));
            }

            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no válido.";
                return RedirectToAction(nameof(Crear));
            }

            var usuarioActual = await _userManager.GetUserAsync(User);

            // Usar transacción para garantizar consistencia
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = new Venta
                {
                    ClienteID = clienteId,
                    UsuarioID = usuarioActual?.Id,
                    FechaVenta = DateTime.Now,
                    MetodoPago = metodoPago,
                    Estado = "Completada",
                    Total = carrito.Sum(c => c.Subtotal)
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                foreach (var item in carrito)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoID);
                    if (producto == null || producto.Stock < item.Cantidad)
                        throw new Exception($"Stock insuficiente para: {item.Nombre}");

                    producto.Stock -= item.Cantidad;
                    _context.Productos.Update(producto);

                    _context.DetallesVenta.Add(new DetalleVenta
                    {
                        VentaID = venta.ID,
                        ProductoID = item.ProductoID,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Subtotal = item.Subtotal
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                HttpContext.Session.Remove(CarritoKey);
                TempData["Exito"] = $"Venta #{venta.ID} registrada correctamente.";
                return RedirectToAction(nameof(Detalle), new { id = venta.ID });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Crear));
            }
        }
    }
}