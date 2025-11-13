using iTextSharp.text;
using iTextSharp.text.pdf;
using System;

namespace Easy_ManagerWeb.Utils
{
    public class PdfHeaderFooter : PdfPageEventHelper
    {
        private readonly string _empresa;

        public PdfHeaderFooter(string empresa)
        {
            _empresa = empresa;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            var cb = writer.DirectContent;
            var pageSize = document.PageSize;

            // Fonte
            var font = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY);

            // Cabeçalho
            var header = new Phrase($"{_empresa} - Relatório de Entregas", font);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, header,
                document.LeftMargin, pageSize.GetTop(document.TopMargin) + 10, 0);

            var data = new Phrase($"Emitido em: {DateTime.Now:dd/MM/yyyy HH:mm}", font);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, data,
                pageSize.Width - document.RightMargin, pageSize.GetTop(document.TopMargin) + 10, 0);

            // Rodapé com número da página
            var footer = new Phrase($"Página {writer.PageNumber}", font);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, footer,
                (pageSize.Left + pageSize.Right) / 2, pageSize.GetBottom(document.BottomMargin) - 10, 0);
        }
    }
}
