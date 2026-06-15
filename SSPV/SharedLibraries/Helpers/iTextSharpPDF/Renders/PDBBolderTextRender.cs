using iTextSharp.text.pdf;

namespace iTextSharpPDF.Renders
{
    public class PDBBolderTextRender : PDFBBCodeTextRenderBase
    {
        public PDBBolderTextRender(PDFDocument pdfDocument, PDFTextRenderBase nextCodeParser = null)
                : base(pdfDocument, nextCodeParser, @"\[bb\].*\[/bb\]") { }

        public override string GetTextFromChunk(string chunk)
        {
            return chunk.Replace("[bb]", "").Replace("[/bb]", "");
        }

        public override void SetRenderConditions()
        {
            pdfDocument.Canvas.SetLineWidth(0.5);
            pdfDocument.Canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
        }
    }
}