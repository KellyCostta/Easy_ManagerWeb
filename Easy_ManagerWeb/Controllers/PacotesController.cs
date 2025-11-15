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


        [HttpPost, ActionName("Excluir_Pacote")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarExclusao(int id)
        {
            var pacote = _context.Pacotes.FirstOrDefault(p => p.Id == id);

            if (pacote == null)
                return RedirectToAction("Gerenciamento_pacotes");

            // 1) Verifica se existe alguma Entrega que contenha esse pacote via relação Pacotes
            bool temEntregas = _context.Entregas
                .Any(e => e.Pacotes.Any(p => p.Id == id));

            // 2) Verificação extra (legado): se sua entidade Pacote ainda tem um campo EntregaId
            //    (por exemplo int EntregaId; ou int? EntregaId), também considera isso como vínculo.
            try
            {
                // Evita exceção caso propriedade não exista; se existir e for > 0 consideramos vínculo.
                var entregaIdProp = pacote.GetType().GetProperty("EntregaId");
                if (!temEntregas && entregaIdProp != null)
                {
                    var valor = entregaIdProp.GetValue(pacote);
                    if (valor != null)
                    {
                        if (int.TryParse(valor.ToString(), out int entregaId) && entregaId > 0)
                            temEntregas = true;
                    }
                }
            }
            catch
            {
                // se algo falhar aqui, não impede o fluxo principal — já fizemos a verificação principal acima
            }

            if (temEntregas)
            {
                // se o pacote estiver em uso, não permite a exclusão e avisa o usuário
                TempData["ErroExclusao"] = "Não é possível excluir o pacote. Ele está vinculado a uma ou mais entregas.";
                return RedirectToAction("Gerenciamento_pacotes");
            }

            // Se não está vinculado, pode excluir
            _context.Pacotes.Remove(pacote);
            _context.SaveChanges();

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

            // Retorna JSON com ID, Tamanho e Peso
            return Json(new
            {
                id = pacote.Id,
                tamanho = pacote.Tamanho,
                peso = pacote.Peso
            });
        }


    }
}