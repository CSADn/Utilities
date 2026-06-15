using Helpers;
using IBMU2.UODOTNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uniVerseSise.Entidades;

namespace uniVerseSise.DBManager
{
    public class ConexionUV : IDisposable
    {
        #region Propiedades Publicas

        /// <summary>
        /// Devuelve el caracter delimitador de registros de Universe
        /// </summary>
        public static string DelimitadorRegistroUV
        {
            get { return "þ"; }
        }

        /// <summary>
        /// Devuelve el caracter delimitador de multivalores de un atributo de Universe
        /// </summary>
        public static string DelimitadorMultivalorUV
        {
            get { return "ý"; }
        }


        /// <summary>
        /// Devuelve el caracter delimitador de subvalores de un atributo de Universe
        /// </summary>
        public static string DelimitadorSubvalorUV
        {
            get { return "ü"; }
        }

        /// <summary>
        /// Retorna la ultima conexión creada
        /// </summary>
        public static ConexionUV Current
        {
            get
            {
                return current;
            }
        }

        public bool IsConnected
        {
            get
            {
                return (Conexion != null && Conexion.IsActive);
            }
        }

        #endregion

        #region Propiedades privadas

        private static ConexionUV current = null;

        private static string Usuario
        {
            get
            {
                return "db.uv.user".FromAppSettings<string>(notFoundException: true);
            }
        }

        private static string Password
        {
            get
            {
                return "db.uv.password".FromAppSettings<string>(notFoundException: true);
            }
        }

        private static string IpServidor
        {
            get
            {

                return "db.uv.server".FromAppSettings<string>(notFoundException: true);
            }
        }

        private static string Cuenta
        {
            get
            {
                return "db.uv.account".FromAppSettings<string>(notFoundException: true);
            }
        }

        private static string Servicio
        {
            get
            {
                return "db.uv.services".FromAppSettings<string>("uvcs");
            }
        }

        private volatile IBMU2.UODOTNET.UniSession Conexion = null;

        public string ErrorMenssage { get; private set; }

        #endregion

        #region Conectar y Desconectar

        /// <summary>
        /// Establece una conexion a Universe 
        /// </summary>
        /// <param name="uSn"></param>
        /// <param name="exceptionMessage"></param>
        /// <param name="uSn"></param>
        /// <param name="ipServidor"></param>
        /// <param name="usuario"></param>
        /// <param name="password"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        private bool ConectarUV(string ipServidor, string usuario, string password, string cuenta, bool throwException = false)
        {
            ErrorMenssage = "";
            Conexion = null;
            try
            {
                Conexion = UniObjects.OpenSession(ipServidor, usuario, password, cuenta, Servicio);
                return Conexion.IsActive;
            }
            catch (Exception ex)
            {
                ErrorMenssage = ex.Message;
                if (throwException)
                    throw ex;
            }

            return false;
        }

        /// <summary>
        /// Establece una conexion a Universe a partir de los datos pasados en los parámetros.
        /// </summary>
        /// <param name="ipServidor"></param>
        /// <param name="usuario"></param>
        /// <param name="password"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public bool Conectar(string ipServidor, string usuario, string password, string cuenta)
        {
            Conexion = null;
            try
            {
                //ConectarUV, si no puede hacerlo, reintenta 6 veces cada 9 segundos.
                for (int intento = 0; intento < 20; intento++)
                {
                    //if (!ConectarUV(out uSn, out exceptionMessage, ipServidor, usuario, password, cuenta) && (ReintentarError(exceptionMessage)))
                    if (!ConectarUV(ipServidor, usuario, password, cuenta))
                    {
                        //Pausa 5 segundos para reintento
                        System.Threading.Thread.Sleep(500);
                    }
                    else
                    {
                        break;
                    }
                }

                if (Conexion == null)
                    throw new ApplicationException("No se pudo establecer una conexión: " + ErrorMenssage);

                return Conexion.IsActive;
            }
            catch
            {
                Conexion = null;
                throw;
            }
        }

        /// <summary>
        /// Verifica si a partir del error informado, debe reintentarse establecer la conexion
        /// </summary>
        /// <param name="exceptionMessage"></param>
        /// <returns></returns>
        private static bool ReintentarError(string exceptionMessage)
        {
            return exceptionMessage.Contains("ErrorCode=39134");
        }

        /// <summary>
        /// Establece una conexion a Universe con la cuenta predeterminada para el ambiente de ejecución establecido.
        /// </summary>
        /// <returns></returns>
        public bool Conectar()
        {
            try
            {
                Conectar(IpServidor, Usuario, Password, Cuenta);
                return Conexion.IsActive;
            }
            catch
            {
                Conexion = null;
                throw;
            }
        }

        /// <summary>
        /// Cierra la conexión si ésta se encuentra abierta.
        /// </summary>
        /// <returns></returns>
        public void Desconectar()
        {
            try
            {
                if (Conexion != null && Conexion.IsActive)
                {
                    UniObjects.CloseSession(Conexion);
                }
            }
            finally
            {
                Conexion = null;
            }
        }

        /// <summary>
        /// Desconexion
        /// </summary>
        public void Dispose()
        {
            Desconectar();
        }

        #endregion

        #region AbrirArchivoUv

        /// <summary>
        /// Abre un archivo de SISE contra una conexión determinada.
        /// </summary>
        /// <param name="nombreArchivo"></param>
        /// <param name="archivo"></param>
        public bool AbrirArchivoUv(out UniFile uFe, string nombreArchivo)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            try
            {
                uFe = Conexion.CreateUniFile(nombreArchivo);
                return true;
            }
            catch
            {
                uFe = null;
                return false;
            }
        }

        #endregion

        #region BorrarArchivoUV

        public bool BorrarArchivo(string nombreArchivo)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            try
            {
                UniCommand cmd = Conexion.CreateUniCommand();
                //Ejecutar comando
                cmd = Conexion.CreateUniCommand();
                cmd.Command = "CLEAR-FILE " + nombreArchivo;
                cmd.Execute();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion BorrarArchivoUV

        #region EjecutarComando

        /// <summary>
        /// Ejecuta un comando en Universe y lo devuelve
        /// </summary>
        /// <param name="cmd">Comando devuelto</param>
        /// <param name="cantRegistros">Cantidad de registros afectados</param>
        /// <param name="conexion">conexión activa sobre la cual se ejecuta el comando</param>
        /// <param name="consulta">texto del comando a ser ejecutado</param>
        /// <returns>Verdadero si se ejecutó correctamente el comando</returns>
        public bool EjecutarComando(out UniCommand cmd, out long cantRegistros, string consulta)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            cantRegistros = 0;
            try
            {
                //Ejecutar comando
                cmd = Conexion.CreateUniCommand();
                cmd.Command = consulta; //
                cmd.Execute();
                //Obtener y convertir respuesta a long
                if (cmd.Response.IndexOf(" ") > -1)
                {
                    return (Int64.TryParse(cmd.Response.Remove(cmd.Response.IndexOf(" ")), out cantRegistros));
                }
                else
                {
                    throw new Exception("La respuesta no puede ser parseada: " + cmd.Response);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una consulta
        /// </summary>
        /// <param name="consulta"></param>
        /// <returns></returns>
        public string EjecutarComando(string consulta)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");
            try
            {
                //Ejecutar comando
                UniCommand cmd = Conexion.CreateUniCommand();
                cmd.Command = consulta; //
                cmd.Execute();

                if (!string.IsNullOrEmpty(cmd.Response))
                    return cmd.Response;

                UniSelectList lista = Conexion.CreateUniSelectList(0);

                StringBuilder str = new StringBuilder();
                while (!lista.LastRecordRead)
                {
                    str.AppendLine(lista.Next());
                }

                return str.ToString();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una consulta
        /// </summary>
        /// <param name="consulta"></param>
        /// <returns></returns>
        public long EjecutarComando(List<string> consultas)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            List<string> retValue = new List<string>();
            long cantRegistros = 0;
            try
            {
                UniCommand cmd = Conexion.CreateUniCommand();
                //Ejecutar comando
                foreach (string consulta in consultas)
                {
                    cmd.Command = consulta; //
                    cmd.Execute();
                    retValue.Add(cmd.Response);
                }


                //Obtener y convertir respuesta a long
                if (retValue?.Last().IndexOf(" ") > -1)
                    Int64.TryParse(cmd.Response.Remove(cmd.Response.IndexOf(" ")), out cantRegistros);

                return cantRegistros;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una consulta
        /// </summary>
        /// <param name="consulta"></param>
        /// <returns></returns>
        public List<string> EjecutarComandoBulk(List<string> consultas)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            List<string> retValue = new List<string>();
            try
            {
                UniCommand cmd = Conexion.CreateUniCommand();
                //Ejecutar comando
                foreach (string consulta in consultas)
                {
                    cmd.Command = consulta; //
                    cmd.Execute();
                    retValue.Add(cmd.Response);
                }

                return retValue;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        ///  Ejecuta un comando de SISE contra una conexion y devuelve solamente el nro de registros afectados.
        /// </summary>
        /// <param name="cantRegistros"></param>
        /// <param name="conexion"></param>
        /// <param name="consulta"></param>
        /// <returns></returns>
        public bool EjecutarComando(out long cantRegistros, string consulta)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            cantRegistros = 0;
            try
            {
                //Ejecutar comando
                UniCommand cmd = Conexion.CreateUniCommand();
                return (EjecutarComando(out cmd, out cantRegistros, consulta));
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region EjecutarLista IDs

        /// <summary>
        /// Ejecuta un comando de SISE contra una conexión y devuelve una lista de IDs de los registros afectados.
        /// </summary>
        /// <param name="cantRegistros">Cantidad de registros de la lista</param>
        /// <param name="lista">Lista devuelta</param>
        /// <param name="conexion"></param>
        /// <param name="consulta">Query de selección a ser ejecutado</param>
        /// <returns></returns>
        public bool EjecutarLista(out UniSelectList lista, out long cantRegistros, string consulta)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            cantRegistros = 0;
            lista = null;
            try
            {
                UniCommand cmd = Conexion.CreateUniCommand();

                if (EjecutarComando(out cmd, out cantRegistros, consulta))
                {
                    lista = Conexion.CreateUniSelectList(0);
                    return true;
                }
                return false;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Ejecuta un comando de SISE contra una conexión y devuelve una lista de IDs de los registros afectados.
        /// </summary>
        /// <returns></returns>
        public UniSelectListResults EjecutarLista(List<string> commandos)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            UniSelectListResults retValue = new UniSelectListResults();

            try
            {
                UniCommand cmd = Conexion.CreateUniCommand();
                retValue.CantidadRegistros = EjecutarComando(commandos);

                if (retValue.CantidadRegistros > 0)
                {
                    retValue.SelectList = Conexion.CreateUniSelectList(0);
                    retValue.Ejecutado = true;
                }
            }
            catch
            {
                throw;
            }
        
            return retValue;
        }
        
        #endregion

        #region EjecutarDataset

        public UniDataSet EjecutarDataset(string consultaArchivoUv, int count = -1)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            UniDataSet uSet = null;

            long cantRegistros;
            //Tomar nombre archivo de la consulta "SELECT NombreArchivo.."
            if (consultaArchivoUv.IndexOf("SELECT") > -1)
            {
                string[] consultaSelect = consultaArchivoUv.Split(" ".ToCharArray());
                string nombreArchivoUV = consultaSelect[1];

                if (!EjecutarDataset(out uSet, out cantRegistros, nombreArchivoUV, consultaArchivoUv, count))
                    uSet = null;
            }
            else
            {
                throw new ApplicationException("Este método solo soporta consultas del tipo SELECT o sus derivados.");
            }

            return uSet;
        }

        public UniDataSet EjecutarDataset(string consulta, string archivo, int count = -1)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            UniDataSet uSet = null;

            long cantRegistros;

            if (!EjecutarDataset(out uSet, out cantRegistros, consulta, archivo, count))
                uSet = null;

            return uSet;
        }

        /// <summary>
        /// Ejecuta un comando de Universe contra una conexión y devuelve una lista con registros completos afectados a un archivo especificado.
        /// </summary>
        /// <param name="uSet">DataSet devuelto</param>
        /// <param name="cantRegistros">Cantidad de registros del DataSet</param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="consultaArchivoUv">Query a ser ejecutado</param>
        /// <returns></returns>
        public UniDataSetResult EjecutarDataSetBulk(string consulta, string nombreArchivoUV)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            UniDataSetResult retValue = new UniDataSetResult();

            retValue.CantidadRegistros = 0;
            retValue.DataSet = null;

            try
            {
                //Obtener lista desde la consulta
                UniFile uFe;

                //Si la consulta tiene muchos elementos
                string query = consulta;

                string[] queries = query.Split('\n');
                List<string> commandos = new List<string>();

                if (queries.Length > 0)
                {
                    foreach (var q in queries)
                    {
                        if (!string.IsNullOrWhiteSpace(q))
                            commandos.Add(q.Replace("\r", string.Empty));
                    }
                }

                UniSelectListResults uniListResults = EjecutarLista(commandos);
                if (uniListResults.Ejecutado)
                {
                    retValue.CantidadRegistros = uniListResults.CantidadRegistros;

                    //Abrir archivo asociado a la consulta
                    if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                    {
                        if (retValue.CantidadRegistros > 0)
                        {
                            //Cargar array desde la lista de IDs
                            List<string> sArray = new List<string>();

                            while (!uniListResults.SelectList.LastRecordRead)
                                sArray.Add(uniListResults.SelectList.Next());

                            retValue.DataSet = uFe.ReadRecords(sArray.ToArray());
                            retValue.Ejecutado = true;
                        }
                    }
                }
            }
            catch
            {
                retValue.DataSet = null;
                retValue.Ejecutado = false;
            }

            return retValue;
        }

        /// <summary>
        /// Ejecuta un comando de Universe contra una conexión y devuelve una lista con registros completos afectados a un archivo especificado.
        /// </summary>
        /// <param name="uSet">DataSet devuelto</param>
        /// <param name="cantRegistros">Cantidad de registros del DataSet</param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="consultaArchivoUv">Query a ser ejecutado</param>
        /// <returns></returns>
        private bool EjecutarDataset(out UniDataSet uSet, out long cantRegistros, string nombreArchivoUV, string consultaArchivoUv, int count = -1)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            cantRegistros = 0;
            uSet = null;
            try
            {
                //Obtener lista desde la consulta
                UniSelectList lista;
                UniFile uFe;

                if (EjecutarLista(out lista, out cantRegistros, consultaArchivoUv))
                {
                    //Abrir archivo asociado a la consulta
                    if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                    {
                        if (cantRegistros > 0)
                        {
                            if (count > 0)
                            {
                                int countReader = 0;
                                //Cargar array desde la lista de IDs
                                List<string> sArray = new List<string>();

                                while (countReader < count && !lista.LastRecordRead)
                                {
                                    sArray.Add(lista.Next());
                                    countReader++;
                                }

                                //string[] sArray = lista.ReadList().ToString().Split(DelimitadorRegistroUV.ToCharArray());

                                //if (count > 0)
                                //    sArray = sArray.Take(count).ToArray();

                                //Cargar registros al UniDataset desde lista de IDs en el array de string.
                                uSet = uFe.ReadRecords(sArray.ToArray());
                            }
                            else
                            {
                                //Cargar array desde la lista de IDs
                                string[] sArray = lista.ReadList().ToString().Split(DelimitadorRegistroUV.ToCharArray());

                                if (count > 0)
                                    sArray = sArray.Take(count).ToArray();

                                //Cargar registros al UniDataset desde lista de IDs en el array de string.
                                uSet = uFe.ReadRecords(sArray);
                            }
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                uSet = null;
                return false;
            }
        }

        /// <summary>
        /// Ejecuta un comando de Universe contra una conexión y devuelve una lista con registros completos afectados a un archivo especificado.
        /// </summary>
        /// <param name="dt">DataTable a ser devuelta</param>
        /// <param name="cantRegistros">Cantidad de registros del DataTable</param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="consultaArchivoUv">Query a ser ejecutado</param>
        /// <returns></returns>
        public bool EjecutarDataset(out System.Data.DataTable dt, out long cantRegistros, string nombreArchivoUV, string consultaArchivoUv)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            dt = null;
            cantRegistros = 0;
            try
            {
                // Cargar registros en el UniDataset
                UniDataSet uSet;
                if (EjecutarDataset(out uSet, out cantRegistros, nombreArchivoUV, consultaArchivoUv))
                {
                    dt = UniDataSetToDataTable(uSet);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private System.Data.DataTable UniDataSetToDataTable(UniDataSet uSet)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            bool columnasYaAgregadas = false;
            //Recorrer UniDataSet y agregar filas al DataTable
            foreach (UniRecord item in uSet)
            {
                //Mapear la fila desde el UniRecord a la matriz de objetos
                object[] oCols = item.Record.ToString().Split(DelimitadorRegistroUV.ToCharArray());

                //Agregar columnas a partir de la cantidad de atributos del primer registro
                if (!columnasYaAgregadas)
                {
                    for (int ic = 0; ic <= item.Record.Dcount(); ic++)
                    {
                        dt.Columns.Add(ic.ToString());
                    }
                    columnasYaAgregadas = true;
                }
                //Cargar la fila a la tabla
                dt.Rows.Add(oCols);
            }
            return dt;
        }

        #endregion

        #region LeerRegistro
        /// <summary>
        /// Lee un registro de un archivo Universe. No lo bloquea.
        /// </summary>
        /// <param name="uDy">Registro leido</param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="id"></param>
        /// <returns>True si pudo ser leido el registro</returns>
        public bool LeerRegistro(out UniDynArray uDy, string nombreArchivoUV, string id)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            return LeerRegistro(out uDy, nombreArchivoUV, id, false);
        }

        /// <summary>
        /// Lee un registro de un archivo Universe
        /// </summary>
        /// <param name="uDy">Registro leido</param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="id"></param>
        /// <param name="bloquearRegistro">Verdadero bloquea el registro</param>
        /// <returns>True si pudo ser leido y bloqueado el registro</returns>
        public bool LeerRegistro(out UniDynArray uDy, string nombreArchivoUV, string id, bool bloquearRegistro)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            uDy = null; bool ret = false;
            try
            {
                UniFile uFe;
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    uDy = uFe.Read(id);

                    if (bloquearRegistro)
                        uFe.LockRecord(1);

                    ret = true;
                }
                return ret;
            }
            catch
            {
                return ret;
            }
        }


        /// <summary>
        /// Lee un registro de un archivo Universe (sin bloquearlo)
        /// </summary>
        /// <param name="uRd">Registro leido</param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="id"></param>
        /// <returns>True si pudo ser leido el registro</returns>
        public bool LeerRegistro(out UniRecord uRd, string nombreArchivoUV, string id)
        {
            return LeerRegistro(out uRd, nombreArchivoUV, id, false);
        }

        /// <summary>
        /// Lee y bloquea un registro de un archivo Universe
        /// </summary>
        /// <param name="uRd">Registro leido</param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="id"></param>
        /// <param name="bloquearRegistro">True si se desea bloquear el registro (se debe utilizar DesbloquearRegistro())</param>
        /// <returns>True si pudo ser leido el registro</returns>
        public bool LeerRegistro(out UniRecord uRd, string nombreArchivoUV, string id, bool bloquearRegistro)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            uRd = new UniRecord();
            try
            {
                UniFile uFe;
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    if (!String.IsNullOrEmpty(id) && id != "0")
                    {
                        uFe.Read(id);
                        uRd.RecordID = id;
                        uRd.Record = uFe.Record;

                        if (bloquearRegistro)
                            uFe.LockRecord(1);
                        else
                            uFe.LockRecord(0);

                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }


            //uRd = new UniRecord();
            //try
            //{
            //    UniFile uFe;
            //    if (AbrirArchivoUv(out uFe, conexion, nombreArchivoUV))
            //    {
            //        uFe.UniFileLockStrategy = UniObjectsTokens.LOCK_MY_READU;
            //        uFe.Read(id);
            //        uRd.Record = uFe.Record;
            //        uRd.RecordID = id;
            //        uFe.UniFileLockStrategy = UniObjectsTokens.LOCK_NO_LOCK;
            //        return true;
            //    }
            //    return false;
            //}
            //catch 
            //{
            //    return false;
            //}
        }

        public bool LeerRegistro(out UniRecord uRd, UniFile uFe, string id)
        {
            return LeerRegistro(out uRd, uFe, id, false);
        }

        public bool LeerRegistro(out UniRecord uRd, UniFile uFe, string id, bool bloquearRegistro)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            uRd = new UniRecord();
            bool lecturaOk = false;
            if (uFe != null)
            {
                uFe.Read(id);
                uRd.RecordID = id;
                uRd.Record = uFe.Record;
                if (bloquearRegistro)
                    uFe.LockRecord(1);
                else
                    uFe.LockRecord(0);
            }

            return lecturaOk;
        }

        /// <summary>
        /// Agrega un atributo en la posicion indicada desplazando hacia abajo al que este actualmente
        /// </summary>
        /// <param name="archivoUV"></param>
        /// <param name="idRegistro"></param>
        /// <param name="atributo"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        public bool AgregarAtributoUv(string nombreArchivoUv, string idRegistro, int atributo, string valor)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            UniFile uFe;
            try
            {
                if (AbrirArchivoUv(out uFe, nombreArchivoUv))
                {
                    uFe.Read(idRegistro);
                    uFe.Record.Insert(atributo, valor);
                    uFe.Write();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool DesBloquearRegistro(string nombreArchivoUV, string id)
        {
            if (!IsConnected)
                throw new ApplicationException("No conectado");

            try
            {
                UniFile uFe;
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    uFe.UnlockRecord(id);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region DatoUV

        /// <summary>
        /// Lee el valor de un atributo de un archivo de Universe.
        /// </summary>
        /// <param name="nombreArchivoUV">Nombre del archivo sobre la que se realiza la lectura.</param>
        /// <param name="id">Id del registro a leer.</param>
        /// <param name="atributo">Atributo a leer.</param>
        /// <param name="valor">Multivalor a leer.</param>
        /// <param name="subValor">Subvalor del multivalor a leer.</param>
        /// <returns></returns>
        public bool DatoUV(out string ret, string nombreArchivoUV, string id, int atributo, int valor, int subValor)
        {
            ret = "";
            try
            {
                UniFile uFe;
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    UniDynArray ud = uFe.Read(id);
                    ret = ud.Extract(atributo, valor, subValor).ToString();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lee el valor de un atributo de un archivo de Universe.
        /// </summary>
        /// <param name="nombreArchivoUV">Nombre del archivo sobre la que se realiza la lectura.</param>
        /// <param name="id">Id del registro a leer.</param>
        /// <param name="atributo">Atributo a leer.</param>
        /// <param name="valor">Multivalor a leer.</param>
        /// <returns></returns>
        public bool DatoUV(out string ret, string nombreArchivoUV, string id, int atributo, int valor)
        {
            ret = "";
            try
            {
                UniFile uFe;
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    UniDynArray ud = uFe.Read(id);
                    ret = ud.Extract(atributo, valor).ToString();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lee el valor de un atributo de un archivo de Universe.
        /// </summary>
        /// <param name="nombreArchivoUV">Nombre del archivo sobre la que se realiza la lectura.</param>
        /// <param name="id">Id del registro a leer.</param>
        /// <param name="atributo">Atributo a leer.</param>
        /// <returns></returns>
        public bool DatoUV(out string ret, string nombreArchivoUV, string id, int atributo)
        {
            ret = "";
            try
            {
                UniFile uFe;
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    UniDynArray ud = uFe.Read(id);
                    ret = ud.Extract(atributo).ToString();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Lee un subvalor de un registro de un archivo de Universe.
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="id"></param>
        /// <param name="atributo"></param>
        /// <param name="valor"></param>
        /// <param name="subValor"></param>
        /// <returns></returns>
        public bool DatoUV(out int ret, string nombreArchivoUV, string id, int atributo, int valor, int subValor)
        {
            string retAux = "";
            ret = 0;
            if (DatoUV(out retAux, nombreArchivoUV, id, atributo, valor, subValor))
            {
                return (int.TryParse(retAux, out ret));
            }
            else
            {
                return false;
            }
            ;
        }

        /// <summary>
        /// Lee un valor de un registro de un archivo de Universe.
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="id"></param>
        /// <param name="atributo"></param>
        /// <param name="valor"></param>
        /// <param name="subValor"></param>
        /// <returns></returns>
        public bool DatoUV(out int ret, string nombreArchivoUV, string id, int atributo, int valor)
        {
            string retAux = "";
            ret = 0;
            if (DatoUV(out retAux, nombreArchivoUV, id, atributo, valor))
            {
                return (int.TryParse(retAux, out ret));
            }
            else
            {
                return false;
            }
            ;
        }

        /// <summary>
        /// Lee un atributo de un registro de un archivo de Universe.
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="id"></param>
        /// <param name="atributo"></param>
        /// <param name="valor"></param>
        /// <param name="subValor"></param>
        /// <returns></returns>
        public bool DatoUV(out int ret, string nombreArchivoUV, string id, int atributo)
        {
            string retAux = "";
            ret = 0;
            if (DatoUV(out retAux, nombreArchivoUV, id, atributo))
            {
                return (int.TryParse(retAux, out ret));
            }
            else
            {
                return false;
            }
            ;
        }

        /// <summary>
        /// Lee un subvalor de un registro de un archivo de Universe.
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="id"></param>
        /// <param name="atributo"></param>
        /// <param name="valor"></param>
        /// <param name="subValor"></param>
        /// <returns></returns>
        public bool DatoUV(out decimal ret, string nombreArchivoUV, string id, int atributo, int valor, int subValor)
        {
            string retAux = "";
            ret = 0;
            if (DatoUV(out retAux, nombreArchivoUV, id, atributo, valor, subValor))
            {
                return (decimal.TryParse(retAux, out ret));
            }
            else
            {
                return false;
            }
            ;
        }


        #endregion

        #region Métodos de Escritura


        /// <summary>
        /// Elimina un registro de un archivo de Universe.
        /// </summary>
        /// <param name="conexion">Conexión sobre la que se realiza la eliminación.</param>
        /// <param name="nombreArchivoUV">Nombre del archivo sobre el que se realiza la eliminación.</param>
        /// <param name="id">Id del registro a eliminar.</param>
        /// <returns>Verdadero si pudo eliminar el registro.</returns>
        public bool EliminarRegistroUv(string nombreArchivoUV, string id)
        {
            try
            {
                //Abrir archivo
                UniFile uFe;
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    //Eliminar registro
                    uFe.DeleteRecord(id);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// Modifica el valor de un atributo, de un registro de un archivo de Universe.
        /// </summary>
        /// <param name="conexion">Conexión sobre la que se realiza la modificación</param>
        /// <param name="nombreArchivoUV">Nombre del archivo sobre el que se realiza la modificación.</param>
        /// <param name="idRegistro">Id del registro a eliminar</param>
        /// <param name="atributo">Nro. de atributo sobre el que se realiza la modificación.</param>
        /// <param name="valor">Nuevo valor que se escribe en el atributo del registro</param>
        /// <returns>Verdadero si pudo modificar el valor del atributo del registro</returns>
        /// <remarks>Este método bloquea el registro mientras realiza la modificación</remarks>
        public bool ModificarAtributoUv(string nombreArchivoUV, string idRegistro, int atributo, string valor)
        {
            bool ret = false;
            UniFile uFe = null;
            try
            {
                //Abrir archivo
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    //uFe.RecordID = idRegistro;
                    uFe.Read(idRegistro);
                    //Bloquear record
                    //uFe.LockRecord(idRegistro, 1);   
                    //Modificar atributo                    
                    uFe.WriteField(atributo, valor);
                    ret = true;
                }
                return ret;
            }
            finally
            {
                //Cerrar archivo
                uFe.Close();
            }
        }

        /// <summary>
        /// Escribe un registro previamente modificado
        /// </summary>
        /// <param name="conexion"></param>
        /// <param name="nombreArchivoUV"></param>
        /// <param name="idRegistro"></param>
        /// <param name="registroModificado"></param>
        /// <returns></returns>
        public bool ModificarRegistroUv(string nombreArchivoUV, string idRegistro, UniDynArray registroModificado)
        {
            bool ret = false;
            UniFile uFe = null;
            try
            {
                //Abrir archivo
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    //uFe.RecordID = idRegistro;
                    uFe.Read(idRegistro);
                    //Bloquear record
                    //uFe.LockRecord(idRegistro, 1);   
                    //Modificar atributo                    
                    uFe.Write(idRegistro, registroModificado);
                    ret = true;
                }
                return ret;
            }
            finally
            {
                //Cerrar archivo
                uFe.Close();
            }
        }

        /// <summary>
        /// Recibe un archivo UV previamente abierto y modifica el campo especificado
        /// </summary>
        /// <param name="conexion"></param>
        /// <param name="archivoUV"></param>
        /// <param name="idRegistro"></param>
        /// <param name="atributo"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        public bool ModificarAtributoUv(UniFile archivoUV, string idRegistro, int atributo, string valor)
        {
            bool modificadoOK = false;
            //archivoUV.RecordID = idRegistro;
            //Bloquear record
            //archivoUV.LockRecord(idRegistro, 1);
            //Modificar atributo                    
            try
            {
                archivoUV.WriteField(atributo, valor);
                modificadoOK = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return modificadoOK;
        }

        /// <summary>
        /// Agrega un valor en la posicion indicada de un atributo indicado
        /// </summary>
        /// <param name="sesion"></param>
        /// <param name="nombreArchivoUv"></param>
        /// <param name="idRegistro"></param>
        /// <param name="atributo"></param>
        /// <param name="indice"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        public bool AgregarSubValorUv(string nombreArchivoUv, string idRegistro, int atributo, int indiceAtributo, string valor)
        {
            UniFile uFe;
            try
            {
                if (AbrirArchivoUv(out uFe, nombreArchivoUv))
                {
                    uFe.Read(idRegistro);
                    uFe.Record.Insert(atributo, indiceAtributo, valor);
                    uFe.Write();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Elimina un lote de registros de un archivo de Universe.
        /// </summary>
        /// <param name="nombreArchivoUV">Archivo sobre la que se realiza la eliminación.</param>
        /// <param name="uSet">UniDataset con el lote de registros a eliminar de un mismo archivo.</param>
        /// <returns>Verdadero si pudo eliminar el lote.</returns>
        public bool EliminarRegistroUv(string nombreArchivoUV, UniDataSet uSet)
        {
            try
            {
                //Abrir archivo
                UniFile uFe;
                if (AbrirArchivoUv(out uFe, nombreArchivoUV))
                {
                    //Eliminar un lote de registros de un mismo archivo desde un UniDataset.
                    uFe.DeleteRecord(uSet);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Escribe un registro a un archivo de Universe, que abre sobre la sesion informada
        /// </summary>
        /// <param name="sesion"></param>
        /// <param name="nombreArchivoUv"></param>
        /// <param name="id"></param>
        /// <returns>Verdadero si pudo escribirse el registro</returns>
        public bool EscribirRegistroUv(UniDynArray registro, string nombreArchivoUv, string id)
        {
            try
            {
                UniFile uFe;
                if (AbrirArchivoUv(out uFe, nombreArchivoUv))
                {
                    uFe.Write(id, registro);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region transacciones

        public UniTransaction IniciarTransaccion()
        {
            UniTransaction ut = Conexion.CreateUniTransaction();
            ut.Begin();
            if (ut.IsActive())
                return ut;
            else
            {
                return null;
            }
        }

        #endregion

        #region Metodos de Acceso

        /// <summary>
        /// Crea una nueva conexion, destruyendo la anterior
        /// </summary>
        public static ConexionUV CrearConexion()
        {
            if (current != null && current.IsConnected)
                current.Desconectar();

            current = new ConexionUV();

            //Intento conectar 
            if (!current.Conectar())
                throw new ApplicationException("Error al conectar al servidor: " + current.ErrorMenssage);
            return current;
        }

        #endregion

    }
}
