using System.Data;

namespace Helpers
{
    public class CrudFKTable
    {
        #region Propiedades

        public string PkTable { get; set; }

        public string FkTable { get; set; }

        public string FkName { get; set; }

        #endregion

        #region Factory Method

        public static CrudFKTable Create(DataRow dr)
        {
            return dr.ToEntity<CrudFKTable>();
        }

        #endregion

    }
}
