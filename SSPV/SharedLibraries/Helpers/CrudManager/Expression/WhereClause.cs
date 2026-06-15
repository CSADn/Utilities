using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public class WhereClause
    {
        #region Propiedades

        public string Where { get; set; }

        public List<object> Parameters { get; set; }

        #endregion

        #region Constructor

        public WhereClause()
        {

        }

        public WhereClause(string where, List<object> parameters)
            : this()
        {
            Where = where;
            Parameters = parameters;
        }

        #endregion
    }
}
