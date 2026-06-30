using GestionVentasInventario.Data;
using GestionVentasInventario.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionVentasInventario.Controllers
{
    [Authorize(Roles = "Administrador,Vendedor")]
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClienteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTAR
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes
                .OrderBy(c => c.NombreCompleto)
                .ToListAsync();
            return View(clientes);
        }

        // CREAR GET
        public IActionResult Crear()
        {
            return View();
        }

        // CREAR POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Cliente cliente)
        {
            // Verificar DNI duplicado
            bool dniExiste = await _context.Clientes
                .AnyAsync(c => c.DNI == cliente.DNI);
            if (dniExiste)
            {
                ModelState.AddModelError("DNI", "Ya existe un cliente con ese DNI.");
            }

            // Verificar Email duplicado si se ingresó
            if (!string.IsNullOrEmpty(cliente.Email))
            {
                bool emailExiste = await _context.Clientes
                    .AnyAsync(c => c.Email == cliente.Email);
                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Ya existe un cliente con ese email.");
                }
            }

            if (ModelState.IsValid)
            {
                cliente.FechaRegistro = DateTime.Now;
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Cliente registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // EDITAR GET
        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // EDITAR POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Cliente cliente)
        {
            if (id != cliente.ID) return NotFound();

            // Verificar DNI duplicado excluyendo el cliente actual
            bool dniExiste = await _context.Clientes
                .AnyAsync(c => c.DNI == cliente.DNI && c.ID != cliente.ID);
            if (dniExiste)
            {
                ModelState.AddModelError("DNI", "Ya existe otro cliente con ese DNI.");
            }

            // Verificar Email duplicado excluyendo el cliente actual
            if (!string.IsNullOrEmpty(cliente.Email))
            {
                bool emailExiste = await _context.Clientes
                    .AnyAsync(c => c.Email == cliente.Email && c.ID != cliente.ID);
                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Ya existe otro cliente con ese email.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Clientes.Update(cliente);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Cliente actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // DESACTIVAR
        public async Task<IActionResult> Eliminar(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            cliente.Estado = false;
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Cliente desactivado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // BUSCAR POR DNI (para usarlo después en Ventas)
        [HttpGet]
        public async Task<IActionResult> BuscarPorDni(string dni)
        {
            var cliente = await _context.Clientes
                .Where(c => c.DNI == dni && c.Estado)
                .Select(c => new { c.ID, c.NombreCompleto, c.DNI, c.Email, c.Telefono })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return Json(new { encontrado = false });

            return Json(new { encontrado = true, cliente });
        }
    }
}