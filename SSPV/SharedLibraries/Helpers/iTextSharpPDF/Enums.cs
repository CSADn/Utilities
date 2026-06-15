using System;

namespace iTextSharpPDF
{
    public enum Align
    {
        Left,
        Center,
        Right,
        Justified
    }

    public enum VerticalAlign
    {
        Top,
        Center,
        Bottom
    }

    public enum ElementType
    {
        Text,
        Textblock,
        TextColumns,
        Paragraph,
        Rectangle,
        Barcode,
        Table,
        Image
    }

    public enum ImageScale
    {
        Auto,
        Cover,
        Contain
    }
}
