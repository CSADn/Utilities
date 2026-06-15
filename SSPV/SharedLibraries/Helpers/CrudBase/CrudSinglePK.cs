using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace Helpers
{
    /// <summary>
    /// CRUD para entidades con clave primaria de un campo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="C"></typeparam>
    public abstract class CrudSinglePK<T, C> : CrudBase<T, C>, ICrudBase<C>
        where T : class
        where C : class, new()
    {
        #region Properties

        /// <summary>
        /// Campo de Id en la DB
        /// </summary>
        protected abstract string PkColumnName { get; }

        /// <summary>
        /// Nombre de Id de columna FK
        /// </summary>
        protected virtual string FkColumnName { get; set; }

        /// <summary>
        /// Nombre de la columna para obtener los datos por código
        /// </summary>
        protected virtual string CodColumnName { get; set; }

        /// <summary>
        /// Especifica si se eliminan los registros de las tablas referenciadas por el registro a eliminar
        /// </summary>
        protected virtual bool DeleteParents { get; set; }

        /// <summary>
        /// Lista de Entidades de las cuales se eliminara en caso de elegir DeleteParent en true
        /// </summary>
        protected virtual List<Type> ParentsTables { get; set; }

        #endregion

        #region Virtual Methods

        #region Select

        /// <summary>
        /// Obtener todo
        /// </summary>
        /// <returns></returns>
        public virtual List<C> GetAll()
        {
            return base.GetAll(TableName, FilterColumnName, FilterValue, CryptProps);
        }

        /// <summary>
        /// Obtener todo para tablas con una Foreign key a una tabla maestra
        /// </summary>
        /// <param name="idFK">Valor de la Foreign Key por el cual se filtrará</param>
        /// <returns></returns>
        public virtual List<C> GetAll(int idFK)
        {
            return base.GetAll(TableName, FkColumnName, idFK, FilterColumnName, FilterValue, CryptProps);
        }

        /// <summary>
        /// Obtener por id
        /// </summary>
        /// <param name="id">Id de la entidad a devolver</param>
        /// <returns></returns>
        public virtual C GetById(int id)
        {
            return base.GetById(TableName, PkColumnName, id, FilterColumnName, FilterValue, CryptProps);
        }

        /// <summary>
        /// Obtener por clave primaria y clave foranea
        /// </summary>
        /// <param name="idPK">Valor de la clave primaria</param>
        /// <param name="IdFK">Valor de la clave foranea</param>
        /// <returns></returns>
        public virtual C GetById(int idPK, int idFK)
        {
            return base.GetById(TableName, PkColumnName, idPK, FkColumnName, idFK, FilterColumnName, FilterValue, CryptProps);
        }

        /// <summary>
        /// Obtener por lista de id
        /// </summary>
        /// <param name="idList">lista con los id de las entidades a devolver</param>
        /// <returns></returns>
        public virtual List<C> GetByIdList(int[] idList)
        {
            return base.GetByIdList(TableName, PkColumnName, idList);
        }

        /// <summary>
        /// Obtener Entidad por Código
        /// </summary>
        /// <param name="cod">Valor del Campo Código</param>
        /// <returns></returns>
        public virtual C GetByCod(string cod)
        {
            return base.GetByCod(TableName, CodColumnName, cod, FilterColumnName, FilterValue, CryptProps);
        }

        /// <summary>
        /// Obtener entidad por una columna específica
        /// </summary>
        /// <param name="columnName">Nombre de la columna</param>
        /// <param name="value">Valor por el que se desaa obtener la entidad</param>
        /// <returns></returns>
        public virtual List<C> GetBy(string columnName, object value)
        {
            return base.GetBy(TableName, columnName, value, FilterColumnName, FilterValue, CryptProps);
        }

        /// <summary>
        /// Obtener la entidad por una seria de columnas
        /// </summary>
        /// <param name="columns">Nombre de las columnas por las cuales se desea obtener la entidad</param>
        /// <param name="values">Valores de las columnas</param>
        /// <returns></returns>
        public virtual List<C> GetBy(string[] columns, object[] values)
        {
            return base.GetBy(TableName, columns, values, FilterColumnName, FilterValue, CryptProps);
        }
        
        /// <summary>
        /// Obtener cantidad de registros por Campo
        /// </summary>
        /// <param name="column">Columna por la que se realizara el count</param>
        /// <param name="value">valor de la columna</param>
        /// <returns></returns>
        public virtual int GetCount(string column, object value)
        {
            return base.GetCountBy(TableName, column, value, FilterColumnName, FilterValue);
        }

        #endregion

        #region CRUD
        
        /// <summary>
        /// Actualizar o insertar una entidad
        /// </summary>
        /// <param name="entity">Entidad</param>
        /// <param name="useTransaction">true, usar transacción</param>
        /// <returns></returns>
        public virtual List<object> AddOrUpdate(C entity, bool useTransaction = true, bool insertKey = false)
        {
            return base.AddOrUpdate(entity, PkColumnName, useTransaction, insertKey, CryptProps);
        }

        /// <summary>
        /// Eliminar una entidad
        /// </summary>
        /// <param name="id">Valor de la clave primaria</param>
        /// <param name="logicDelete">true = Baja Lógica</param>
        /// <returns></returns>
        public virtual bool Delete(int id, bool logicDelete = false)
        {
            return base.Delete(TableName, PkColumnName, id, logicDelete, DeleteParents, ParentsTables, FilterColumnName, FilterValue);
        }

        /// <summary>
        /// Eliminar por PK y FK
        /// </summary>
        /// <param name="idPK">Valor de la clave primaria</param>
        /// <param name="idFK">Valor de la clave foranea</param>
        /// <returns></returns>
        public virtual bool Delete(int idPK, int idFK)
        {
            return base.Delete(TableName, PkColumnName, idPK, FkColumnName, idFK);
        }

        /// <summary>
        /// Eliminar entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <param name="logicDelete">true = baja lógica</param>
        /// <returns></returns>
        public virtual bool Delete(C entity, bool logicDelete = false)
        {
            PropertyInfo pInfo = base.GetPropertyInfo(PkColumnName);
            
            if (pInfo == null) return false;

            object value = pInfo.GetValue(entity, null);
            
            if (value == null) return false;

            return base.Delete(TableName, PkColumnName, Convert.ToInt32(value), logicDelete, FilterColumnName, FilterValue);
        }

        #endregion

        #endregion
    }
}
