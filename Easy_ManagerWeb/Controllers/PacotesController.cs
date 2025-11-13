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

        // GET: Listar pacotes
        public IActionResult Gerenciamento_pacotes()
        {
            var pacotes = _context.Pacotes
                                  .Include(p => p.Entregas) // Pacotes com entregas
                             
                                  .ToList();

            return View(pacotes);
        }

        // GET: Formulário para novo pacote
        public IActionResult Novo_pacote()
        {
            return View();
        }

        // POST: Criar novo pacote
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: /Pacotes/Editar_Pacote/5 (Exibir formulário de edição)
        // GET: /Pacotes/Editar_Pacote/5 (Exibir formulário de edição)
        public IActionResult Editar_Pacote(int id)
        {
            var pacote = _context.Pacotes.FirstOrDefault(p => p.Id == id);

            if (pacote == null)
            {
                return NotFound();
            }

            // ⚠️ Limpa o ModelState para impedir que o Razor use valores antigos
            ModelState.Clear();

            return View(pacote);
        }

        // POST: /Pacotes/Editar_Pacote/5 (Processar edição)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar_Pacote(Pacote pacote)
        {
            if (ModelState.IsValid)
            {
                // A data de cadastro é mantida
                _context.Pacotes.Update(pacote);
                _context.SaveChanges();

                return RedirectToAction("Gerenciamento_pacotes");
            }

            return View(pacote);
        }

        // GET: /Pacotes/Excluir_Pacote/5 (Exibir tela de confirmação)
        public IActionResult Excluir_Pacote(int id)
        {
            var pacote = _context.Pacotes.FirstOrDefault(p => p.Id == id);

            if (pacote == null)
            {
                return NotFound();
            }

            return View(pacote);
        }

        // POST: /Pacotes/Excluir_Pacote/5 (Executar a exclusão)
        // ActionName para que o POST chame este método.
        [HttpPost, ActionName("Excluir_Pacote")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarExclusao(int id)
        {
            var pacote = _context.Pacotes.FirstOrDefault(p => p.Id == id);

            if (pacote != null)
            {
                // Verifica se o pacote está em alguma entrega antes de excluir
                var temEntregas = _context.Entregas.Any(e => e.PacoteId == id);
                if (temEntregas)
                {
                    // se o pacote estiver em uso, não permite a exclusão e avisa o usuário
                    TempData["ErroExclusao"] = "Não é possível excluir o pacote. Ele está vinculado a uma ou mais entregas.";
                    return RedirectToAction("Gerenciamento_pacotes");
                }

                _context.Pacotes.Remove(pacote);
                _context.SaveChanges();
            }

            return RedirectToAction("Gerenciamento_pacotes");
        }

        // POST: /Pacotes/NovoPacoteAjax
        [HttpPost]
        public IActionResult NovoPacoteAjax([FromBody] Pacote pacote)
        {
            if (pacote == null || string.IsNullOrEmpty(pacote.Tamanho))
                return BadRequest("Pacote inválido");

            pacote.DataCadastro = DateTime.Now;
            _context.Pacotes.Add(pacote);
            _context.SaveChanges();

            // Retorna JSON com ID e Descrição para atualizar o select
            return Json(new { id = pacote.Id, descricao = pacote.Tamanho });
        }


    }
}