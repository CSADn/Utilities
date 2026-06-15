using System.Collections.Generic;

namespace Helpers
{
    /// <summary>
    /// Interface para los cruds que requieren devolver entidades DT
    /// </summary>
    /// <typeparam name="DT">Entidad para DataTable </typeparam>
    public interface IDataTable<DT> 
        where DT: class, new()
    {
        /// <summary>
        /// Obtener todo para entidad DataTable
        /// </summary>
        /// <returns></returns>
        List<DT> GetAllDT();
    }
}
