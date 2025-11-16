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


        // 🔹 LISTAGEM DE ENTREGAS
        public IActionResult Gerenciamento_entregas()
        {

            var entregas = _context.Entregas.ToList();

            int totalEntregas = entregas.Count;
            int totalCanceladas = entregas.Count(e => e.Status == "Cancelada");
            int totalFinal = totalEntregas - totalCanceladas;

            ViewBag.TotalFinal = totalFinal;

            return View(entregas);
        

        }


        // GET: NovaEntrega
        public IActionResult Nova_entrega()
        {
            // Clientes: mostrar Nome + Telefone
            ViewBag.Clientes = new SelectList(
                _context.Clientes
                    .Select(c => new {
                        c.Id,
                        Texto = c.Nome + " - " + c.Telefone
                    })
                    .ToList(),
                "Id",
                "Texto"
            );

            // Pacotes: mostrar Tamanho + Peso (não descrição)
            ViewBag.Pacotes = new SelectList(
                _context.Pacotes
                    .Select(p => new {
                        p.Id,
                        Texto = p.Tamanho + " - " + p.Peso   // ex: "Pequeno - 2kg"
                    })
                    .ToList(),
                "Id",
                "Texto"
            );

            return View();
        }


        // 🔹 POST: Nova entrega
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Nova_entrega(Entrega entrega)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome", entrega.ClienteId);
                return View(entrega);
            }

            // BUSCAR TODOS OS PACOTES SELECIONADOS
            var pacotes = _context.Pacotes
                .Where(p => entrega.PacotesIds.Contains(p.Id))
                .ToList();

            if (pacotes.Count == 0)
            {
                ModelState.AddModelError("", "Selecione ao menos um pacote.");
                ViewBag.Clientes = new SelectList(_context.Clientes.ToList(), "Id", "Nome", entrega.ClienteId);
                return View(entrega);
            }

            //--------------------------------------
            // 1. SOMA DOS PACOTES (PESO + TAMANHO)
            //--------------------------------------
            double valorTotalPacotes = 0;
            double valorBasePeso = 5.0;

            foreach (var pacote in pacotes)
            {
                double valorPeso = 0;
                double valorTamanho = 0;
                double pesoInformado = 0;

                // PESO
                if (double.TryParse(pacote.Peso, out pesoInformado))
                {
                    if (pesoInformado <= 1)
                        valorPeso = valorBasePeso;
                    else
                        valorPeso = valorBasePeso + (pesoInformado - 1) * 1;
                }

                // TAMANHO
                switch (pacote.Tamanho)
                {
                    case "Pequeno": valorTamanho = 1; break;
                    case "Médio": valorTamanho = 3; break;
                    case "Grande": valorTamanho = 5; break;
                }

                valorTotalPacotes += (valorPeso + valorTamanho);
            }

            //--------------------------------------
            // 2. CALCULOS ÚNICOS DA ENTREGA
            //--------------------------------------

            double valorBaseKm = 5.0;
            double valorDistancia = 0;
            double valorTempo = 0;

            double distancia = entrega.Distancia ?? 0;
            double tempoInformado = double.Parse(entrega.Tempo);

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

            entrega.Orcamento = Math.Round(orcamento, 2);
            entrega.Status = "Pendente";

            // SALVA A ENTREGA
            _context.Entregas.Add(entrega);
            _context.SaveChanges();

            //--------------------------------------
            // 4. RELACIONA PACOTES À ENTREGA
            //--------------------------------------

            foreach (var pacote in pacotes)
            {
                pacote.EntregaId = entrega.Id;
                _context.Pacotes.Update(pacote);
            }


            _context.SaveChanges();

            return RedirectToAction("Resumo_Orcamento", new { id = entrega.Id });
        }



        // 🔹 RESUMO DO ORÇAMENTO
        public IActionResult Resumo_Orcamento(int id)
        {
            var entrega = _context.Entregas
                .Include(e => e.Cliente)
                .Include(e => e.Pacotes)
                .FirstOrDefault(e => e.Id == id);

            if (entrega == null)
                return NotFound();

            return View(entrega);
        }

        //GET Editar Entrega
        public IActionResult Editar_Entrega(int id)
        {
            var entrega = _context.Entregas
                .Include(e => e.Pacotes) // carrega pacotes já vinculados
                .FirstOrDefault(e => e.Id == id);

            if (entrega == null) return NotFound();

            // Lista de pacotes disponíveis (não vinculados ou vinculados a esta entrega)
            var pacotes = _context.Pacotes
                .Where(p => p.EntregaId == null || p.EntregaId == entrega.Id)
                .Select(p => new
                {
                    p.Id,
                    Nome = p.Tamanho + " - " + p.Peso + " Kg"
                })
                .ToList();

            // IDs dos pacotes atualmente associados
            var pacotesSelecionados = entrega.Pacotes.Select(p => p.Id).ToList();

            // MultiSelectList com seleção prévia
            ViewBag.Pacotes = new MultiSelectList(
                pacotes,
                "Id",
                "Nome",
                pacotesSelecionados
            );

            ViewBag.Clientes = new SelectList(
                _context.Clientes,
                "Id",
                "Nome",
                entrega.ClienteId
            );

            return View(entrega);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar_Entrega(Entrega entrega)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nome", entrega.ClienteId);
                ViewBag.Pacotes = new MultiSelectList(
                    _context.Pacotes
                        .Where(p => p.EntregaId == null || entrega.PacotesIds.Contains(p.Id))
                        .Select(p => new { p.Id, Nome = p.Tamanho + " - " + p.Peso + " Kg" }),
                    "Id",
                    "Nome",
                    entrega.PacotesIds
                );

                return View(entrega);
            }

            var entregaDb = _context.Entregas
                .Include(e => e.Pacotes) // Garantir que a lista de pacotes seja carregada
                .FirstOrDefault(e => e.Id == entrega.Id);

            if (entregaDb == null) return NotFound();

            // Atualiza dados básicos
            entregaDb.ClienteId = entrega.ClienteId;
            entregaDb.Distancia = entrega.Distancia;
            entregaDb.Tempo = entrega.Tempo;
            entregaDb.DataAgendada = entrega.DataAgendada;

            // ---- Atualiza pacotes vinculados ----
            // Remove pacotes que não estão mais selecionados
            var pacotesParaRemover = entregaDb.Pacotes
                .Where(p => entrega.PacotesIds == null || !entrega.PacotesIds.Contains(p.Id))
                .ToList();
            foreach (var p in pacotesParaRemover)
            {
                p.EntregaId = null;
            }

            // Adiciona pacotes selecionados
            var pacotesSelecionados = _context.Pacotes
                .Where(p => entrega.PacotesIds != null && entrega.PacotesIds.Contains(p.Id))
                .ToList();

            foreach (var p in pacotesSelecionados)
            {
                p.EntregaId = entregaDb.Id;
            }

            // Atualiza navigation property
            entregaDb.Pacotes = pacotesSelecionados;

            // ---- Recalcular orçamento ----
            double valorBasePeso = 5.0;
            double valorBaseKm = 5.0;

            double valorPesoTotal = 0;
            double valorTamanhoTotal = 0;
            double valorDistancia = entregaDb.Distancia <= 5 ? valorBaseKm : valorBaseKm + (entregaDb.Distancia.Value - 5);

            double tempoInformado = 0;
            double.TryParse(entregaDb.Tempo, out tempoInformado);
            double valorTempo = tempoInformado <= 10 ? 1 : 1 + (tempoInformado - 10) * 0.20;

            foreach (var p in pacotesSelecionados)
            {
                if (double.TryParse(p.Peso, out double pesoEmKg))
                {
                    valorPesoTotal += pesoEmKg <= 1 ? valorBasePeso : valorBasePeso + (pesoEmKg - 1);
                }

                switch (p.Tamanho)
                {
                    case "Pequeno": valorTamanhoTotal += 1; break;
                    case "Médio": valorTamanhoTotal += 3; break;
                    case "Grande": valorTamanhoTotal += 5; break;
                }
            }

            entregaDb.Orcamento = Math.Round(valorPesoTotal + valorTamanhoTotal + valorDistancia + valorTempo, 2);

            _context.SaveChanges();

            return RedirectToAction("Gerenciamento_entregas");
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
                .Include(e => e.Pacotes)
                .FirstOrDefault(e => e.Id == id);

            if (entrega == null)
                return NotFound();

            return View(entrega);
        }

        [HttpPost, ActionName("Excluir_Entrega")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarExclusao(int id)
        {
            // 1️⃣ Deletar pacotes vinculados à entrega
            var pacotes = _context.Pacotes.Where(p => p.EntregaId == id).ToList();
            if (pacotes.Any())
            {
                _context.Pacotes.RemoveRange(pacotes);
                _context.SaveChanges();
            }

            // 2️⃣ Deletar a entrega
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
                .AsQueryable();

            DateTime hoje = DateTime.Today;

            // FILTROS
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

                writer.PageEvent = new PdfHeaderFooter("Easy Manager");
                doc.Open();

                // Título
                var titulo = new Paragraph(
                    $"Relatório de Entregas - {periodo?.ToUpper() ?? "GERAL"}",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15f
                };
                doc.Add(titulo);

                // TABELA: agora com 4 colunas
                var tabela = new PdfPTable(4)
                {
                    WidthPercentage = 100
                };

                tabela.SetWidths(new float[] { 3f, 3f, 2f, 2f });

                // Cabeçalho
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.WHITE);
                BaseColor headerColor = new BaseColor(52, 73, 94);

                void AddHeader(string text)
                {
                    tabela.AddCell(new PdfPCell(new Phrase(text, headerFont))
                    {
                        BackgroundColor = headerColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 6
                    });
                }

                AddHeader("Cliente");
                AddHeader("Endereço");
                AddHeader("Data Agendada");
                AddHeader("Valor");

                // Linhas
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                foreach (var e in lista)
                {
                    tabela.AddCell(new Phrase(e.Cliente?.Nome ?? "-", cellFont));
                    tabela.AddCell(new Phrase(e.Cliente?.Endereco ?? "-", cellFont));
                    tabela.AddCell(new Phrase(e.DataAgendada.ToString("dd/MM/yyyy HH:mm"), cellFont));

                    string valor = e.Orcamento?.ToString("C2") ?? "R$ 0,00";
                    tabela.AddCell(new Phrase(valor, cellFont));
                }

                doc.Add(tabela);
                doc.Close();

                return File(
                    stream.ToArray(),
                    "application/pdf",
                    $"Relatorio_{periodo}_{DateTime.Now:yyyyMMddH}.pdf"
                );
            }
        }

        public IActionResult AlterarStatus(int id)
        {
            var entrega = _context.Entregas.FirstOrDefault(e => e.Id == id);

            if (entrega == null)
                return NotFound();

            entrega.Status = "Entregue";
            _context.SaveChanges();

            return RedirectToAction("Gerenciamento_entregas"); // ou o nome da sua lista
        }

        public IActionResult CancelarEntrega(int id)
        {
            var entrega = _context.Entregas.FirstOrDefault(e => e.Id == id);

            if (entrega == null)
                return NotFound();

            entrega.Status = "Cancelada"; // <<< Aqui atualiza o status
            _context.SaveChanges();

            TempData["Mensagem"] = "Entrega cancelada com sucesso!";
            return RedirectToAction("Gerenciamento_entregas");
        }



    }
}
    
