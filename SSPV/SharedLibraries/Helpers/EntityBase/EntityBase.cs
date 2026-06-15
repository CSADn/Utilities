using System.Collections.Generic;
using System.Data;

namespace Helpers
{
    public abstract class EntityBase<T> 
        where T: class, new()
    {
        #region Campos protegidos

        protected static Dictionary<string, DataColumn> DicColums = null;

        #endregion

        #region Constructor

        static EntityBase()
        {
            DicColums = new Dictionary<string, DataColumn>();
        }

        #endregion
        
        #region Metodos

        public static T Create(DataRow dr) => dr.ToEntity<T>();
           
        #endregion
    }
}
