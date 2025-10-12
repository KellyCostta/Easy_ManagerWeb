using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
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

        // Listar pacotes
        public IActionResult Gerenciamento_pacotes()
        {
            List<Pacote> pacotes = [.. _context.Pacotes.OrderByDescending(p => p.DataCadastro)];

            return View(pacotes);
        }

        // Formulário para novo pacote
        public IActionResult Novo_pacote()
        {
            return View(); // Apenas abre o formulário
        }

        // Criar novo pacote
        [HttpPost]
        public IActionResult Novo_pacote(Pacote pacote)
        {
            if (ModelState.IsValid)
            {
                // Define a data de cadastro ao criar
                pacote.DataCadastro = DateTime.Now;

                _context.Pacotes.Add(pacote);
                _context.SaveChanges();

                return RedirectToAction("Gerenciamento_pacotes");
            }

            // Se houver erro, retorna para o formulário com os dados preenchidos
            return View(pacote);
        }
    }
}
