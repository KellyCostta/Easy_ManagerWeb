using Easy_ManagerWeb.Models;
using Easy_ManagerWeb.Utils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Easy_ManagerWeb.Controllers
{
    // Modelo auxiliar para atualizar o status
    public class StatusUpdateModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class EntregasController : Controller
    {
        private readonly AppDbContext _context;

        public EntregasController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // 🔹 CHAMADO QUANDO ACEITA UM ORÇAMENTO
        public IActionResult CreateFromOrcamento(int id)
        {
            var orcamento = _context.Orcamentos
                .FirstOrDefault(o => o.Id == id);

            if (orcamento == null)
            {
                TempData["Erro"] = $"Orçamento com ID {id} não encontrado.";
                return RedirectToAction("ListarOrcamentos", "Orcamentos");
            }

            // 🔁 Redireciona para Nova_entrega com o ID do orçamento
            return RedirectToAction("Nova_entrega", new { orcamentoId = id });
        }

        // 🔹 LISTAGEM DE ENTREGAS
        public IActionResult Gerenciamento_entregas()
        {
            var entregas = _context.Entregas
                .Include(e => e.Cliente)
                .Include(e => e.Pacote)
                .ToList();

            return View(entregas);
        }

        // 🔹 GET: Nova entrega (pode vir de um orçamento)
        public IActionResult Nova_entrega()
        {

            ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome");
            ViewBag.Pacotes = new SelectList(_context.Pacotes.ToList(), "Id", "Tamanho");



            return View();
        }

        // 🔹 POST: Nova entrega
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Nova_entrega(Entrega entrega)
        {
            if (ModelState.IsValid)
            {
                var pacote = _context.Pacotes.FirstOrDefault(p => p.Id == entrega.PacoteId);

                double valorBasePeso = 5.0;
                double valorBaseKm = 5.0;

                double valorPeso = 0;
                double valorTempo = 0;
                double valorTamanho = 0;
                double valorDistancia = 0;
                double pesoInformado = 0;
                double tempoInformado = double.Parse(entrega.Tempo) ;

                double distancia = entrega.Distancia ?? 0;

                if (pacote != null && double.TryParse(pacote.Peso, out pesoInformado))
                {
                    if (pesoInformado <= 1)
                        valorPeso = valorBasePeso;
                    else
                        valorPeso = valorBasePeso + (pesoInformado - 1) * 1;
                }

                switch (pacote?.Tamanho)
                {
                    case "Pequeno":
                        valorTamanho = 1;
                        break;
                    case "Médio":
                        valorTamanho = 3;
                        break;
                    case "Grande":
                        valorTamanho = 5;
                        break;
                }

                if (distancia <= 5)
                    valorDistancia = valorBaseKm;
                else
                    valorDistancia = valorBaseKm + (distancia - 5) * 1;


                if (tempoInformado <= 1)
                    valorTempo = 1.0;
                else
                    valorTempo = 1 + (tempoInformado - 10) *0.20 ;


                double orcamento = valorPeso + valorTamanho + valorDistancia + valorTempo;

                entrega.Orcamento = Math.Round(orcamento, 2);
                entrega.Status = "Pendente";

                _context.Entregas.Add(entrega);
                _context.SaveChanges();

                return RedirectToAction("Resumo_Orcamento", new { id = entrega.Id });
            }

            ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome", entrega.ClienteId);
            ViewBag.Pacotes = new SelectList(_context.Pacotes.ToList(), "Id", "Tamanho", entrega.PacoteId);
            return View(entrega);
        }


        // 🔹 RESUMO DO ORÇAMENTO
        public IActionResult Resumo_Orcamento(int id)
        {
            var entrega = _context.Entregas
                .Include(e => e.Cliente)
                .Include(e => e.Pacote)
                .FirstOrDefault(e => e.Id == id);

            if (entrega == null)
                return NotFound();

            return View(entrega);
        }

        // 🔹 EDITAR ENTREGA
        public IActionResult Editar_Entrega(int id)
        {
            var entrega = _context.Entregas.FirstOrDefault(e => e.Id == id);
            if (entrega == null) return NotFound();

            ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome", entrega.ClienteId);
            ViewBag.Pacotes = new SelectList(_context.Pacotes.ToList(), "Id", "Descricao", entrega.PacoteId);
            return View(entrega);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar_Entrega(Entrega entrega)
        {
            if (ModelState.IsValid)
            {
                var pacote = _context.Pacotes.FirstOrDefault(p => p.Id == entrega.PacoteId);

                double valorBase = 5.0;
                double valorPorKm = 1.8;
                double valorPorGrama = 0.002;

                double distancia = entrega.Distancia ?? 0;
                double pesoGramas = 0;

                if (pacote != null && double.TryParse(pacote.Peso, out double pesoInformado))
                    pesoGramas = pesoInformado;

                entrega.Orcamento = Math.Round(valorBase + (distancia * valorPorKm) + (pesoGramas * valorPorGrama), 2);

                _context.Entregas.Update(entrega);
                _context.SaveChanges();

                return RedirectToAction("Gerenciamento_entregas");
            }

            ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome", entrega.ClienteId);
            ViewBag.Pacotes = new SelectList(_context.Pacotes.ToList(), "Id", "Descricao", entrega.PacoteId);
            return View(entrega);
        }

        // 🔹 ATUALIZAR STATUS (via AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarStatus([FromBody] StatusUpdateModel dados)
        {
            if (dados == null) return BadRequest();

            var entrega = _context.Entregas.FirstOrDefault(e => e.Id == dados.Id);
            if (entrega == null) return NotFound();

            entrega.Status = dados.Status;
            _context.SaveChanges();

            return Ok();
        }

        // 🔹 EXCLUIR ENTREGA
        public IActionResult Excluir_Entrega(int id)
        {
            var entrega = _context.Entregas
                .Include(e => e.Cliente)
                .Include(e => e.Pacote)
                .FirstOrDefault(e => e.Id == id);

            if (entrega == null)
                return NotFound();

            return View(entrega);
        }

        [HttpPost, ActionName("Excluir_Entrega")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarExclusao(int id)
        {
            var entrega = _context.Entregas.FirstOrDefault(e => e.Id == id);
            if (entrega != null)
            {
                _context.Entregas.Remove(entrega);
                _context.SaveChanges();
            }

            return RedirectToAction("Gerenciamento_entregas");
        }

        public async Task<IActionResult> Relatorio(string periodo)
        {
            var entregas = _context.Entregas
                .Include(e => e.Cliente)
                .AsQueryable();

            DateTime hoje = DateTime.Today;

            switch (periodo?.ToLower())
            {
                case "futuras":
                    entregas = entregas.Where(e => e.DataAgendada > hoje);
                    ViewBag.Titulo = "Entregas Futuras";
                    break;

                case "diario":
                    entregas = entregas.Where(e => e.DataAgendada.Date == hoje);
                    ViewBag.Titulo = "Entregas do Dia";
                    break;

                case "semanal":
                    var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek + (int)DayOfWeek.Monday);
                    var fimSemana = inicioSemana.AddDays(7);
                    entregas = entregas.Where(e => e.DataAgendada >= inicioSemana && e.DataAgendada < fimSemana);
                    ViewBag.Titulo = "Entregas Semanais";
                    break;

                case "mensal":
                    entregas = entregas.Where(e => e.DataAgendada.Month == hoje.Month && e.DataAgendada.Year == hoje.Year);
                    ViewBag.Titulo = "Entregas Mensais";
                    break;

                case "anual":
                    entregas = entregas.Where(e => e.DataAgendada.Year == hoje.Year);
                    ViewBag.Titulo = "Entregas Anuais";
                    break;

                default:
                    ViewBag.Titulo = "Todas as Entregas";
                    break;
            }

            return View(await entregas.OrderBy(e => e.DataAgendada).ToListAsync());
        }

        public IActionResult GerarPdf(string periodo)
        {
            var entregas = _context.Entregas
                .Include(e => e.Cliente)
                .Include(e => e.Pacote)
                .AsQueryable();

            DateTime hoje = DateTime.Today;

            switch (periodo?.ToLower())
            {
                case "futuras":
                    entregas = entregas.Where(e => e.DataAgendada > hoje);
                    break;
                case "diario":
                    entregas = entregas.Where(e => e.DataAgendada.Date == hoje);
                    break;
                case "semanal":
                    var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek);
                    var fimSemana = inicioSemana.AddDays(7);
                    entregas = entregas.Where(e => e.DataAgendada >= inicioSemana && e.DataAgendada < fimSemana);
                    break;
                case "mensal":
                    entregas = entregas.Where(e => e.DataAgendada.Month == hoje.Month && e.DataAgendada.Year == hoje.Year);
                    break;
                case "anual":
                    entregas = entregas.Where(e => e.DataAgendada.Year == hoje.Year);
                    break;
            }

            var lista = entregas.OrderBy(e => e.DataAgendada).ToList();

            using (var stream = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 40, 40, 60, 40);
                var writer = PdfWriter.GetInstance(doc, stream);

                // Cabeçalho/Rodapé
                writer.PageEvent = new PdfHeaderFooter("Easy Manager");

                doc.Open();

                var titulo = new Paragraph($"Relatório de Entregas - {periodo?.ToUpper() ?? "GERAL"}",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK));
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 15f;
                doc.Add(titulo);

                // Cria tabela com 5 colunas
                var tabela = new PdfPTable(5) { WidthPercentage = 100 };
                tabela.SetWidths(new float[] { 2.5f, 3f, 2f, 2f, 2f });

                // Cabeçalho colorido
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.WHITE);
                BaseColor headerColor = new BaseColor(52, 73, 94);

                void AddHeader(string text)
                {
                    var cell = new PdfPCell(new Phrase(text, headerFont))
                    {
                        BackgroundColor = headerColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 6
                    };
                    tabela.AddCell(cell);
                }

                AddHeader("Cliente");
                AddHeader("Endereço");
                AddHeader("Data Agendada");
                AddHeader("Pacote");
                AddHeader("Status");

                // Linhas
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                foreach (var e in lista)
                {
                    tabela.AddCell(new Phrase(e.Cliente?.Nome ?? "-", cellFont));
                    tabela.AddCell(new Phrase(e.Cliente?.Endereco ?? "-", cellFont));
                    tabela.AddCell(new Phrase(e.DataAgendada.ToString("dd/MM/yyyy HH:mm"), cellFont));
               
                    tabela.AddCell(new Phrase(e.Status ?? "-", cellFont));
                }

                doc.Add(tabela);
                doc.Close();

                return File(stream.ToArray(), "application/pdf", $"Relatorio_{periodo}_{DateTime.Now:yyyyMMddHHmm}.pdf");
            }
        }
    }
}
    
