using GestionVentasInventario.Data;
using GestionVentasInventario.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestionVentasInventario.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductoController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // LISTAR
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
            return View(productos);
        }

        // CREAR GET
        public IActionResult Crear()
        {
            ViewBag.Categorias = new SelectList(_context.Categorias.Where(c => c.Estado), "ID", "Nombre");
            return View();
        }

        // CREAR POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto producto, IFormFile? imagenFile)
        {
            if (ModelState.IsValid)
            {
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    string carpeta = Path.Combine(_env.WebRootPath, "imagenes", "productos");
                    Directory.CreateDirectory(carpeta);
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagenFile.FileName);
                    string rutaCompleta = Path.Combine(carpeta, nombreArchivo);
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        await imagenFile.CopyToAsync(stream);
                    producto.ImagenUrl = "/imagenes/productos/" + nombreArchivo;
                }

                producto.FechaRegistro = DateTime.Now;
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Producto creado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categorias = new SelectList(_context.Categorias.Where(c => c.Estado), "ID", "Nombre");
            return View(producto);
        }

        // EDITAR GET
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            ViewBag.Categorias = new SelectList(_context.Categorias.Where(c => c.Estado), "ID", "Nombre", producto.CategoriaID);
            return View(producto);
        }

        // EDITAR POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Producto producto, IFormFile? imagenFile)
        {
            if (id != producto.ID) return NotFound();

            if (ModelState.IsValid)
            {
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    string carpeta = Path.Combine(_env.WebRootPath, "imagenes", "productos");
                    Directory.CreateDirectory(carpeta);
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagenFile.FileName);
                    string rutaCompleta = Path.Combine(carpeta, nombreArchivo);
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        await imagenFile.CopyToAsync(stream);
                    producto.ImagenUrl = "/imagenes/productos/" + nombreArchivo;
                }

                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Producto actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categorias = new SelectList(_context.Categorias.Where(c => c.Estado), "ID", "Nombre", producto.CategoriaID);
            return View(producto);
        }

        // ELIMINAR (desactivar)
        public async Task<IActionResult> Eliminar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            producto.Estado = false;
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Producto desactivado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}