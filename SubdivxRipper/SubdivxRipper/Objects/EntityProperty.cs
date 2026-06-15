using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubdivxRipper.Objects
{
    public class EntityProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string SqlValue { get; set; }

        public EntityProperty()
        {
            //
        }
    }
}
