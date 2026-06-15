using IBMU2.UODOTNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using uniVerseSise.DBManager;
using uniVerseSise.Helpers;
using uniVerseSise.Manager.Helpers;
using Helpers;
using uniVerseSise.Entidades;

namespace uniVerseSise.Manager
{
    /// <summary>
    /// Manager de las tablas
    /// </summary>
    public class TablaManager : Singleton<TablaManager>
    {
        #region Constantes

        public static DateTime BaseDate = new DateTime(1967, 12, 31);

        public enum Comandos
        {
            [Description("SELECT")]
            SELECT
        }

        public enum Parametros
        {
            [Description("SAMPLE 5")]
            SAMPLE_5
        }

        #endregion

        #region Constructor

        private TablaManager()
        {

        }

        #endregion

        #region Metodos

        /// <summary>
        /// Retorna todos los datos de un almacenamiento
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="consulta"></param>
        /// <returns></returns>
        public List<T> ExecuteCommand<T>(string commando, int count = -1, string parameters = null)
            where T : class, new()
        {

            AtrtibuteInspectorManager inspector = new AtrtibuteInspectorManager(typeof(T));

            try
            {
                if (ConexionUV.Current == null || !ConexionUV.Current.IsConnected)
                    throw new ApplicationException("No se ha establecido una conexion para ejecutar la consulta, debe establecer una conexion con anterioridad");

                UniDataSet uDs = ConexionUV.Current.EjecutarDataset($"{commando} {inspector.NombreArchivo.Archivo} {parameters}", count);

                if (uDs != null)
                    return FillClass<T>(inspector, uDs);

                return new List<T>();
            }
            catch (Exception ex)
            {
                string err = $"Error de lectura de datos de uniVersa: {ex.Message}";
                Logger.Instance.LogError(err + " - innerException: " + ex.ToString());
                throw new Exception(err, ex);
            }
        }

        public List<T> ExecuteCommandBulk<T>(string commando)
            where T : class, new()
        {

            AtrtibuteInspectorManager inspector = new AtrtibuteInspectorManager(typeof(T));

            try
            {
                if (ConexionUV.Current == null || !ConexionUV.Current.IsConnected)
                    throw new ApplicationException("No se ha establecido una conexion para ejecutar la consulta, debe establecer una conexion con anterioridad");

                UniDataSetResult uDs = ConexionUV.Current.EjecutarDataSetBulk(commando, inspector.NombreArchivo.Archivo);

                if (uDs.Ejecutado)
                    return FillClass<T>(inspector, uDs.DataSet);

                return new List<T>();
            }
            catch (Exception ex)
            {
                string err = $"Error de lectura de datos de uniVersa: {ex.Message}";
                Logger.Instance.LogError(err + " - innerException: " + ex.ToString());
                throw new Exception(err, ex);
            }
        }

        public List<T> ExecuteCommand<T>(Comandos commando, Parametros parameters)
            where T : class, new()
        {
            return ExecuteCommand<T>(commando.Description(), -1, parameters.Description());
        }

        public List<T> ExecuteCommand<T>(Comandos commando)
            where T : class, new()
        {
            return ExecuteCommand<T>(commando.Description());
        }

        public List<T> ExecuteCommand<T>(Comandos commando, string parametros)
            where T : class, new()
        {
            return ExecuteCommand<T>(commando.Description(), -1, parametros);
        }

        private static List<T> FillClass<T>(AtrtibuteInspectorManager inspector, UniDataSet uDs) where T : class, new()
        {
            List<T> retValue = new List<T>();

            foreach (UniRecord ur in uDs)
            {
                if (ur.RecordReturnValue != 0) //Si llege al final de archivo
                    continue;

                T value = new T();
                retValue.Add(value);

                foreach (var item in inspector.DataCampoPropiedad)
                {
                    if (item.Key.Id)
                        item.Value.SetValue(value, ur.RecordID);
                    else if (item.Key.Text)
                        item.Value.SetValue(value, ur.Record.StringValue);
                    else
                    {
                        for (int i = 1; i < ur.Record.Count() + 1; i++)
                        {
                            if (item.Key.Position != i)
                                continue;

                            //Reservado para campos multivaluados
                            if (ur.Record.Dcount(i) > 1)
                            {
                                //Campo Multivaluado
                                //Si tiene array de subitems, y es de tipo array
                                if (item.Key.SubpositionItems != null && typeof(IList).IsAssignableFrom(item.Value.PropertyType))
                                {
                                    IList propValueList = (IList)Activator.CreateInstance(item.Value.PropertyType);

                                    for (int j = 1; j < ur.Record.Dcount(i) + 1; j++)
                                    {
                                        //Si existe en el array de subposition
                                        if (item.Key.SubpositionItems.Any(si => si == j))
                                            propValueList.Add(ConverterManager.Convert(ur.Record.Extract(i, j).ToString(), item.Value.PropertyType.GenericTypeArguments[0]));
                                    }

                                    //Asigno vector
                                    item.Value.SetValue(value, propValueList);
                                }
                                else
                                {
                                    for (int j = 1; j < ur.Record.Dcount(i) + 1; j++)
                                    {
                                        if (item.Key.Subposition == j)
                                            item.Value.SetValue(value,
                                                ConverterManager.Convert(ur.Record.Extract(i, j).ToString(), item.Value.PropertyType));
                                    }
                                }
                            }
                            else
                            {
                                if (item.Key.SubpositionItems != null && typeof(IList).IsAssignableFrom(item.Value.PropertyType))
                                {
                                    IList propValueList = (IList)Activator.CreateInstance(item.Value.PropertyType);

                                    for (int j = 1; j < ur.Record.Dcount(i) + 1; j++)
                                    {
                                        //Si existe en el array de subposition
                                        if (item.Key.SubpositionItems.Any(si => si == j))
                                            propValueList.Add(ConverterManager.Convert(ur.Record.Extract(i, j).ToString(), item.Value.PropertyType.GenericTypeArguments[0]));
                                    }

                                    //Asigno vector
                                    item.Value.SetValue(value, propValueList);
                                }
                                else
                                    item.Value.SetValue(value,
                                    ConverterManager.Convert(ur.Record.Extract(i).ToString(), item.Value.PropertyType));
                            }
                        }
                    }
                }
            }

            return retValue;
        }


        #endregion
    }
}
