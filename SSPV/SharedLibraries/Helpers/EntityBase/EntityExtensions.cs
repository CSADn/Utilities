using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Helpers
{
    public static class EntityExtensions
    {
        private static Dictionary<Type, object> _dicDefaultTypes = new Dictionary<Type, object>()
        {
            { typeof(bool), default(bool) },
            { typeof(byte), default(byte) },
            { typeof(char), default(char) },
            { typeof(decimal), default(decimal) },
            { typeof(double), default(double) },
            { typeof(float), default(float) },
            { typeof(int), default(int) },
            { typeof(long), default(long) },
            { typeof(sbyte), default(sbyte) },
            { typeof(short), default(short) },
            { typeof(uint), default(uint) },
            { typeof(ulong), default(ulong) },
            { typeof(ushort), default(ushort) },
        };

        private static D CloneInternal<D>(object entity)
            //where T : class, new()
            where D : class, new()
        {
            D retEnt = new D();

            //Solo las propiedades de retorno que difieren de las originales
            PropertyInfo[] retProps = typeof(D).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            retEnt = new D();
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
            return retEnt;
        }

        /// <summary>
        /// Clonar una entidad base pasando sus datos a una custom que hereda de la misma
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad a clonar</typeparam>
        /// <typeparam name="D">Tipo de la entidad Custom</typeparam>
        /// <param name="entity">Entidad a clonar</param>
        /// <returns></returns>
        public static D Clone<T, D>(this EntityBase<T> entity) 
            where T: class, new() 
            where D: class, new()
        {
            D retEnt = new D();

            if (entity != null)
            {
                if (typeof(D).BaseType == entity.GetType())
                   retEnt = CloneInternal<D>(entity);
                else
                    throw new InvalidCastException("La entidad a clonar tiene que heredar de " + entity.GetType().Name);
            }

            return retEnt;
        }

        public static D Clone<T, D>(this IEntityClone<T> entity) where T : class, new() where D : class, new()
            => CloneInternal<D>(entity);

            
        public static List<D> CloneList<T, D>(this List<T> entities)
            where T : EntityBase<T>, new() 
            where D : class, new()
        {
            List<D> retList = new List<D>();

            if(entities != null && entities.Count > 0)
                entities.ForEach(f => retList.Add(f.Clone<T, D>()));
            
            return retList;
        }

        /// <summary>
        /// Clonar las propiedades de una entidad custom que se corresponden con su entidad base
        /// </summary>
        /// <typeparam name="D">Tipo de la entidad Custom</typeparam>
        /// <typeparam name="T">Tipo de la entidad Base</typeparam>
        /// <param name="entity">Entidad custom a clonar</param>
        /// <returns></returns>
        public static T Back<D, T>(this D entityCustom)
            where T : class, new()
            where D : class, new()
        {
            if (entityCustom == null)
                throw new NullReferenceException("La entidad a clonar no puede ser nula");

            if (entityCustom.GetType().BaseType != typeof(T))
                throw new InvalidCastException("La entidad de origen debe heredar de " + typeof(T).Name);

            T retEnt = new T();
            if (typeof(T).BaseType == typeof(EntityBase<T>))
            {
                PropertyInfo[] retProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] declaredProps = entityCustom.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                //.Where(w => retProps.Any(a => a.Name == w.Name))

                foreach (PropertyInfo propiedad in entityCustom.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(w => retProps.Any(a => a.Name == w.Name)))
                {
                    try
                    {
                        if (propiedad.CanWrite)
                        {
                            object value = propiedad.GetValue(entityCustom, null);
                            var declared = declaredProps.FirstOrDefault(a => a.Name == propiedad.Name);
                            var retProd = retProps.FirstOrDefault(f => f.Name == propiedad.Name);

                            if (declared != null && retProd != null)
                            {
                                if (propiedad.PropertyType == retProd.PropertyType)
                                    continue;
                                else
                                {
                                    Type targetType = propiedad.PropertyType.IsNullableType() ? Nullable.GetUnderlyingType(propiedad.PropertyType) : propiedad.PropertyType;

                                    if (value == null && targetType.IsValueType)
                                        value = Activator.CreateInstance(targetType);
                                    
                                    retProd.SetValue(retEnt, value, null);
                                }
                            }
                            else
                            { 
                                
                                propiedad.SetValue(retEnt, value, null); //Convert.ChangeType(value, propiedad.PropertyType)
                            }
                        }
                    }
                    catch { }
                }
            }
            else
                throw new InvalidCastException("La entidad de destino debe heredar de " + typeof(EntityBase<T>).Name);


            return retEnt;
        }

        /// <summary>
        /// Devuelve una entidad T mapeando por nombre de propiedades y columnas del datarow
        /// </summary>
        /// <typeparam name="T">Entidad</typeparam>
        /// <param name="row">Fila del datarow a mapear</param>
        /// <returns>Entidad</returns>
        public static T ToEntity<T>(this DataRow row) where T : class, new()
        {
            if (row == null)
                throw new ArgumentNullException();

            Type type = typeof(T);
            
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            if (properties == null || properties.Count() == 0 )
                throw new NullReferenceException($"La clase {type.Name} no posee propiedades");

            Dictionary<string, DataColumn> dicColumns = null;

            if (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(EntityBase<>))
            {
                FieldInfo field = type.GetField("DicColums", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (field == null)
                    throw new NullReferenceException($"La clase {type.Name} no posee el campo DicColumns");

                dicColumns = field.GetValue(null) as Dictionary<string, DataColumn>;
            }

            if (dicColumns == null || dicColumns.Count == 0)
            {
                DataTable dt = row.Table;
                dicColumns = new Dictionary<string, DataColumn>();
                dt.Columns.Cast<DataColumn>().ToList().ForEach(f => dicColumns.Add(f.ColumnName.ToLowerInvariant(), f));
            }

            T retValue = new T();

            for (int i = 0; i < properties.Count(); i++)
            {
                PropertyInfo pi = properties[i];

                if (pi.CanWrite)
                {
                    DataColumn dc = null;

                    if (dicColumns.TryGetValue(pi.Name.ToLowerInvariant(), out dc))
                    {
                        object value = row[dc.ColumnName];
                        try
                        {
                            if (pi.PropertyType.IsEnum)
                                pi.SetValue(retValue, Enum.ToObject(pi.PropertyType, value));
                            else if (value == DBNull.Value)
                                pi.SetValue(retValue, pi.PropertyType.GetDefault());
                            else
                                pi.SetValue(retValue, Convert.ChangeType(value, Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType));
                        }
                        catch (Exception)
                        {
                            throw new InvalidCastException($"No se pudo convertir el valor {value} al tipo {pi.PropertyType.Name}");
                        }
                    }
                }
            }
            
            return retValue;
        }

    }
}
