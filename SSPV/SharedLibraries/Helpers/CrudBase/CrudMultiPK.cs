using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Helpers
{
    /// <summary>
    /// Crud Base Generico para entidades con clave primaria de mas de un campo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="C"></typeparam>
    public abstract class CrudMultiPK<T, C> : CrudBase<T, C>
        where T : class
        where C : class, new()
    {
        #region Properties

        /// <summary>
        /// Array para clave primaria compuesta
        /// </summary>
        protected abstract string[] PkColumnNames { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        protected CrudMultiPK()
        {

        }

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
        /// Obtener todo filtrando por alguna de las pk
        /// </summary>
        /// <param name="pk">Campo de clave primaria por el que se va a filtrar</param>
        /// <param name="idPk">Valor del id de la clave primaroa</param>
        /// <returns></returns>
        public virtual List<C> GetAll(string pkColumn, int idPk)
        {
            //Verificar que la columna exista en las pk
            if (string.IsNullOrWhiteSpace(pkColumn) || !PkColumnNames.Any(a => a.Equals(pkColumn)))
                return null;

            return base.GetAll(TableName, pkColumn, idPk,  FilterColumnName, FilterValue, CryptProps);
        }

        /// <summary>
        /// Obtener por lista de id de algún campo de clave primaria
        /// </summary>
        /// <param name="pkColumn">Campo de clave primaria</param>
        /// <param name="idList">Lista de Ids</param>
        /// <returns></returns>
        public virtual List<C> GetByIdList(string pkColumn, int[] idList)
        {
            return base.GetByIdList(TableName, pkColumn, idList, FilterColumnName, FilterValue, CryptProps);
        }

        /// <summary>
        /// Obtener por clave primaria compuesta (enteros)
        /// </summary>
        /// <param name="pksValues"></param>
        /// <returns></returns>
        public virtual C GetById(int[] pksValues)
        {
            string query = @"SELECT * FROM #TABLE_NAME WHERE ".Replace("#TABLE_NAME", TableName);

            if (PkColumnNames == null || PkColumnNames.Length == 0)
                throw new NullReferenceException("Debe establecer los campos de clave primaria");

            if (pksValues == null || pksValues.Length == 0)
                throw new NullReferenceException("Debe establecer los valores de clave primaria");
            
            if (PkColumnNames.Length != pksValues.Length)
                throw new Exception("Los id ingresados no coinciden con las columnas de clave primaria");

            for (int i = 0; i < PkColumnNames.Length; i++)
            {
                query += PkColumnNames[i] + " = ?";

                if (i < PkColumnNames.Length - 1)
                    query += " AND ";
            }

            C retValue = null;

            object[] parameters = pksValues.Select(s => (object)s).ToArray();
            if (CryptProps != null && CryptProps.Length > 0)
                retValue = _dataModel.Execute<C>(query, parameters).EntityCypher(Utilities.CypherAction.Decrypt, CryptProps).FirstOrDefault();
            else
                retValue = _dataModel.Execute<C>(query, parameters).FirstOrDefault();

            return retValue;
        }

        /// <summary>
        /// Obtiene por clave primaria compuesta (object)
        /// </summary>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public virtual C GetBy(object[] pkValues)
        {
            return this.GetBy(TableName, PkColumnNames, pkValues, FilterColumnName, FilterValue, CryptProps).FirstOrDefault();
        }

        /// <summary>
        /// Obtener una lista de entidades por clave primaria compuesta (1 o todas)
        /// </summary>
        /// <param name="pkColumns">Columnas de la clave primaria</param>
        /// <param name="pkValues">Valores de las columnas</param>
        /// <returns></returns>
        public virtual List<C> GetBy(string[] pkColumns, object[] pkValues)
        {
            return this.GetBy(TableName, pkColumns, pkValues, FilterColumnName, FilterValue, CryptProps);
        }

        #endregion

        #region CRUD

        /// <summary>
        /// Actualizar o insertar una entidad
        /// </summary>
        /// <param name="entity">Entidad</param>
        /// <param name="useTransaction">true, usar transacción</param>
        /// <returns></returns>
        public virtual List<object> AddOrUpdate(C entity, bool useTransaction = false)
        {
            List<Expression<Func<C, object>>> pks = new List<Expression<Func<C, object>>>();

            if (entity != null && CryptProps != null && CryptProps.Length > 0)
                entity = Utilities.EntityCypher(entity, Utilities.CypherAction.Crypt, CryptProps);

            foreach (var item in PkColumnNames)
                pks.Add(MakeExpressionFromEntity("pk", item));

            return _dataModel.InsertOrUpdate(useTransaction, true, entity, pks.ToArray());
        }

        /// <summary>
        /// Eliminar por valor de clave primaria
        /// </summary>
        /// <param name="pksValues">Valores de la clave primaria</param>
        /// <returns></returns>
        public virtual bool Delete(object[] pksValues)
        {
            
            if (string.IsNullOrWhiteSpace(TableName))
                throw new NullReferenceException("Debe establecer el nombre de tabla");

            if (PkColumnNames == null || PkColumnNames.Length == 0)
                throw new NullReferenceException("Debe establecer los campos de clave primaria a eliminar");

            if (pksValues == null || pksValues.Length == 0)
                throw new NullReferenceException("Debe establecer los valores de clave primaria a eliminar");

            if (PkColumnNames.Length != pksValues.Length)
                throw new Exception("Los id ingresados no coinciden con las columnas de clave primaria");

            string query = @"DELETE FROM #TABLE_NAME WHERE ".Replace("#TABLE_NAME", TableName);

            for (int i = 0; i < PkColumnNames.Length; i++)
            {
                query += PkColumnNames[i] + " = ?";

                if (i < PkColumnNames.Length - 1)
                    query += " AND ";
            }

            return _dataModel.ExecuteNonQuery(query, pksValues) > 0;
        }

        public virtual bool Delete(string pkColumn, int idPk)
        {
            return base.Delete(TableName, pkColumn, idPk);
        }

        #endregion

        #endregion
    }
}
