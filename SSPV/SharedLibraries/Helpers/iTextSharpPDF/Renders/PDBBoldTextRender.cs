using iTextSharp.text.pdf;

namespace iTextSharpPDF.Renders
{
    public class PDBBoldTextRender : PDFBBCodeTextRenderBase
    {
        public PDBBoldTextRender(PDFDocument pdfDocument, PDFTextRenderBase nextCodeParser = null)
                : base(pdfDocument, nextCodeParser, @"\[b\].*\[/b\]") {}

        public override string GetTextFromChunk(string chunk)
        {
            return chunk.Replace("[b]", "").Replace("[/b]", "");
        }

        public override void SetRenderConditions()
        {
            pdfDocument.Canvas.SetLineWidth(0.2);
            pdfDocument.Canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
        }
    }
}
