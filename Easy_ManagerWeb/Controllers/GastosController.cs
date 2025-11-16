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

        // GET: NovoGasto
        public IActionResult Novo_Gasto()
        {
            return View();
        }

        // POST: Novo_Gasto
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

        [HttpPost]
        public IActionResult Excluir_Gasto(int id)
        {
            var gasto = _context.Gasto.Find(id);

            if (gasto == null)
                return NotFound();

            _context.Gasto.Remove(gasto);
            _context.SaveChanges();

            return RedirectToAction("Gerenciamento_Gastos");
        }
        public JsonResult BuscarGasto(int id)
        {
            var gasto = _context.Gasto.Find(id);
            return Json(gasto);
        }

        [HttpPost]
        public JsonResult Editar_Gasto_Ajax([FromBody] Gasto gasto)
        {
            var g = _context.Gasto.Find(gasto.IdGasto);

            if (g == null)
                return Json(new { sucesso = false });

            g.Tipo = gasto.Tipo;
            g.Valor = gasto.Valor;
            g.DataGasto = gasto.DataGasto;

            _context.SaveChanges();

            return Json(new { sucesso = true });
        }


    }
}
