using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharpPDF.Utilities;
using iTextSharpPDF.Renders;

namespace iTextSharpPDF
{
    public delegate void StartingPageHandler(PDFDocument sender);
    public delegate void EndingPageHandler(PDFDocument sender);
    public delegate void CloseDocumentHandler(PDFDocument sender);

    public class PDFDocument
    {
        #region Propiedades

        public Document Document { get; private set; }

        public PdfWriter Writer { get; private set; }

        public PdfContentByte Canvas { get; private set; }

        public PdfContentByte CanvasUnder { get; private set; }

        private PDFTextRenderBase _textRender = null;
        private PDFTextRenderBase TextRender
        {
            get
            {
                if (_textRender == null)
                    _textRender = PDFTextRenderFactory.Create(this);
                return _textRender;
            }
        }

        public float Height
        {
            get
            {
                return (Document == null ? -1f : Document.PageSize.Height);
            }
        }

        public float Width
        {
            get
            {
                return (Document == null ? -1f : Document.PageSize.Width);
            }
        }

        public ITFont DefaultFont { get; set; }

        public bool HeaderVisible { get { return _headerVisible; } set { Document.NewPage(); _headerVisible = value; } }

        public bool FooterVisible { get { return _footerVisible; } set { Document.NewPage(); _footerVisible = value; } }

        #endregion

        #region Atributos

        private MemoryStream _memoryDocument;
        private Rectangle _pageSize;
        private bool _headerVisible;
        private bool _footerVisible;

        #endregion

        #region Eventos

        public event EndingPageHandler OnStartingPage;
        public event EndingPageHandler OnEndingPage;
        public event CloseDocumentHandler OnClosingDocument;

        #endregion

        #region Metodos Publicos

        public PDFDocument() : this(null, new MemoryStream()) { /**/ }

        public PDFDocument(float pageWidth, float pageHeight) : this(new Rectangle(0, 0, pageWidth, pageHeight), new MemoryStream()) { /**/ }

        public PDFDocument(Rectangle pageSize) : this(pageSize, new MemoryStream()) { /**/ }

        public PDFDocument(string filePath)
        {
            _headerVisible = true;
            _footerVisible = true;

            Document = new Document();

            Writer = PdfWriter.GetInstance(Document, new FileStream(filePath, FileMode.Create));

            Document.Open();

            Canvas = Writer.DirectContent;

            CanvasUnder = Writer.DirectContentUnder;

            DefaultFont = ITFont.Helvetica;

            var pageEvent = new PageEvents(this);
            pageEvent.OnStartingPage += (sender) => onStartingPage();
            pageEvent.OnEndingPage += (sender) => onEndingPage();

            Writer.PageEvent = pageEvent;
        }

        public PDFDocument(Rectangle pageSize, MemoryStream memoryStream)
        {
            _memoryDocument = memoryStream;
            _pageSize = pageSize;
            _headerVisible = true;
            _footerVisible = true;

            Document = new Document();

            Writer = PdfWriter.GetInstance(Document, _memoryDocument);

            if (_pageSize != null)
                Document.SetPageSize(_pageSize);

            Document.Open();

            Canvas = Writer.DirectContent;

            CanvasUnder = Writer.DirectContentUnder;

            DefaultFont = ITFont.Helvetica;

            var pageEvent = new PageEvents(this);
            pageEvent.OnStartingPage += (sender) => onStartingPage();
            pageEvent.OnEndingPage += (sender) => onEndingPage();
            pageEvent.OnClosingDocument += (sender) => onClosingDocument();

            Writer.PageEvent = pageEvent;
        }

        public PDFDocument SetMargins(Margins margins)
        {
            if (margins == null)
                return this;

            Document.SetMargins(
                margins.Left,
                margins.Right,
                margins.Top,
                margins.Bottom
            );

            Document.NewPage();

            return this;
        }

        public PDFDocument Text(string text, float x, float y, Align align = Align.Left, ITFont font = null, System.Drawing.Color? color = null, bool bold = false)
        {
            var setFont = font ?? DefaultFont;

            Canvas.SetFont(setFont);

            Canvas.BeginText();

            if (color != null)
            {
                Canvas.SetColorStroke(new BaseColor(color.Value));
                Canvas.SetColorFill(new BaseColor(color.Value));
            }
            
            if (bold)
            {
                Canvas.SetLineWidth(0.1);
                Canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
            }
            else
                Canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);

            float xPos = x;
            float yPos = Height - y - setFont.BaseFont.GetAscentPoint("{ÑÛ/", setFont.Size) - (setFont.Size * 0.011f);
            TextRender.WriteText(text, xPos, yPos, 0, setFont, align);

            Canvas.EndText();

            return this;
        }

        public PDFDocument Textblock(string text, float x, float y, Align align = Align.Left, ITFont font = null, float lineSpacing = 0, System.Drawing.Color? color = null, bool bold = false)
        {
            var setFont = font ?? DefaultFont;
            Canvas.SetFont(setFont);

            Canvas.BeginText();

            if (color != null)
            {
                Canvas.SetColorStroke(new BaseColor(color.Value));
                Canvas.SetColorFill(new BaseColor(color.Value));
            }

            if (bold)
            {
                Canvas.SetLineWidth(0.1);
                Canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
            }
            else
                Canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);

            var lines = text.Replace("\r\n", "\r").Split('\r');
            var tall = setFont.BaseFont.GetAscentPoint("{ÑÛ/", setFont.Size) - setFont.BaseFont.GetDescentPoint("qp", setFont.Size) + lineSpacing;

            for (var i = 0; i < lines.Length; i++)
                TextRender.WriteText(lines[i], x, Height - y - ((i + 1) * tall), 0, setFont, align);

            Canvas.EndText();

            return this;
        }

        public PDFDocument Paragraph(string text, Align align = Align.Left, ITFont font = null, float lineSpacing = 0, float spacingBefore = 0, float spacingAfter = 0, System.Drawing.Color? color = null, bool bold = false, float? x = null, float? y = null, float? width = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                return this;

            var setFont = font ?? DefaultFont;
            Canvas.SetFont(setFont);

            var tall = setFont.BaseFont.GetAscentPoint("Q", setFont.Size) - setFont.BaseFont.GetDescentPoint("q", setFont.Size) + lineSpacing;

            var p = new Paragraph();

            p.Alignment = (int)align;

            p.Font = new Font(setFont.BaseFont, setFont.Size);

            if (color != null)
                p.Font.Color = new BaseColor(color.Value);
                        
            if (bold)
            {
                p.Font.SetStyle("bold");
                Canvas.SetLineWidth(0.1);
                Canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
            }
            else
                Canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);

            var ct = default(ColumnText);

            if (x.HasValue && y.HasValue)
            {
                ct = new ColumnText(Canvas);

                ct.SetSimpleColumn(
                    x.Value,
                    Document.Bottom,
                    (width.HasValue ? x.Value + width.Value : Document.Right),
                    Document.PageSize.Height - y.Value
                );
            }

            p.SpacingBefore = spacingBefore;
            p.SpacingAfter = spacingAfter;
            p.SetLeading(tall, 0);

            p.Add(text);

            if (x.HasValue && y.HasValue)
            {
                ct.AddElement(p);
                ct.Go();
            }
            else
                Document.Add(p);

            return this;
        }

        public PDFDocument Image(string path, float x, float y)
        {
            var bytes = File.ReadAllBytes(path);
            Image(bytes, x, y);

            return this;
        }

        public PDFDocument Image(byte[] buffer, float x, float y, float width = 0f, float height = 0f, ImageScale? scale = null, VerticalAlign? verticalAlign = null, Border border = null, float percent = 0f)
        {
            if (buffer == null || buffer.Length == 0)
                return this;

            scale = scale ?? ImageScale.Auto;
            verticalAlign = verticalAlign ?? VerticalAlign.Top;
            Image img = GetImage(buffer, x, y, width, height, scale, verticalAlign, percent);

            if (border != null)
                Rectangle(x, y, width, height, border, null);

            if (Document.IsOpen())
                Document.Add(img);

            return this;
        }

        public PDFDocument TextColumns(List<string> paragraphs, Align align = Align.Left, ITFont font = null, int columns = 1, float gutter = 24f, float spacingAfter = 5f, System.Drawing.Color? color = null, bool bold = false)
        {
            if (paragraphs == null || paragraphs.Count == 0)
                return this;

            var setITFont = font ?? DefaultFont;
            var setFont = new Font(setITFont.BaseFont, setITFont.Size);

            if (color != null)
                setFont.Color = new BaseColor(color.Value);

            if (bold)
                setFont.SetStyle("bold");

            var pageWidth = Document.PageSize.Width - Document.LeftMargin - Document.RightMargin;
            var columnWidth = (pageWidth / columns);
            var lly = Document.BottomMargin;
            var ury = Document.PageSize.Height - Document.TopMargin;

            var columnsBlocks = new List<Rectangle>();

            for (int i = 0; i < columns; i++)
            {
                var llx = Document.LeftMargin + (columnWidth * i);
                var urx = llx + columnWidth;

                if (columns > 1)
                {
                    if (i == 0)
                        urx -= (gutter / 2);
                    else if (i == (columns - 1))
                        llx += (gutter / 2);
                    else
                    {
                        llx += (gutter / 2);
                        urx -= (gutter / 2);
                    }
                }

                var r = new Rectangle(llx, lly, urx, ury);
                r.BackgroundColor = new BaseColor(System.Drawing.Color.Red);

                columnsBlocks.Add(r);
            }

            var column = 0;
            var columnText = new ColumnText(Canvas);

            columnText.SetSimpleColumn(columnsBlocks[column]);

            foreach (var ptext in paragraphs)
            {
                var p = new Paragraph(ptext, setFont);
                p.Alignment = (int)align;
                p.SpacingAfter = spacingAfter;

                columnText.AddElement(p);

                while (ColumnText.HasMoreText(columnText.Go()))
                {
                    column++;

                    if (column >= columns)
                    {
                        column = 0;
                        Document.NewPage();
                    }

                    columnText.SetSimpleColumn(columnsBlocks[column]);
                }
            }

            return this;
        }

        public PDFDocument Table(List<TableColumn> columns, List<List<string>> rows, float headerHeight = 0f, float rowHeight = 0f, float x = -1f, float y = -1f, Align align = Align.Left, Border border = null, ITFont font = null, System.Drawing.Color? color = null, bool bold = false)
        {
            if (columns == null || columns.Count == 0 || rows == null || rows.Count == 0)
                return this;

            Font setFont = CreateFont(font, color, bold);
            Font setFontBold = CreateFont(font, color, true);

            var columnsNumber = columns.Count();
            var columnsWidth = columns
                .Select(s => s.Width)
                .ToArray();

            var table = new PdfPTable(columnsNumber);
            table.HorizontalAlignment = (int)align;
            table.SetTotalWidth(columnsWidth);
            table.LockedWidth = true;

            foreach (var c in columns)
            {
                Font hFont = c.BoldHeader ? setFontBold : setFont;

                if (c.FontHeaderSize > 0 )
                    hFont.Size = c.FontHeaderSize;

                var cell = new PdfPCell(new Phrase(c.Header, hFont));
                cell.HorizontalAlignment = (int)c.Align;
                cell.VerticalAlignment = (int)(c.VerticalAlign.HasValue ? c.VerticalAlign : VerticalAlign.Bottom);
                cell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                if (c.Border != null)
                {
                    cell.BorderColor = new BaseColor(c.Border.Color ?? System.Drawing.Color.Black);

                    if (c.Border.All > 0)
                    {
                        cell.Border = iTextSharp.text.Rectangle.BOX;
                        cell.BorderWidth = c.Border.All;
                    }
                    else
                    {
                        cell.BorderWidthTop = c.Border.Top;
                        cell.BorderWidthLeft = c.Border.Left;
                        cell.BorderWidthBottom = c.Border.Bottom;
                        cell.BorderWidthRight = c.Border.Right;
                    }
                }

                if (headerHeight > 0)
                    cell.MinimumHeight = headerHeight;

                RenderTableCellPadding(c.Padding, cell);
                table.AddCell(cell);
            }

            table.CompleteRow();

            foreach (var r in rows)
            {
                for (var i = 0; i < columns.Count; i++)
                {
                    PdfPCell cell = null;

                    if (columns[i].IsImage)
                    {
                        //Transformo el r[i] en un byte
                        Image img = GetImage(Convert.FromBase64String(r[i]), x, y, columns[i].ImageWidth, columns[i].ImageHeight, columns[i].ImageScale, columns[i].VerticalAlign);
                        cell = new PdfPCell(img, columns[i].FitImage);
                    }
                    else
                    {
                        Font cellFont = setFont;
                        if (columns[i].Bold)
                            cellFont = setFontBold;

                        cell = new PdfPCell(new Phrase(r[i], cellFont));
                    }

                    cell.HorizontalAlignment = (int)columns[i].Align;
                    cell.VerticalAlignment = (int)(columns[i].VerticalAlign.HasValue ? columns[i].VerticalAlign.Value : VerticalAlign.Top);
                    RenderTableCellBorder(border, cell);
                    RenderTableCellPadding(columns[i].Padding, cell);


                    if (rowHeight > 0)
                        cell.MinimumHeight = rowHeight;
                    table.AddCell(cell);
                }

                var lastRow = table.GetRow(table.getLastCompletedRowIndex());

                if (lastRow.MaxHeights == 0)
                    lastRow.GetCells().First().Phrase.Add("\u0000");

                table.CompleteRow();
            }

            if (x != -1f || y != -1f)
                table.WriteSelectedRows(0, table.Size, x, Height - y, Canvas);
            else
                Document.Add(table);

            return this;
        }

        public PDFDocument Rectangle(float x, float y, float width, float height, Border border = null, System.Drawing.Color? fill = null)
        {
            var rectangle = new Rectangle(x, Height - height - y, x + width, Height - y);

            if (border != null)
            {
                var allWidth = (border.All > 0 ? (float?)border.All : default(float?));

                rectangle.BorderColor = new BaseColor(border.Color.Value);
                rectangle.BorderWidthTop = allWidth ?? border.Top;
                rectangle.BorderWidthLeft = allWidth ?? border.Left;
                rectangle.BorderWidthBottom = allWidth ?? border.Bottom;
                rectangle.BorderWidthRight = allWidth ?? border.Right;
            }

            if (fill != null)
                rectangle.BackgroundColor = new BaseColor(fill.Value);

            Canvas.Rectangle(rectangle);

            return this;
        }

        public PDFDocument Merge(string filePath, int page = 1, PdfContentByte canvas = null, float x = 0f, float y = 0f)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return this;

            var pdfBytes = Utils.GetPDFContent(filePath);
            return Merge(pdfBytes, page, canvas, x, y);
        }

        public PDFDocument Merge(byte[] pdfBytes, int page = 1, PdfContentByte canvas = null, float x = 0f, float y = 0f)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
                return this;

            var reader = new PdfReader(pdfBytes);
            var imported = Writer.GetImportedPage(reader, page);

            if (canvas == null)
                canvas = CanvasUnder;

            canvas.AddTemplate(imported, x, y);

            return this;
        }

        public PDFDocument Join(byte[] pdfBuffer, bool inheritPageSize = false)
        {
            if (pdfBuffer == null || pdfBuffer.Length == 0)
                return this;

            var reader = new PdfReader(pdfBuffer);

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                var page = Writer.GetImportedPage(reader, i);

                if (inheritPageSize)
                    Document.SetPageSize(new Rectangle(0f, 0f, page.Width, page.Height));

                Document.NewPage();
                Canvas.AddTemplate(page, 0, 0);
            }

            return this;
        }

        public void Close()
        {
            Document.Close();
        }

        public byte[] CloseAndGetBuffer()
        {
            Document.NewPage();
            Document.Close();

            if (_memoryDocument == null)
                return null;

            return _memoryDocument.GetBuffer();
        }

        public void DrawRulers()
        {
            PrintXAxis((int)Document.Top);
            PrintXAxis((int)Document.Bottom);
            PrintYAxis((int)Document.Left);
            PrintYAxis((int)Document.Right);
        }

        #endregion

        #region Metodos Privados

        private void PrintXAxis(int y)
        {
            var font = ITFont
                .FromBaseFont(
                    BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false),
                    7
                );

            int x = 0;
            while (x <= Width)
            {
                if (x % 20 == 0)
                {
                    Text("" + x, x, y + 8, Align.Center, font);
                    Text("|", x, y, Align.Center, font);
                }
                else
                {
                    Text(".", x, y, Align.Center, font);
                }
                x += 5;
            }
        }

        private void PrintYAxis(int x)
        {
            var font = ITFont
                .FromBaseFont(
                    BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false),
                    7
                );

            int y = 0;
            while (y <= Height)
            {
                if (y % 20 == 0)
                {
                    Text("__ " + y, x, y - 7f, Align.Left, font);
                }
                else
                {
                    Text("_", x, y - 7f, Align.Left, font);
                }
                y += 5;
            }
        }

        private void onStartingPage()
        {
            OnStartingPage?.Invoke(this);
        }

        private void onEndingPage()
        {
            OnEndingPage?.Invoke(this);
        }

        private void onClosingDocument()
        {
            OnClosingDocument?.Invoke(this);
        }

        private Image GetImage(byte[] buffer, float x, float y, float width = 0f, float height = 0f, ImageScale? scale = null, VerticalAlign? verticalAlign = null, float percent = 0f)
        {
            var img = iTextSharp.text.Image.GetInstance(buffer);

            switch (scale.Value)
            {
                case ImageScale.Auto:
                    if (width > 0 && height > 0)
                        img.ScaleAbsolute(width, height);
                    else if (percent > 0)
                        img.ScalePercent(percent);
                    else
                        img.ScaleAbsolute((img.Width * 72) / 96, (img.Height * 72) / 96);

                    img.SetAbsolutePosition(x, Height - y - img.ScaledHeight);
                    break;

                case ImageScale.Cover:
                    if (width > 0 && height > 0)
                    {
                        if (img.Width > img.Height)
                        {
                            var scaledHeight = height;
                            var scaledWidth = (height * img.Width) / img.Height;
                            img.ScaleToFit(scaledWidth, scaledHeight);
                            img.SetAbsolutePosition((x + (width / 2)) - (img.ScaledWidth / 2), Height - y - img.ScaledHeight);
                        }
                        else
                        {
                            var scaledWidth = width;
                            var scaledHeight = (width * img.Height) / img.Width;
                            img.ScaleToFit(scaledWidth, scaledHeight);
                            img.SetAbsolutePosition(x, Height - y - (height / 2) - (img.ScaledHeight / 2));
                        }
                    }
                    break;

                case ImageScale.Contain:
                    if (width > 0 && height > 0)
                    {
                        img.ScaleToFit(width, height);

                        var alignX = x + (width / 2) - (img.ScaledWidth / 2);
                        var alignY = Height - y;

                        switch (verticalAlign.Value)
                        {
                            case VerticalAlign.Top:
                                alignY = Height - y - img.ScaledHeight;
                                break;

                            case VerticalAlign.Center:
                                alignY = Height - y - ((img.ScaledHeight + height) / 2);
                                break;

                            case VerticalAlign.Bottom:
                                alignY = Height - y - height;
                                break;
                        }

                        img.SetAbsolutePosition(alignX, alignY);
                    }
                    else
                        img.SetAbsolutePosition(x, Height - y - img.Height);
                    break;
            }

            return img;
        }

        private static void RenderTableCellBorder(Border border, PdfPCell cell)
        {
            cell.Border = iTextSharp.text.Rectangle.NO_BORDER;

            if (border != null)
            {
                cell.BorderColor = new BaseColor(border.Color.Value);

                if (border.All > 0)
                {
                    cell.Border = iTextSharp.text.Rectangle.BOX;
                    cell.BorderWidth = border.All;
                }
                else
                {
                    cell.BorderWidthTop = border.Top;
                    cell.BorderWidthLeft = border.Left;
                    cell.BorderWidthBottom = border.Bottom;
                    cell.BorderWidthRight = border.Right;
                }
            }
        }

        private static void RenderTableCellPadding(Padding padding, PdfPCell cell)
        {
            if (padding != null)
            {
                if (padding.All > 0)
                {
                    cell.Padding = padding.All;
                }
                else
                {
                    cell.PaddingLeft = padding.Left;
                    cell.PaddingRight = padding.Right;
                    cell.PaddingTop = padding.Top;
                    cell.PaddingBottom = padding.Botton;
                }
            }
        }

        private Font CreateFont(ITFont font, System.Drawing.Color? color, bool bold)
        {
            var setITFont = font ?? DefaultFont;
            var setFont = new Font(setITFont.BaseFont, setITFont.Size);

            if (color != null)
                setFont.Color = new BaseColor(color.Value);

            if (bold)
                setFont.SetStyle("bold");
            return setFont;
        }

        #endregion
    }
}
