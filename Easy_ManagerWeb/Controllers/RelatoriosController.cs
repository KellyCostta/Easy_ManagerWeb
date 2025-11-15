using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easy_ManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Easy_ManagerWeb.Controllers
{
    public class RelatoriosController : Controller
    {
        private readonly AppDbContext _context;

        public RelatoriosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Relatórios de Lucros
        public IActionResult Lucros()
        {
            return View();
        }

        // POST: Calcular Lucros
        [HttpPost]
        public async Task<IActionResult> Lucros(string periodo, DateTime? dataInicio, DateTime? dataFim)
        {
            DateTime inicio, fim;

            if (!string.IsNullOrEmpty(periodo))
                (inicio, fim) = CalcularPeriodo(periodo);
            else if (dataInicio.HasValue && dataFim.HasValue)
            {
                inicio = dataInicio.Value;
                fim = dataFim.Value;
            }
            else
            {
                inicio = DateTime.Now.Date;
                fim = DateTime.Now;
            }

            // 🔹 Buscar entregas
            var entregas = await _context.Entregas
                .Include(e => e.Cliente)
                .Where(e => e.DataAgendada >= inicio && e.DataAgendada <= fim)
                .Select(e => new
                {
                    Cliente = e.Cliente.Nome,
                    DataAgendada = e.DataAgendada,
                    ValorEntrega = e.Orcamento ?? 0
                })
                .ToListAsync();

            // 🔹 Buscar gastos
            var gastos = await _context.Gasto
                .Where(g => g.DataGasto >= inicio && g.DataGasto <= fim)
                .Select(g => new
                {
                    Tipo = g.Tipo,
                    Valor = g.Valor,
                    Data = g.DataGasto
                })
                .ToListAsync();

            // 🔹 Cálculos
            double receita = entregas.Sum(e => (double)e.ValorEntrega);
            double despesas = gastos.Sum(g => (double)g.Valor);
            double lucro = receita - despesas;

            // 🔹 Passar dados para a View
            ViewBag.Periodo = string.IsNullOrEmpty(periodo) ? "Personalizado" : periodo;
            ViewBag.DataInicio = inicio.ToShortDateString();
            ViewBag.DataFim = fim.ToShortDateString();
            ViewBag.Receita = receita;
            ViewBag.Despesas = despesas;
            ViewBag.Lucro = lucro;
            ViewBag.Vendas = entregas;
            ViewBag.Gastos = gastos;

            return View();
        }

        // GET: Gerar PDF (iTextSharp 5)
        [HttpGet]
        public async Task<IActionResult> GerarPdf(string periodo)
        {
            var (dataInicio, dataFim) = CalcularPeriodo(periodo);

            // Entregas
            var entregas = await _context.Entregas
                .Include(e => e.Cliente)
                .Where(e => e.DataAgendada >= dataInicio && e.DataAgendada <= dataFim)
                .Select(e => new
                {
                    Cliente = e.Cliente.Nome,
                    DataAgendada = e.DataAgendada,
                    ValorEntrega = e.Orcamento ?? 0
                })
                .ToListAsync();

            // Gastos
            var gastos = await _context.Gasto
                .Where(g => g.DataGasto >= dataInicio && g.DataGasto <= dataFim)
                .Select(g => new
                {
                    Tipo = g.Tipo,
                    Data = g.DataGasto,
                    Valor = g.Valor
                })
                .ToListAsync();

            double receita = entregas.Sum(e => (double)e.ValorEntrega);
            double despesas = gastos.Sum(g => (double)g.Valor);
            double lucro = receita - despesas;

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 40, 40, 50, 40);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // ===== FONTES =====
                var fonteTitulo = new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD);
                var fonteSubtitulo = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD);
                var fonteCabecalho = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
                var fonteTexto = new Font(Font.FontFamily.HELVETICA, 12);
                var fonteRodape = new Font(Font.FontFamily.HELVETICA, 10, Font.ITALIC, BaseColor.GRAY);

                // ===== LOGO =====
                string caminhoLogo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Imagens", "LOGO.jpg");
                if (System.IO.File.Exists(caminhoLogo))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(caminhoLogo);
                    logo.ScaleAbsolute(100f, 60f);
                    logo.Alignment = Element.ALIGN_RIGHT;
                    logo.SpacingAfter = 10f;
                    doc.Add(logo);
                }

                // ===== TÍTULO =====
                Paragraph titulo = new Paragraph("RELATÓRIO DE LUCROS", fonteTitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                doc.Add(titulo);

                // ===== PERÍODO =====
                doc.Add(new Paragraph($"Período: {dataInicio:dd/MM/yyyy} - {dataFim:dd/MM/yyyy}\n\n", fonteTexto));

                // ===== TABELA DE ENTREGAS =====
                doc.Add(new Paragraph("Nome do Cliente    Data da Entrega    Valor da Entrega", fonteCabecalho));
                PdfPTable tabelaEntregas = new PdfPTable(3)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 5,
                    SpacingAfter = 5
                };
                tabelaEntregas.SetWidths(new float[] { 45f, 25f, 30f });

                foreach (var e in entregas)
                {
                    tabelaEntregas.AddCell(new Phrase(e.Cliente, fonteTexto));
                    tabelaEntregas.AddCell(new Phrase(e.DataAgendada.ToString("dd/MM/yyyy"), fonteTexto));
                    tabelaEntregas.AddCell(new Phrase($"R$ {e.ValorEntrega:F2}", fonteTexto));
                }
                doc.Add(tabelaEntregas);
                doc.Add(new Paragraph($"Total: R$ {receita:F2}\n\n", fonteCabecalho));

                // ===== TABELA DE GASTOS =====
                doc.Add(new Paragraph("Tipo de Gasto    Data de Cadastro    Valor", fonteCabecalho));
                PdfPTable tabelaGastos = new PdfPTable(3)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 5,
                    SpacingAfter = 5
                };
                tabelaGastos.SetWidths(new float[] { 45f, 25f, 30f });

                foreach (var g in gastos)
                {
                    tabelaGastos.AddCell(new Phrase(g.Tipo, fonteTexto));
                    tabelaGastos.AddCell(new Phrase(g.Data.ToString("dd/MM/yyyy") ?? "", fonteTexto));
                    tabelaGastos.AddCell(new Phrase($"R$ {g.Valor:F2}", fonteTexto));
                }
                doc.Add(tabelaGastos);
                doc.Add(new Paragraph($"Total: R$ {despesas:F2}\n\n", fonteCabecalho));

                // ===== RESUMO FINAL =====
                doc.Add(new Paragraph("Lucro:", fonteSubtitulo));
                doc.Add(new Paragraph($"Receita   + R$ {receita:F2}", fonteTexto));
                doc.Add(new Paragraph($"Despesas  - R$ {despesas:F2}", fonteTexto));
                doc.Add(new Paragraph("_____________________________________________\n", fonteTexto));
                doc.Add(new Paragraph($"Total de Lucro no período = R$ {lucro:F2}\n\n", fonteSubtitulo));

                // ===== RODAPÉ =====
                doc.Add(new Paragraph("DBS DELIVERY", fonteCabecalho));
                doc.Add(new Paragraph("Instagram: @dbs_delivery", fonteRodape));
                doc.Add(new Paragraph("Telefone: (xx) xxxx-xxxx | CNPJ: 00.000.000/0000-00", fonteRodape));

                doc.Close();

                byte[] pdfBytes = ms.ToArray();
                return File(pdfBytes, "application/pdf", $"Lucro_{periodo}_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }

        // 🔹 Função auxiliar para calcular período
        private (DateTime inicio, DateTime fim) CalcularPeriodo(string periodo)
        {
            DateTime hoje = DateTime.Now;
            DateTime inicio, fim;

            switch (periodo)
            {
                case "diario":
                    inicio = hoje.Date;
                    fim = hoje;
                    break;

                case "semanal":
                    int diff = (7 + (hoje.DayOfWeek - DayOfWeek.Monday)) % 7;
                    inicio = hoje.AddDays(-1 * diff).Date;
                    fim = inicio.AddDays(6);
                    break;

                case "mensal":
                    inicio = new DateTime(hoje.Year, hoje.Month, 1);
                    fim = inicio.AddMonths(1).AddDays(-1);
                    break;

                case "anual":
                    inicio = new DateTime(hoje.Year, 1, 1);
                    fim = new DateTime(hoje.Year, 12, 31);
                    break;

                default:
                    inicio = hoje.Date;
                    fim = hoje;
                    break;
            }

            return (inicio, fim);
        }

        // GET: Dashboard resumo
        public IActionResult Dashboard()
        {
            // 1. Dados agregados
            var totalEntregas = _context.Entregas.Count();
            var totalFaturado = _context.Entregas.Sum(e => e.Orcamento) ?? 0;
            var orcamentoMedio = totalEntregas > 0 ? totalFaturado / totalEntregas : 0;

            ViewBag.TotalEntregas = totalEntregas;
            ViewBag.TotalFaturado = totalFaturado;
            ViewBag.OrcamentoMedio = orcamentoMedio;

            // 2. Entregas por Status
            var entregasPorStatus = _context.Entregas
                .GroupBy(e => e.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.StatusLabels = entregasPorStatus.Select(x => x.Status).ToList();
            ViewBag.StatusData = entregasPorStatus.Select(x => x.Count).ToList();

            // 3. Top 5 Clientes
            var topClientes = _context.Entregas
                .Include(e => e.Cliente)
                .Where(e => e.ClienteId > 0)
                .GroupBy(e => e.ClienteId)
                .Select(g => new
                {
                    ClienteNome = g.First().Cliente.Nome,
                    TotalEntregas = g.Count()
                })
                .OrderByDescending(x => x.TotalEntregas)
                .Take(5)
                .ToList();

            ViewBag.TopClientes = topClientes;

            // 4. Últimas Entregas
            var ultimasEntregas = _context.Entregas
                .Include(e => e.Cliente)
                .Include(e => e.Pacotes)
                .OrderByDescending(e => e.Id)
                .Take(10)
                .ToList();

            return View(ultimasEntregas);
        }
    }
}
