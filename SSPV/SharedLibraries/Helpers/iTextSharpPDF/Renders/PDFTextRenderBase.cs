
using iTextSharp.text;

namespace iTextSharpPDF.Renders
{
    public abstract class PDFTextRenderBase
    {
        #region Propiedades

        protected PDFTextRenderBase nextParse;
        protected PDFDocument pdfDocument;

        #endregion

        #region Metodos

        public PDFTextRenderBase(PDFTextRenderBase nextParse, PDFDocument pdfDocument)
        {
            this.nextParse = nextParse;
            this.pdfDocument = pdfDocument;
        }

        public virtual void WritePlainText(string text, float x, float y, float rotation, ITFont font, Align align = Align.Left)
        {
            pdfDocument.Canvas.ShowTextAligned((int)align, text, x, y, rotation);
        }

        public virtual float CalculateWidthText(string text, ITFont font)
        {
            Chunk ch = new Chunk(text, new Font(font.BaseFont, font.Size));
            return ch.GetWidthPoint();
        }

        #endregion

        #region Abstractos

        public abstract void WriteText(string text, float x, float y, float rotation, ITFont font, Align align = Align.Left);

        #endregion
    }
}
