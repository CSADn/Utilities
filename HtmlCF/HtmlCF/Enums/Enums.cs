using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace HtmlCF
{
    public enum ElementType
    {
        [Description("Normal")]
        Normal,

        [Description("Corte de captura")]
        CaptureBreak
    }

    public enum SelectorType
    {
        [Description("Id")]
        Id,

        [Description("Clase")]
        Class,

        [Description("Propiedad")]
        Property,

        [Description("Atributo")]
        Attribute,

        [Description("Función")]
        Function
    }

    public enum CaptureType
    {
        [Description("Propiedad")]
        Property,

        [Description("Atributo")]
        Attribute,

        [Description("Función")]
        Function,

        [Description("Condición")]
        Condition
    }

}
