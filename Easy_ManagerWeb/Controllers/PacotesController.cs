using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Easy_ManagerWeb.Controllers
{
    public class PacotesController : Controller
    {
        private readonly AppDbContext _context;

        public PacotesController(AppDbContext context)
        {
            _context = context;
        }

        // Listar pacotes com clientes vinculados
        public IActionResult Gerenciamento_pacotes()
        {
            var pacotes = _context.Pacotes
                                  .Include(p => p.Entregas) // Pacotes com entregas
                                  .ThenInclude(e => e.Cliente) // Clientes vinculados
                                  .OrderByDescending(p => p.DataCadastro)
                                  .ToList();

            return View(pacotes);
        }

        // Formulário para novo pacote
        public IActionResult Novo_pacote()
        {
            return View();
        }

        // Criar novo pacote
        [HttpPost]
        public IActionResult Novo_pacote(Pacote pacote)
        {
            if (ModelState.IsValid)
            {
                pacote.DataCadastro = DateTime.Now;

                _context.Pacotes.Add(pacote);
                _context.SaveChanges();

                return RedirectToAction("Gerenciamento_pacotes");
            }

            return View(pacote);
        }
    }
}
