namespace iTextSharpPDF.Renders
{
    public static class PDFTextRenderFactory
    {
        #region Privados

        private static PDFTextRenderBase CreateInternal(PDFDocument pdfDocument)
        {
            return  new PDBBoldTextRender(pdfDocument,
                    new PDBBolderTextRender(pdfDocument,
                    new PDBBoldestTextRender(pdfDocument,
                    new PDFItalicTextRender(pdfDocument)
                   )));
        }
        
        #endregion

        #region Publicos

        public static PDFTextRenderBase Create(PDFDocument pdfDocument)
        {
            return CreateInternal(pdfDocument);
        }

        #endregion
    }
}
