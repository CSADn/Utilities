using System.Collections.Generic;

namespace Helpers
{
    public class SearchCondition
    {
        #region Propiedades

        public string Search { get; set; }

        public string Where
        {
            get
            {
                if (Fields == null || Fields.Count == 0)
                    return string.Empty;
                else
                    return "(" + string.Join(" OR ", Fields) + ")";
            }
        }

        public List<object> Parameters { get; set; }

        public List<string> Fields { get; set; }

        #endregion

        #region Constructor

        public SearchCondition()
        {
            Parameters = new List<object>();
            Fields = new List<string>();
        }

        #endregion
    }
}
