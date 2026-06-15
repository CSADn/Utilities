using System;

namespace Helpers
{
    /// <summary>
    /// Metodos a implementar para que el HandlerBase soporte autocruds
    /// </summary>
    public interface IAutoCrudsHandler
    {
        #region Métodos

        string GetAll(dynamic invoke);

        string GetAllDT(dynamic invoke); //Paginado normal

        string GetAllDT(dynamic invoke, Func<string, int> count); //Paginado con parametros

        string GetAllCustom(dynamic invoke);

        string GetById(dynamic invoke, string idParam);

        string GetCustomById(dynamic invoke, string idParam);

        //En la implementación de este metodo en el handler, se debe llamar al AddOrUpdate con allowTriggers (el de 4 parametros del CrudManager
        //Por ese motivo, debe pasarse un false o true
        //Este es el código que debe usarse en el handler 
        //  => AddOrUpdate(_context, invoke, useTransaction, insertKey, false, upperStrings, excludedProps);
        string AddOrUpdate(dynamic invoke, bool useTransaction, bool insertKey, bool upperStrings, params string[] excludedProps);

        string AddOrUpdateCustom(dynamic invoke, bool upperStrings, bool insertKey, params string[] excludedProps);

        string Delete(dynamic invoke, string idParam, bool logicDelete);

        #endregion
    }
}
