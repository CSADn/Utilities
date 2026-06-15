using System;
using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Helpers;
using DatabaseModel;

namespace historico
{
    public class TablaHistorico_MigrarDatos
    {
        private static TablaHistorico_MigrarDatos _instance;

        public List<Entities.ParametroHistoricoCabecera> _listaTablasCabeceras = new List<Entities.ParametroHistoricoCabecera>();

        public static TablaHistorico_MigrarDatos Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TablaHistorico_MigrarDatos();
                return _instance;
            }
        }

        public TablaHistorico_MigrarDatos()
        {
            //
        }

        #region proceso principal

        public void iniciarProcesoTablaBaseToHistorico(string nombreTablaCabecera, List<string> listaTablasAProcesar, List<Entities.TablaTemporal> listaTablasHerencia)
        {
            try
            {
                // OBJETIVO: Buscar en tabla PARAMETROS_HISTORICO_CABECERA: idCabecera, nombreTablaCabecera, nombreClaveEdad, rangoClaveEdad

                // sSql: query execute
                string sSql = string.Empty;
                //Nombre de tabla base(cabecera) e historico
                string nombreTablaCabeceraHistorica = string.Empty;
                //Columnas que se enviaran a INSERT INTO
                string columnasForInsertIntoCabecera = string.Empty;
                //Variables para armar SELECT de los registros a insertar
                string columnasForSelectIntoCabecera = string.Empty;
                string sSql_whereTablaCabecera = string.Empty;
                string sSql_JoinTablaTemporal = string.Empty;
                string insertTablaIdentificadoraIdCabecera = string.Empty;
                string nombreColumnaPK = string.Empty;
                string sSql_InserTablaTemporal = string.Empty;
                // Obtener estructura tabla Cabecera
                var itemTablaCabecera = Crud.ParametroHistoricoCabecera.Instance.GetAllParametroHistoricoCabeceraDeUnaTabla(nombreTablaCabecera);
                //Obtener el where de la cabecera
                sSql_whereTablaCabecera = armarWhereFormulaGeneracionHistoricoCabecera(itemTablaCabecera);
                
                //Nombre de las tablas
                nombreTablaCabecera = itemTablaCabecera.nombreTablaCabecera;
                nombreTablaCabeceraHistorica = Crud.EstructuraTabla.nombreTablaHistorico(itemTablaCabecera.nombreTablaCabecera);
                
                //Obtener nombre de la columna primary key de la tabla cabecera
                nombreColumnaPK = Crud.EstructuraTabla.Instance.GetNaneColumnPKByTabla(itemTablaCabecera.nombreTablaCabecera);
                //Crear tabla temporal del nodo raiz/padre principal
                string sSql_TablaTemporal = script_CreacionTablaTemporal(nombreColumnaPK, nombreTablaCabecera,nombreTablaCabecera, sSql_whereTablaCabecera);
                
                //Crear las tablas temporales filtros de los hijos que son padres 
                var listaHijosQueSonPadres = listaTablasHerencia.Where(x => x.NombreTabla != nombreTablaCabecera && x.bCrearTemporal).ToList();
                foreach (var item in listaHijosQueSonPadres)
                {
                    string sSql_auxTablaTemporal = string.Empty;
                    string sSql_auxWhere = string.Empty;

                    sSql_auxWhere = "EXISTS (SELECT 1 FROM #TEMP_" + item.NombreTablaFiltro +" b " + " WHERE a." + item.ClaveFiltro + " = " + "b." + item.ClaveFiltro + ")";
                    sSql_auxTablaTemporal = script_CreacionTablaTemporal(item.Pk,item.NombreTabla,item.NombreTabla, sSql_auxWhere);
                    sSql_TablaTemporal += sSql_auxTablaTemporal;
                }
                //Se procesa la migracion del padre y sus hijos
                migrandoTablasAHistorico(
                                            itemTablaCabecera,                      // TablaCabecera = Tabla Padre Principal
                                            nombreColumnaPK,                        // Columna Pk de Tabla Padre Principal
                                            sSql_TablaTemporal,                     // Tablas temporales de los padres
                                            listaTablasAProcesar,                   // Lista de tablas a procesar en orden segun sus fks
                                            listaTablasHerencia                     // Lista de estructura familiar de las tablas a Procesar
                                            );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void migrandoTablasAHistorico
        (
        Entities.ParametroHistoricoCabecera itemTablaCabecera,              // itemCabecera
        string nombreColumnaPk,                                             // nombre de columna pk
        string sSql_TablaTemporal,                                          // Tabla temporal identifica los ids a migrar de la cabecera
        List<string> listaTablaAProcesar,                                   // lista de tablas a procesar
        List<Entities.TablaTemporal> listaTablaHerencia                     // Lista de tablas Herencia con sus respectiva estructura
        )
        {
            try
            {
                if (string.IsNullOrEmpty(sSql_TablaTemporal))
                    return;
                // Nombre tablas a procesar
                string nombreTablaCabecera = itemTablaCabecera.nombreTablaCabecera;
                string nombreTablaCabeceraHistorico = Crud.EstructuraTabla.nombreTablaHistorico(nombreTablaCabecera);
                
                // Columnas que se utilizaran para el insert into de la tabla historico
                string columnasForInsertIntoCabeceraHisto = Crud.EstructuraTabla.Instance.obtenerColumnasTablaHistoForInsertInto(nombreTablaCabeceraHistorico);
                
                // Columnas que se utilizaran para el select de la tabla base de los campos que se migraran
                string columnasForSelectIntoCabecera = Crud.EstructuraTabla.Instance.obtenerColumnasTablaBaseForSelectInsert(nombreTablaCabecera, nombreTablaCabeceraHistorico);
                
                // Variable que poseera el query de los datos a migrar. 
                string sSqlCabeceraInsert = string.Empty;
                string sSqlCabeceraDelete = string.Empty;
                string sSqlGlobal = string.Empty;

                // Estructura tabla Cabecera
                var tablaCabecera = listaTablaHerencia.Where(x => x.NombreTabla == nombreTablaCabecera).FirstOrDefault();
                string sSqlWhereExistsTablaTemporalCabecera = script_WhereExistWithTablaTemp(tablaCabecera.NombreTabla, tablaCabecera.Pk, tablaCabecera.ClaveFiltro);
                
                // variable exists se usa exists y no inner join porque si se agregan nuevas columnas a la tabla base
                // iran valores por default a los registros ya existentes.
                var itemTablaTemporal = listaTablaHerencia.Where(x => x.NombreTabla == nombreTablaCabecera).FirstOrDefault();
                string sSqlWhereExistsTablaTemporal = script_WhereExistWithTablaTemp(itemTablaTemporal.NombreTabla, itemTablaTemporal.ClaveFiltro, itemTablaTemporal.ClaveFiltro);
                // Migrando registros cabecera 
                sSqlCabeceraInsert = sSql_TablaTemporal;
                sSqlCabeceraInsert += Environment.NewLine +
                                string.Format("INSERT INTO {0} ({1})SELECT {2} FROM {3} b WHERE {4};",
                                    nombreTablaCabeceraHistorico,       // {0}
                                    columnasForInsertIntoCabeceraHisto, // {1}
                                    columnasForSelectIntoCabecera,      // {2}
                                    nombreTablaCabecera,                // {3}
                                    sSqlWhereExistsTablaTemporal);      // {4}
                // Execution query inserta la cabecera en el historico.
                sSqlGlobal = sSqlCabeceraInsert;

                // Realizar migracion datos detalle 
                foreach (var nombreTablaDetalle in listaTablaAProcesar)
                {
                    sSqlWhereExistsTablaTemporal = string.Empty;
                    // sSqlDetalle: arma el query para la migracion y eliminacion de datos
                    string sSqlDetalleInsert = string.Empty;
                    string sSqlDetalleDelete = string.Empty;
                    string nombreTablaDetalleHistorico = Crud.EstructuraTabla.nombreTablaHistorico(nombreTablaDetalle);
                    // Obtener las columnas que se utilizaran para el insert into de la tabla historico
                    string columnasForInsertIntoDetalleHisto = Crud.EstructuraTabla.Instance.obtenerColumnasTablaHistoForInsertInto(nombreTablaDetalleHistorico);
                    // Obtener las columans que se utilizaran para el select de la tabla base de los campos que se migraran
                    string columnasForSelectIntoDetalle = Crud.EstructuraTabla.Instance.obtenerColumnasTablaBaseForSelectInsert(nombreTablaDetalle, nombreTablaDetalleHistorico);
                    /* Modelo del insert: 
                        INSERT INTO TABLA_HISTO(COLUMNS_HISTO) 
                        SELECT COLUMNS_BASE FROM TABLA_BASE 
                        WHERE EXISTS(SELECT 1 FROM TABLA_TEMPORAL)
                    */
                    var itemTablaTemporalDependendiente = listaTablaHerencia.Where(x => x.NombreTabla == nombreTablaDetalle).FirstOrDefault();
                    sSqlWhereExistsTablaTemporal = script_WhereExistWithTablaTemp(itemTablaTemporalDependendiente.NombreTablaFiltro, itemTablaTemporalDependendiente.ClaveFiltro, itemTablaTemporalDependendiente.ClaveFiltro);

                    sSqlDetalleInsert = 
                        Environment.NewLine +
                        string.Format("INSERT INTO {0} ({1})SELECT {2} FROM {3} b WHERE {4};",
                        nombreTablaDetalleHistorico,                // {0}
                        columnasForSelectIntoDetalle,               // {1}
                        columnasForInsertIntoDetalleHisto,          // {2}
                        nombreTablaDetalle,                         // {3}
                        sSqlWhereExistsTablaTemporal);              // {4}
                    sSqlGlobal += sSqlDetalleInsert;

                    sSqlDetalleDelete += 
                        Environment.NewLine +
                        string.Format("DELETE b FROM {0} b WHERE {1};",
                        nombreTablaDetalle,             // {0}
                        sSqlWhereExistsTablaTemporal);  // {1}
                    sSqlGlobal += sSqlDetalleDelete;
                }
                // Realizar depuracion/eliminacion datos cabecera/tabla raiz
                sSqlCabeceraDelete += 
                    Environment.NewLine +
                    string.Format("DELETE b FROM {0} b WHERE {1};", nombreTablaCabecera, sSqlWhereExistsTablaTemporalCabecera);
                sSqlGlobal += sSqlCabeceraDelete;

                //Ejecucion en un query general
                var ok = DataModel.Instance
                    .Transaction(scope =>
                    {
                        DataModel.Instance.
                        ExecuteNonQuery(null,sSqlGlobal,7200);
            },true);
            if (!ok)
                throw new BusinessException(Code.Historico_ErrorMigracion);
        }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void iniciandoMigracionTablaHistoricoToNewHistorico(string nombreTablaHistorico, string nombreTablaHistoricoOld)

        {
            try
            {
                string sSql = string.Empty;
                //--------------------------------------------------------------------------------
                string columnasForInsertInto = Crud.EstructuraTabla.Instance.obtenerColumnasTablaHistoForInsertInto(nombreTablaHistorico);
                //--------------------------------------------------------------------------------
                string columnasForSelectInto = Crud.EstructuraTabla.Instance.obtenerColumnasTablaBaseForSelectInsert(nombreTablaHistorico, nombreTablaHistoricoOld);
                //--------------------------------------------------------------------------------
                sSql = string.Format("INSERT INTO {0} ({1})SELECT {2} FROM {3};", nombreTablaHistorico, columnasForInsertInto, columnasForSelectInto, nombreTablaHistoricoOld);
                //--------------------------------------------------------------------------------
                sSql += "DROP TABLE " + nombreTablaHistoricoOld + " ;";
                //--------------------------------------------------------------------------------
                var ok = DataModel.Instance
                .Transaction(scope =>
                {
                    DataModel.Instance.
                    ExecuteNonQuery(sSql);
                },true);
                if (!ok)
                {
                    Crud.EstructuraTabla.executeRevertirRenombreTablaHistoricoToOld(nombreTablaHistorico, nombreTablaHistoricoOld);
                    throw new BusinessException(Code.Historico_ErrorMigracionTablaHistoricoOld );
                }
                //--------------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // OBJETIVO: Armar el Where de la migracion de datos: asi se procesaran los registros a historicos indicados
        private static string armarWhereFormulaGeneracionHistoricoCabecera(Entities.ParametroHistoricoCabecera itemParametroCabecera)
        {
            string sSql_Where = string.Empty;
            string data_type_Column_NombreClaveEdad = string.Empty;

            // - Obtener lista Generica de DATA_TYPE utilizadas por Sql
            List<Entities.EstructuraTabla> listEstructuraTablaCabecera = Crud.EstructuraTabla.Instance.GetEstructurasTablasByTabla(itemParametroCabecera.nombreTablaCabecera);
            
            // - Obtener el DATA_TYPE de la columna "NombreClaveEdad" para evaluar si es una fecha o un campo numerico identity, que son los dos tipos de datos soportados.
            var itemCabeceraClave = listEstructuraTablaCabecera.Where(x => x.COLUMN_NAME == itemParametroCabecera.nombreClaveEdad).FirstOrDefault();
            if (itemCabeceraClave == null)
                throw new BusinessException(Code.Historico_ErrorConfiguracionTablaHistorico);
            data_type_Column_NombreClaveEdad  = itemCabeceraClave.DATA_TYPE;
            string aux_rangoClaveEdad = itemParametroCabecera.rangoClaveEdad;
            if (string.IsNullOrEmpty(aux_rangoClaveEdad)) //Si no hay valor seteado se setea con el valor por default 30
                aux_rangoClaveEdad = "30";
            // Formula para obtener los registros que se migraran a historico: nombreClaveEdad - rangoClaveEdad
            if (data_type_Column_NombreClaveEdad == "DATETIME"|| data_type_Column_NombreClaveEdad == "DATETIME2")
                sSql_Where = itemParametroCabecera.nombreClaveEdad + " < ( GETDATE() -" + aux_rangoClaveEdad + " ) ";
            else
                sSql_Where = itemParametroCabecera.nombreClaveEdad + " < (itemParametroCabecera.nombreClaveEdad -" + aux_rangoClaveEdad + ")";
            // Se devuelve el resultado WHERE que se utilizara
            return sSql_Where;
        }

        public static List<int> armarListaIndicadoraDatosAMigrar(Entities.ParametroHistoricoCabecera itemParametroCabecera)
        {
            // Lista resultado con los indicadores pk de la tabla Cabecera/Raiz Principal
            List<int> listPkCabeceraAMigrar = new List<int>();
            // String que obtiene la formula identificadora para extraer los datos a migrar.
            string sSql_Where_FormulaIndicExtraccDatosAMigrar = armarWhereFormulaGeneracionHistoricoCabecera(itemParametroCabecera);
            // - Se obtiene una lista con los ids de la cabecera que se migraran a historico
            listPkCabeceraAMigrar = Crud.ParametroHistoricoCabecera.Instance.GetAllIdTablaCabecera(itemParametroCabecera.nombreTablaCabecera, sSql_Where_FormulaIndicExtraccDatosAMigrar);
            // - Se obtiene el DATA_TYPE de la columna "NombreClaveEdad" para evaluar si es una fecha o un campo numerico identity, que son los dos tipos de datos soportados.
            var listaEstructuraTablaCabecera = Crud.EstructuraTabla.Instance.GetEstructurasTablasByTabla(itemParametroCabecera.nombreTablaCabecera);
            var data_Type_Column_NombreClaveEdad = listaEstructuraTablaCabecera.Where(x => x.COLUMN_NAME == itemParametroCabecera.nombreClaveEdad).FirstOrDefault().DATA_TYPE;
            // Formula para obtener los registros que se migraran a historico: nombreClaveEdad - rangoClaveEdad
            if (data_Type_Column_NombreClaveEdad == "DATETIME")
                sSql_Where_FormulaIndicExtraccDatosAMigrar = itemParametroCabecera.nombreClaveEdad + " < ( GETDATE() -" + itemParametroCabecera.rangoClaveEdad + " ) ";
            else
                sSql_Where_FormulaIndicExtraccDatosAMigrar = itemParametroCabecera.nombreClaveEdad + " < (itemParametroCabecera.nombreClaveEdad -" + itemParametroCabecera.rangoClaveEdad + ")";
            // - Armar un query para insertar los registros de la tabla detalle que se identifiquen con los ids de la tabla cabecera/lista
            listPkCabeceraAMigrar = Crud.ParametroHistoricoCabecera.Instance.GetAllIdTablaCabecera(itemParametroCabecera.nombreTablaCabecera, sSql_Where_FormulaIndicExtraccDatosAMigrar);
            return listPkCabeceraAMigrar;
        }

        public static string script_CreacionTablaTemporal
            (
            string nombreColumnaSelectTT,
            string nombreTT,
            string nombreTablaFiltrar,
            string sSqlWhere
            )
        {
            string sSqlTablaTemporal = string.Empty;
            sSqlTablaTemporal = @"
                                SELECT  a.@NOMBRECOLUMNASELECTTT@
                                INTO    #Temp_@NOMBRETT@
                                FROM    @NOMBRETABLAFILTRAR@ a
                                WHERE   @WHERE@;
                                ".Replace("@NOMBRECOLUMNASELECTTT@", nombreColumnaSelectTT)
                                 .Replace("@NOMBRETT@", nombreTT)
                                 .Replace("@NOMBRETABLAFILTRAR@", nombreTablaFiltrar)
                                 .Replace("@WHERE@", sSqlWhere);
            return sSqlTablaTemporal;
        }

        public static List<string> script_List_ArmarInsertTablaTemp(List<int> listIdsRegistroTablaCabeceraAMigrar)
        {
            List<string> listasSqlInsertTablaTemporal = new List<string>();
            string sSqlInsertRecordTablaTemp = string.Empty;
            foreach (var item in listIdsRegistroTablaCabeceraAMigrar)
            {
                sSqlInsertRecordTablaTemp  = Environment.NewLine + "INSERT INTO #Tabla_Temp(Id INT) VALUES (" + item.ToString() + ");";
                listasSqlInsertTablaTemporal.Add(sSqlInsertRecordTablaTemp);
            }
            return listasSqlInsertTablaTemporal;
        }

        public static string script_WhereExistWithTablaTemp(string nombreTablaTemporal, string nombreColumnaPk, string nombreColumnaFiltro)
        {
            string sSql_WhereExistsTablaTemp = string.Empty;
            sSql_WhereExistsTablaTemp = " EXISTS (SELECT 1 FROM #TEMP_@NOMBRETABLA@ a WHERE a." + nombreColumnaPk + " = b." + nombreColumnaFiltro + ")";
            sSql_WhereExistsTablaTemp = sSql_WhereExistsTablaTemp.Replace("@NOMBRETABLA@", nombreTablaTemporal);
            return sSql_WhereExistsTablaTemp;
        }
        #endregion
    }
}
