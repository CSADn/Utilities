using System.Collections.Generic;

namespace Helpers
{
    public interface ICrudBase<C> 
        where C : class
    {
        #region Metodos select 
        
        List<C> GetAll();

        List<C> GetAll(int idFK);

        C GetById(int id);

        C GetById(int idPK, int idFK);

        List<C> GetByIdList(int[] idList);

        C GetByCod(string cod);

        #endregion
        
        #region Metodos CRUD

        List<object> AddOrUpdate(C entity, bool useTransaction = true, bool insertKey = false);

        bool Delete(int id, bool logicDelete = false);

        bool Delete(int idPK, int idFK);

        #endregion
    }
}
