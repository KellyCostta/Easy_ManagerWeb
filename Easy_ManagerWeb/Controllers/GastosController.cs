using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Easy_ManagerWeb.Controllers
{
    public class GastosController : Controller
    {
        private readonly AppDbContext _context;

        public GastosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Gasto/Novo_Gasto
        public IActionResult Novo_Gasto()
        {
            return View();
        }

        // POST: Gasto/Novo_Gasto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Novo_Gasto(Gasto gasto)
        {
            if (ModelState.IsValid)
            {
                _context.Gasto.Add(gasto);
                _context.SaveChanges();
                return RedirectToAction("Gerenciamento_Gastos");
            }

            return View(gasto);
        }

        // GET: Gasto
        public IActionResult Gerenciamento_Gastos()
        {
            var lista = _context.Gasto.ToList();
            return View(lista);
        }
    }
}
