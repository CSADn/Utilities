using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web;
using System.IO;

namespace iTextSharpPDF
{
    public class PageEvents : PdfPageEventHelper
    {
        private PDFDocument _pdfDocument;

        public event StartingPageHandler OnStartingPage;
        public event EndingPageHandler OnEndingPage;
        public event CloseDocumentHandler OnClosingDocument;

        public PageEvents()
        {
            //
        }

        public PageEvents(PDFDocument pdfDocument)
        {
            if (pdfDocument == null)
                throw new ArgumentNullException();

            _pdfDocument = pdfDocument;
        }


        public override void OnStartPage(PdfWriter writer, Document document)
        {
            onStartingPage();
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            onEndingPage();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            onClosingDocument();
        }


        private void onStartingPage()
        {
            if (OnStartingPage != null)
                OnStartingPage(_pdfDocument);
        }

        private void onEndingPage()
        {
            if (OnEndingPage != null)
                OnEndingPage(_pdfDocument);
        }

        private void onClosingDocument()
        {
            if (OnClosingDocument != null)
                OnClosingDocument(_pdfDocument);
        }
    }
}
