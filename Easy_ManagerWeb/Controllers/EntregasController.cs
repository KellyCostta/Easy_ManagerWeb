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

        // GET: Nova entrega (com pacote opcional)
        public IActionResult Nova_entrega(int? pacoteId = null)
        {
            ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome");
            ViewBag.Pacotes = new SelectList(_context.Pacotes.ToList(), "Id", "Descricao", pacoteId);
            return View();
        }

        // POST: Nova entrega
        [HttpPost]
        public IActionResult Nova_entrega(Entrega entrega)
        {
            if (ModelState.IsValid)
            {
                var pacote = _context.Pacotes.FirstOrDefault(p => p.Id == entrega.PacoteId);

                // Cálculo de orçamento atualizado (peso em gramas)
                double valorBase = 5.0; // valor fixo base
                double valorPorKm = 1.8; // custo por km
                double valorPorGrama = 0.002; // custo por grama

                double distancia = entrega.Distancia ?? 0;
                double pesoGramas = 0;

                // tenta converter o campo de texto em número (em gramas)
                if (pacote != null && double.TryParse(pacote.Peso, out double pesoInformado))
                {
                    pesoGramas = pesoInformado;
                }

                double orcamento = valorBase + (distancia * valorPorKm) + (pesoGramas * valorPorGrama);
                entrega.Orcamento = Math.Round(orcamento, 2);

                _context.Entregas.Add(entrega);
                _context.SaveChanges();

                // redireciona para o resumo do orçamento
                return RedirectToAction("Resumo_Orcamento", new { id = entrega.Id });
            }

            // Recarrega dropdowns em caso de erro
            ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome", entrega.ClienteId);
            ViewBag.Pacotes = new SelectList(_context.Pacotes.ToList(), "Id", "Descricao", entrega.PacoteId);

            return View(entrega);
        }

        // GET: Exibe o orçamento gerado
        public IActionResult Resumo_Orcamento(int id)
        {
            var entrega = _context.Entregas
                                  .Include(e => e.Cliente)
                                  .Include(e => e.Pacote)
                                  .FirstOrDefault(e => e.Id == id);

            if (entrega == null)
                return NotFound();

            return View(entrega); // Views/Entregas/Resumo_Orcamento.cshtml
        }

        // POST: Atualiza o status sem sair da tabela
        [HttpPost]
        public IActionResult AtualizarStatus(int id, string status)
        {
            var entrega = _context.Entregas.FirstOrDefault(e => e.Id == id);
            if (entrega == null)
                return NotFound();

            entrega.Status = status;
            _context.SaveChanges();

            return Ok();
        }
    }
}
