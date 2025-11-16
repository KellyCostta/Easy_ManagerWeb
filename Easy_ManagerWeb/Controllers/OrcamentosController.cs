using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
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

            double valorBasePeso = 5.0;

  
                double valorPeso = 0;
                double valorTamanho = 0;
                double pesoInformado;
                double.TryParse(model.Peso, out pesoInformado);


            // PESO

            if (pesoInformado <= 1)
                        valorPeso = valorBasePeso;
                    else
                        valorPeso = valorBasePeso + (pesoInformado - 1) * 1;
                

                // TAMANHO
                switch (model.Tamanho)
                {
                    case "Pequeno": valorTamanho = 1; break;
                    case "Médio": valorTamanho = 3; break;
                    case "Grande": valorTamanho = 5; break;
                }

                double valorTotalPacotes = (valorPeso + valorTamanho);
            

            //--------------------------------------
            // 2. CALCULOS ÚNICOS DA ENTREGA
            //--------------------------------------

            double valorBaseKm = 5.0;
            double valorDistancia = 0;
            double valorTempo = 0;

            double distancia = model.Distancia;
            double tempoInformado = double.Parse(model.Tempo);

            // DISTÂNCIA
            if (distancia <= 5)
                valorDistancia = valorBaseKm;
            else
                valorDistancia = valorBaseKm + (distancia - 5) * 1;

            // TEMPO
            if (tempoInformado <= 10)
                valorTempo = 1;
            else
                valorTempo = 1 + (tempoInformado - 10) * 0.20;

            //--------------------------------------
            // 3. ORÇAMENTO FINAL
            //--------------------------------------

            double orcamento = valorTotalPacotes + valorDistancia + valorTempo;

            model.ValorOrcamento = Math.Round(orcamento, 2);

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
