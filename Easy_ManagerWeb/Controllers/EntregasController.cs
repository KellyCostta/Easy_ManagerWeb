using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Easy_ManagerWeb.Controllers
{
    public class EntregasController : Controller
    {
        private readonly AppDbContext _context;

        public EntregasController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Listagem de entregas
        public IActionResult Gerenciamento_entregas()
        {
            var entregas = _context.Entregas
                                   .Include(e => e.Cliente)
                                   .Include(e => e.Pacote)
                                   .ToList();
            return View(entregas);
        }

        // GET: Nova entrega
        public IActionResult Nova_entrega()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome");
            ViewBag.Pacotes = new SelectList(_context.Pacotes.ToList(), "Id", "Descricao");
            return View();
        }

        // POST: Nova entrega
        [HttpPost]
        public IActionResult Nova_entrega(Entrega entrega)
        {
            if (ModelState.IsValid)
            {
                _context.Entregas.Add(entrega);
                _context.SaveChanges();
                return RedirectToAction("Gerenciamento_entregas");
            }

            // Recarrega dropdowns em caso de erro
            ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome", entrega.ClienteId);
            ViewBag.Pacotes = new SelectList(_context.Pacotes.ToList(), "Id", "Descricao", entrega.PacoteId);

            return View(entrega);
        }
    }
}
