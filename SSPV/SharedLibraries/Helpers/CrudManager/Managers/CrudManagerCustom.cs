using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Helpers
{
    /// <summary>
    /// Clase que peremite tener un CrudManager con manejo de Entidades Custom
    /// </summary>
    /// <typeparam name="T">instancia del Manager</typeparam>
    /// <typeparam name="C">Entidad</typeparam>
    /// <typeparam name="D">Entidad Custom (Debe heredar de entidad)</typeparam>
    public abstract class CrudManagerCustom<T, C, D> : CrudManager<T, C>
        where T : class
        where C : class, new()
        where D : class, new()
    {

        #region Metodos privados

        private Type PrimaryKeyType()
        {
            string pkName = GetPrimaryKeyName();

            PropertyInfo pi = typeof(C).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                       .FirstOrDefault(f=> f.Name.ToLowerInvariant().Equals(pkName.ToLowerInvariant()));
            if (pi != null)
                return pi.PropertyType;
            else
                throw new InvalidPrimaryKeysException("La clave primaria no es correcta");
        }

        #endregion

        #region Metodos protegidos

        /// <summary>
        /// Clonar una entidad base pasando sus datos a una custom que hereda de la misma
        /// </summary>
        /// <typeparam name="C">Tipo de la entidad a Clonar</typeparam>
        /// <typeparam name="D">Tipo de la entidad Custom</typeparam>
        /// <param name="entity">Entidad a clonar</param>
        /// <returns></returns>
        protected E Clone<E, F>(F entity)
            where E : class, new()
            where F : class, new()
        {
            E retEnt = null;

            if (entity != null)
            {
                if (typeof(E).BaseType == entity.GetType())
                {
                    //Solo las propiedades de retorno que difieren de las originales
                    PropertyInfo[] retProps = typeof(D).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                    retEnt = new E();
                    foreach (PropertyInfo propiedad in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        try
                        {
                            object value = propiedad.GetValue(entity, null);

                            PropertyInfo retProp = retProps.FirstOrDefault(f => f.Name == propiedad.Name);

                            if (retProp != null && retProp.CanWrite) //Existe en la clase heredada
                            {
                                var targetType = retProp.PropertyType.IsNullableType() ? Nullable.GetUnderlyingType(retProp.PropertyType) : retProp.PropertyType;
                                var convertedValue = Convert.ChangeType(value, targetType);
                                retProp.SetValue(retEnt, convertedValue, null);
                            }
                            else
                            {
                                if (propiedad.CanWrite)
                                    propiedad.SetValue(retEnt, propiedad.GetValue(entity, null), null);
                            }
                        }
                        catch { }
                    }
                }
                else
                    throw new InvalidCastException("La entidad a clonar tiene que heredar de " + entity.GetType().Name);
            }

            return retEnt;
        }

        #endregion

        #region Metodos Custom

        /// <summary>
        /// Obtener entidad custom por Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual D GetCustomById(object id)
        {
            if (typeof(D).BaseType != typeof(C))
                throw new InvalidCastException("La entidad Custom debe heredar de " + typeof(C).Name);

            D retEnt = null;
            C entity = GetById(id);

            retEnt = Clone<D, C>(entity);

            if (retEnt != null)
                FillCustomProperties(retEnt);

            return retEnt;
        }

        /// <summary>
        /// Actualizar o insertar entidad custom
        /// </summary>
        /// <param name="entityCustom"></param>
        /// <returns></returns>
        public virtual List<object> AddOrUpdateCustom(D entityCustom, bool insertKey = false, bool allowTrigger = false)
        {
            bool commitOK = false;
            List<object> IdRet = new List<object>();

            commitOK = _dataModel
                .Transaction(scope =>
                {
                    C toAdd = entityCustom.Back<D, C>();

                    IdRet = AddOrUpdate(toAdd, false, insertKey, allowTrigger);

                    var id = Convert.ChangeType(IdRet.First(), PrimaryKeyType());

                    AddCustomEntities(entityCustom, id);

                }, true);

            if (!commitOK)
                return new List<object> { -1 };
            else
                return IdRet;

        }

        /// <summary>
        /// Actualizar o insertar entidad custom (sin transacción)
        /// </summary>
        /// <param name="entityCustom">Entidad a insertar</param>
        /// <param name="insertKey">Insert PrimaryKey, default = true</param>
        /// <returns>Valores de la clave primaria</returns>
        public virtual List<object> AddOrUpdateCustomWithOutTran(D entityCustom, bool insertKey = true, bool allowTrigger = false)
        {
            List<object> LstRet = new List<object>();

            C toAdd = entityCustom.Back<D, C>();

            LstRet = AddOrUpdate(toAdd, false, insertKey, allowTrigger);
            AddCustomEntities(entityCustom, LstRet);

            return LstRet;
        }

        /// <summary>
        /// Obtener todo con entidades custom
        /// </summary>
        /// <returns>Lista de entidades D</returns>
        public virtual List<D> GetAllCustom()
        {
            List<D> retList = GetAll().ConvertAll(c => Clone<D, C>(c));

            if (retList != null && retList.Count > 0)
                retList.ForEach(f => FillCustomProperties(f));

            return retList;
        }

        /// <summary>
        /// Obtener Todo Con entidades custom
        /// </summary>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public virtual List<D> GetAllCustom(Expression<Func<C, bool>> whereExpr)
        {
            List<D> retList = GetAll(whereExpr).ConvertAll(c => Clone<D, C>(c));

            if (retList != null && retList.Count > 0)
                retList.ForEach(f => FillCustomProperties(f));

            return retList;
        }

        /// <summary>
        /// Obtener Entidades Custom
        /// </summary>
        /// <param name="whereExpr"></param>
        /// <param name="useFixedfilters"></param>
        /// <returns></returns>
        public virtual List<D> GetCustomBy(Expression<Func<C, bool>> whereExpr, bool useFixedfilters = true)
        {
            List<D> retList = GetBy(whereExpr, useFixedfilters).ConvertAll(c => Clone<D, C>(c));

            if (retList != null && retList.Count > 0)
                retList.ForEach(f => FillCustomProperties(f));

            return retList;
        }

        #endregion

        #region Metodos Abstractos

        /// <summary>
        /// Metodo necesario para insertar las propiedades de la entidad custom, tablas relaccionadas
        /// </summary>
        /// <param name="entity">Entidad Custom</param>
        /// <param name="pk">Id de la entidad</param>
        protected abstract void AddCustomEntities(D entity, object pk);

        /// <summary>
        /// Metodo necesario para establecer las propiedades de la entidad custom
        /// </summary>
        /// <param name="entityCustom"></param>
        protected abstract void FillCustomProperties(D entityCustom);

        #endregion

        #region Metodos Virtuales

        protected virtual void AddCustomEntities(D entity, List<object> pks)
        {
            throw new NotImplementedException("Debe implementar este metodo en la clase heredada");
        }

        #endregion
    }
}
