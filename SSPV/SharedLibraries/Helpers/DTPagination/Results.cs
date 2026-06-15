using System.Collections.Generic;

namespace DTPagination
{
    public class Results<T>
    {
        #region Propiedades

        public int draw { get; set; }

        public int recordsTotal { get; set; }

        public int recordsFiltered { get; set; }

        public List<T> data { get; set; }

        #endregion
    }
}
