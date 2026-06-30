using GestionVentasInventario.Data;
using GestionVentasInventario.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionVentasInventario.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTAR
        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();
            return View(categorias);
        }

        // CREAR GET
        public IActionResult Crear()
        {
            return View();
        }

        // CREAR POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Categoría creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // EDITAR GET
        public async Task<IActionResult> Editar(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        // EDITAR POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Categoria categoria)
        {
            if (id != categoria.ID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Categorias.Update(categoria);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Categoría actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // ELIMINAR (desactivar)
        public async Task<IActionResult> Eliminar(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            categoria.Estado = false;
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Categoría desactivada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}