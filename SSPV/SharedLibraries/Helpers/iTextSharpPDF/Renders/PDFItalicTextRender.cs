namespace iTextSharpPDF.Renders
{
    public class PDFItalicTextRender: PDFBBCodeTextRenderBase
    {
        public PDFItalicTextRender(PDFDocument pdfDocument, PDFTextRenderBase nextCodeParser= null)
                : base(pdfDocument, nextCodeParser, @"\[i\].*\[\/i\]") { }

        public override string GetTextFromChunk(string chunk)
        {
            return chunk.Replace("[i]", "").Replace("[/i]", "");
        }

        public override void SetRenderConditions()
        {
            pdfDocument.Canvas.SetTextMatrix(1, 0, 25, 1, 0, 0);
        }
    }
}
