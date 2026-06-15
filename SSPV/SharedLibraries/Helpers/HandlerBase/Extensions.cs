using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class Extensions
    {
        public static List<IAutoCrud> AddAutoCrud<T>(this List<IAutoCrud> input, object fCruds, IAutoCrudsHandler handler, string dtNameSpace = "EntitiesDT", string customNameSpace = "EntitiesCustom", string[] exceptedMethods = null, bool insertPK = false, bool useCustom = false, bool logicDelete = true, bool useTransaction = true, bool upperStrings = true, string[] excludedProps = null, bool readOnly = false) where T: class
        {
            input.Add(new AutoCrud<T>(fCruds, handler, dtNameSpace, customNameSpace, exceptedMethods, insertPK, useCustom, logicDelete, useTransaction, upperStrings, excludedProps, readOnly));
            return input;
        }
    }
}
