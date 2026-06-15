namespace iTextSharpPDF
{
    public class TableColumn
    {
        public string Header { get; set; }

        public bool BoldHeader { get; set; }

        public float FontHeaderSize { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public Align Align { get; set; }

        public Border Border { get; set; }

        public Padding Padding { get; set; }

        public bool IsImage { get; set; }

        public bool FitImage { get; set; }

        public bool Bold { get; set; }
                
        public ImageScale ImageScale { get; set; }

        public VerticalAlign? VerticalAlign { get; set; }

        public float ImageWidth { get; set; }

        public float ImageHeight { get; set; }
        
        public TableColumn()
        {
            //
        }
    }
}
