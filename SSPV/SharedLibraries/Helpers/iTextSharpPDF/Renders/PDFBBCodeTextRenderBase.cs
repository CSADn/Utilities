using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static iTextSharpPDF.PDFDocument;

namespace iTextSharpPDF.Renders
{
    public abstract class PDFBBCodeTextRenderBase: PDFTextRenderBase
    {
        #region Propiedades
        protected string regEx;
        #endregion

        #region Metodos

        public PDFBBCodeTextRenderBase(PDFDocument pdfDocument, PDFTextRenderBase nextCodeParser, string regEx) 
                : base(nextCodeParser, pdfDocument)
            {
            this.regEx = regEx;
        }

        public override void WriteText(string text, float x, float y, float rotation, ITFont font, Align align = Align.Left)
        {
            Regex regEx = new Regex(this.regEx);

            List<string> chunks = new List<string>();

            //Genero listado de trozos a renderizar
            List<string> matches = regEx.Matches(text).Cast<Match>().Select(m => m.Value).ToList();
            string tmpText = text;
            foreach (string mt in matches)
            {
                int idx = tmpText.IndexOf(mt);
                if (idx > 0)
                    chunks.Add(tmpText.Substring(0, idx));
                chunks.Add(mt);
                tmpText = tmpText.Substring(idx + mt.Length);
            }

            if (tmpText.Length > 0)
                chunks.Add(tmpText);

            foreach (string chunk in chunks)
            {
                if (regEx.IsMatch(chunk))
                {
                    string texto = GetTextFromChunk(chunk);

                    pdfDocument.Canvas.SaveState();
                    SetRenderConditions();
                    WriteTextChunk(texto, x, y, rotation, font, align);

                    //Actualizacion de posicion por tamaño del texto
                    x += CalculateWidthText(texto, font);

                    pdfDocument.Canvas.RestoreState();
                }
                else
                {
                    WriteTextChunk(chunk, x, y, rotation, font, align);

                    //Actualizacion de posicion por tamaño del texto
                    x += CalculateWidthText(chunk, font);
                }
            }
        }

        private void WriteTextChunk(string textChung, float x, float y, float rotation, ITFont font, Align align)
        {
            if (nextParse != null)
                nextParse.WriteText(textChung, x, y, rotation, font, align);
            else
                WritePlainText(textChung, x, y, rotation, font, align);
        }

        #endregion

        #region Abstractos

        public abstract string GetTextFromChunk(string chunk);

        public abstract void SetRenderConditions();

        #endregion
    }
}
