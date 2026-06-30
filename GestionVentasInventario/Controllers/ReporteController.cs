using GestionVentasInventario.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionVentasInventario.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReporteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReporteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // VISTA PRINCIPAL
        public IActionResult Index()
        {
            return View();
        }

        // VENTAS POR MES (últimos 6 meses)
        [HttpGet]
        public async Task<IActionResult> VentasPorMes()
        {
            var hace6Meses = DateTime.Now.AddMonths(-6);

            var datos = await _context.Ventas
                .Where(v => v.FechaVenta >= hace6Meses)
                .GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                .Select(g => new
                {
                    Año = g.Key.Year,
                    Mes = g.Key.Month,
                    Total = g.Sum(v => v.Total),
                    Cantidad = g.Count()
                })
                .OrderBy(g => g.Año).ThenBy(g => g.Mes)
                .ToListAsync();

            var labels = datos.Select(d =>
                new DateTime(d.Año, d.Mes, 1).ToString("MMM yyyy")).ToList();
            var totales = datos.Select(d => d.Total).ToList();
            var cantidades = datos.Select(d => d.Cantidad).ToList();

            return Json(new { labels, totales, cantidades });
        }

        // PRODUCTOS MÁS VENDIDOS (top 5)
        [HttpGet]
        public async Task<IActionResult> ProductosMasVendidos()
        {
            var datos = await _context.DetallesVenta
                .Include(d => d.Producto)
                .GroupBy(d => new { d.ProductoID, d.Producto!.Nombre })
                .Select(g => new
                {
                    Nombre = g.Key.Nombre,
                    TotalVendido = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(g => g.TotalVendido)
                .Take(5)
                .ToListAsync();

            var labels = datos.Select(d => d.Nombre).ToList();
            var valores = datos.Select(d => d.TotalVendido).ToList();

            return Json(new { labels, valores });
        }

        // VENTAS POR MÉTODO DE PAGO
        [HttpGet]
        public async Task<IActionResult> VentasPorMetodoPago()
        {
            var datos = await _context.Ventas
                .GroupBy(v => v.MetodoPago)
                .Select(g => new
                {
                    Metodo = g.Key,
                    Total = g.Sum(v => v.Total),
                    Cantidad = g.Count()
                })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            var labels = datos.Select(d => d.Metodo).ToList();
            var valores = datos.Select(d => d.Total).ToList();

            return Json(new { labels, valores });
        }

        // RESUMEN GENERAL (tarjetas)
        [HttpGet]
        public async Task<IActionResult> ResumenGeneral()
        {
            var totalVentas = await _context.Ventas.CountAsync();
            var ingresoTotal = await _context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0;
            var totalClientes = await _context.Clientes.CountAsync(c => c.Estado);
            var totalProductos = await _context.Productos.CountAsync(p => p.Estado);
            var productosStockBajo = await _context.Productos.CountAsync(p => p.Estado && p.Stock <= 5);

            return Json(new
            {
                totalVentas,
                ingresoTotal,
                totalClientes,
                totalProductos,
                productosStockBajo
            });
        }
    }
}