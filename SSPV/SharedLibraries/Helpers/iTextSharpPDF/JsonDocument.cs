using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharpPDF
{
    public class JsonDocument
    {
        public bool Disabled { get; set; }

        public PageSizeEx PageSize { get; set; }

        public string MergeWith { get; set; }

        public ITFont GlobalFont { get; set; }

        public Color GlobalColor { get; set; }

        public Color GlobalFill { get; set; }

        public bool MultiPage { get; set; }

        public Margins Margins { get; set; }

        public string NewPageTemplate { get; set; }

        public Margins NewPageMargins { get; set; }

        public List<JsonElement> Elements { get; set; }

        public List<JsonElement> NewPageElements { get; set; }

        public HeaderFooter Header { get; set; }

        public HeaderFooter Footer { get; set; }

        public bool BlankPage { get; set; }


        public JsonDocument()
        {
            Disabled = false;
            PageSize = new PageSizeEx(iTextSharp.text.PageSize.A4);
            GlobalFont = ITFont.Helvetica;
            GlobalColor = Color.Black;
            GlobalFill = Color.Black;
            MultiPage = false;
            Margins = new Margins(15f);
            NewPageTemplate = null;
            NewPageMargins = new Margins(15f);
        }


        public JsonDocument Clone(bool cloneElements = false)
        {
            var cloned = new JsonDocument
            {
                Disabled = this.Disabled,
                PageSize = this.PageSize,
                MergeWith = this.MergeWith,
                GlobalFont = this.GlobalFont,
                GlobalColor = this.GlobalColor,
                GlobalFill = this.GlobalFill,
                MultiPage = this.MultiPage,
                Margins = this.Margins,
                NewPageTemplate = this.NewPageTemplate,
                NewPageMargins = this.NewPageMargins,
                Header = this.Header,

                Elements = this.Elements
                    ?.Select(s => s.Clone())
                    .ToList(),

                NewPageElements = this.NewPageElements
                    ?.Select(s => s.Clone())
                    .ToList()
            };

            return cloned;
        }
    }
}
