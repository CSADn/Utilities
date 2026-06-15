using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTables
{
    public class InputSettings<T>
    {
        public int draw { get; set; }

        public int start { get; set; }

        public int length { get; set; }

        public string search { get; set; }

        public string orderColumn { get; set; }

        public OrderDirection? orderDir { get; set; }

        public Func<List<T>> entities { get; set; }


        public InputSettings()
        {
            //
        }
    }
}
