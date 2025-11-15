using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Easy_ManagerWeb.Controllers
{
    public class OrcamentosController : Controller
    {
        private readonly AppDbContext _context;

        public OrcamentosController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Página para criar novo orçamento
        public IActionResult Orcamento_Avulso()
        {
            return View();
        }

        // POST: Salvar orçamento avulso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SalvarOrcamentoAvulso(Orcamento model)
        {
            if (!ModelState.IsValid)
                return View("Orcamento_Avulso", model);

            // Cálculo de orçamento seguindo padrão da EntregasController
            double valorBase = 5.0;
            double valorPorKm = 1.8;
            double valorPorGrama = 0.002;

            double distancia = model.Distancia;
            double pesoGramas = 0;

            if (double.TryParse(model.Peso?.ToString(), out double pesoInformado))
                pesoGramas = pesoInformado;

            double orcamento = valorBase + (distancia * valorPorKm) + (pesoGramas * valorPorGrama);
            model.ValorOrcamento = Math.Round((double)orcamento, 2);

            model.Status = "Em Potencial";
            model.DataCadastro = DateTime.Now;

            _context.Orcamentos.Add(model);
            _context.SaveChanges();

            TempData["Mensagem"] = "Orçamento salvo com sucesso!";
            return RedirectToAction("Orcamento_Avulso");
        }

        // GET: Lista de orçamentos
        public IActionResult ListarOrcamentos()
        {
            var orcamentos = _context.Orcamentos
                .OrderByDescending(o => o.DataCadastro)
                .ToList();

            return View(orcamentos);
        }

        // GET: Deletar orçamento
        public IActionResult Deletar(int id)
        {
            var orcamento = _context.Orcamentos.FirstOrDefault(o => o.Id == id);
            if (orcamento == null)
                return NotFound();

            _context.Orcamentos.Remove(orcamento);
            _context.SaveChanges();

            TempData["Mensagem"] = "Orçamento deletado com sucesso!";
            return RedirectToAction("ListarOrcamentos");
        }

        public IActionResult Detalhes(int id)
        {
            var orcamento = _context.Orcamentos.FirstOrDefault(o => o.Id == id);

            if (orcamento == null)
                return NotFound();

            return View(orcamento);
        }



    }
}
