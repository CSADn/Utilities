using System;
using System.Collections.Generic;

namespace DTPagination
{
    public class InputSettings<T>
    {
        #region Propiedades

        public int Draw { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public string Search { get; set; }

        public string[] OrderColumns { get; set; }

        public Func<int, int, string, string[], List<T>> Entities { get; set; }

        public Func<T, int> Count { get; set; }

        #endregion

    }
}
