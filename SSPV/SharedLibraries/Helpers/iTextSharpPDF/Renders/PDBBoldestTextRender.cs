using iTextSharp.text.pdf;

namespace iTextSharpPDF.Renders
{
    public class PDBBoldestTextRender : PDFBBCodeTextRenderBase
    {
        public PDBBoldestTextRender(PDFDocument pdfDocument, PDFTextRenderBase nextCodeParser = null)
                : base(pdfDocument, nextCodeParser, @"\[bbb\].*\[/bbb\]") { }

        public override string GetTextFromChunk(string chunk)
        {
            return chunk.Replace("[bbb]", "").Replace("[/bbb]", "");
        }

        public override void SetRenderConditions()
        {
            pdfDocument.Canvas.SetLineWidth(0.9);
            pdfDocument.Canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
        }
    }
}
