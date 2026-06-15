using DatabaseModel;
using Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crud
{
    public class EstructuraTabla
    {
        private static EstructuraTabla _instance;

        public static EstructuraTabla Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EstructuraTabla();
                return _instance;
            }
        }

        public EstructuraTabla()
        {
            //
        }

        public List<Entities.EstructuraTabla> GetEstructurasTablasByTabla(string nombreTabla)
        {
            try
            {
                string sSql = string.Empty;
                sSql = @"SELECT
                        c.ORDINAL_POSITION,
                        c.TABLE_NAME,		
                        c.COLUMN_NAME,		
                        c.DATA_TYPE,		
                        CAST(ISNULL(c.CHARACTER_MAXIMUM_LENGTH,0) AS INT) CHARACTER_MAXIMUM_LENGTH,
                        CAST(ISNULL(c.NUMERIC_PRECISION,0)AS INT) NUMERIC_PRECISION,
                        CAST(ISNULL(c.NUMERIC_PRECISION_RADIX,0) AS INT) NUMERIC_PRECISION_RADIX,
                        c.IS_NULLABLE,
                        ISNULL(c.COLUMN_DEFAULT,'') COLUMN_DEFAULT,
                        COALESCE( 
                              ( 
                                  select 'YES'
                                  from INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu
                                  where
                                  cu.TABLE_NAME = c.TABLE_NAME AND
                                  cu.TABLE_SCHEMA = c.TABLE_SCHEMA AND
                                  c.COLUMN_NAME = cu.COLUMN_NAME AND
                                  (OBJECTPROPERTY(OBJECT_ID(cu.CONSTRAINT_SCHEMA + '.' + QUOTENAME(cu.CONSTRAINT_NAME)), 'IsPrimaryKey') = 1) 
                              ), 'NO') as IS_PRIMARY_KEY  
                        FROM 
                           INFORMATION_SCHEMA.columns C 
                        where c.table_name = '" + nombreTabla + "' ORDER BY 1 ";
                return DataModel.Instance.Execute<Entities.EstructuraTabla>(sSql);
            }
            catch (Exception ex )
            {
                throw ex;
            }
        }

        public string GetNaneColumnPKByTabla(string nombreTabla)
        {
            try
            {
                string sSql = string.Empty;
                List<Entities.EstructuraTabla> listaResultado = new List<Entities.EstructuraTabla>();
                sSql = @"   SELECT
                            c.ORDINAL_POSITION,
                            c.TABLE_NAME,
                            c.COLUMN_NAME,
                            c.DATA_TYPE,
                            CAST(ISNULL(c.CHARACTER_MAXIMUM_LENGTH,0) AS INT) CHARACTER_MAXIMUM_LENGTH,
                            CAST(ISNULL(c.NUMERIC_PRECISION,0)AS INT) NUMERIC_PRECISION,
                            CAST(ISNULL(c.NUMERIC_PRECISION_RADIX,0) AS INT) NUMERIC_PRECISION_RADIX,
                            c.IS_NULLABLE,
                            ISNULL(c.COLUMN_DEFAULT,'') COLUMN_DEFAULT,
                            COALESCE(
                                  ( 
                                      select 'YES'
                                      from INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu 
                                      where 
                                      cu.TABLE_NAME = c.TABLE_NAME AND 
                                      cu.TABLE_SCHEMA = c.TABLE_SCHEMA and 
                                      c.COLUMN_NAME = cu.COLUMN_NAME and 
                                      (OBJECTPROPERTY(OBJECT_ID(cu.CONSTRAINT_SCHEMA + '.' + QUOTENAME(cu.CONSTRAINT_NAME)), 'IsPrimaryKey') = 1) 
                                  ), 'NO') as IS_PRIMARY_KEY 
                            FROM 
                               INFORMATION_SCHEMA.columns C
                            where c.table_name = '" + nombreTabla + "' ORDER BY 1 ";
                listaResultado = DataModel.Instance.Execute<Entities.EstructuraTabla>(sSql);
                var item = listaResultado.Where(x => x.IS_PRIMARY_KEY == "YES").FirstOrDefault();
                return item.COLUMN_NAME;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void executeRenombreTablaHistoricoToOld(string nombreTablaHistorico,string nombreTablaHistoricoOld)
        {
            try
            {
                string sSql = string.Empty;
                // Eliminacion de constraint existente.
                Crud.Constraint.Instance.eliminar_CONSTRAINT(nombreTablaHistorico);
                sSql = string.Format("EXEC sp_rename '{0}' , '{1}' ;", nombreTablaHistorico, nombreTablaHistoricoOld);
                var ok = DataModel.Instance
                    .Transaction(scope =>
                    {
                        DataModel.Instance.
                        ExecuteNonQuery(sSql);
                    },true);

                if (!ok)
                {
                    throw new BusinessException(Code.Historico_ErrorRenombreHistorico);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void executeRevertirRenombreTablaHistoricoToOld(string nombreTablaHistorico, string nombreTablaHistoricoOld)
        {
            try
            {
                string sSql = string.Empty;
                //Eliminacion de constraint existente.
                //Crud.Constraint.Instance.eliminar_CONSTRAINT(nombreTablaHistorico);
                sSql = string.Format("EXEC sp_rename '{0}' , '{1}' ;", nombreTablaHistoricoOld, nombreTablaHistorico);
                var ok = DataModel.Instance
                    .Transaction(scope =>
                    {
                        DataModel.Instance.
                        ExecuteNonQuery(sSql);
                    },true);
                if (!ok)
                    throw new BusinessException(Code.Historico_ErrorRevertirRenombre);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void crearTablaHistorico(string nombreTablaBase, List<Entities.EstructuraTabla> listaCamposdeTablaBase) //// Lista de campos de tablaBase
        {
            try
            {
                string sSql = string.Empty;
                if (string.IsNullOrEmpty(nombreTablaBase))
                    return;
                sSql = "CREATE TABLE HIST_" + nombreTablaBase + " ( ";
                int i = 0;
                // Recorrer cada campo de la lista para ir armando el create de la tabla historico
                foreach (var item in listaCamposdeTablaBase.OrderBy(x => x.ORDINAL_POSITION).ToList())
                {
                    string sSqlLine = string.Empty;
                    i++;
                    // 1. Nombre Columna tabla
                    sSqlLine += Environment.NewLine + item.COLUMN_NAME + " ";
                    // 2. Tipo de dato
                    sSqlLine += item.DATA_TYPE + " ";
                    // 3. Armado de la longitud del tipo de dato
                    // # Verificar Si Es string y sus variantes..
                    if (
                        item.DATA_TYPE.ToUpper() == "VARCHAR"   || 
                        item.DATA_TYPE.ToUpper() == "NVARCHAR"  || 
                        item.DATA_TYPE.ToUpper() == "CHAR"      || 
                        item.DATA_TYPE.ToUpper() == "NCHAR"
                        ) 
                    {
                        //Se toma la Longitud varchar
                        sSqlLine += "(";
                        sSqlLine += (item.CHARACTER_MAXIMUM_LENGTH ==-1) ? "MAX": item.CHARACTER_MAXIMUM_LENGTH.ToString();
                        sSqlLine += ")";
                    }
                    // # Verificar si es decimal
                    else if (item.DATA_TYPE.ToUpper() == "DECIMAL")
                    {
                        //Sino: verificar si es decimal
                        //Entonces Tomar Longitud(X,Y)
                        sSqlLine += "(";
                        sSqlLine += item.NUMERIC_PRECISION.ToString();
                        sSqlLine += ",";
                        sSqlLine += item.NUMERIC_PRECISION_RADIX.ToString();
                        sSqlLine += "(";
                    }
                    if (
                        item.DATA_TYPE.ToUpper() == "VARBINARY" ||
                        item.DATA_TYPE.ToUpper() == "BINARY"
                        )
                    {
                        //Se toma la Longitud varchar
                        sSqlLine += "(";
                        sSqlLine += (item.CHARACTER_MAXIMUM_LENGTH == -1) ? "MAX" : item.CHARACTER_MAXIMUM_LENGTH.ToString();
                        sSqlLine += ")";
                    }
                    // # Verificar Si es nullable
                    if (item.IS_NULLABLE != "YES")
                        sSqlLine += " NOT NULL ";
                    // Si tiene default se agregara a la nueva tabla
                    if (item.COLUMN_DEFAULT.Trim() !="")
                        sSqlLine +=  " DEFAULT " + item.COLUMN_DEFAULT.ToString();
                    //sSqlLine += " CONSTRAINT DF_HIST_" + item.TABLE_NAME + "_" + item.COLUMN_NAME + "_" + " DEFAULT " + item.COLUMN_DEFAULT.ToString();
                    // # Verificar Si tiene clave primaria unica
                    if (
                        (listaCamposdeTablaBase.Where(x => x.IS_PRIMARY_KEY == "YES").Count() == 1) &&
                        (item.IS_PRIMARY_KEY == "YES")
                        )
                    // # Se crea la primary key unico campo
                    sSqlLine += " CONSTRAINT PK_" + item.TABLE_NAME + "_" + item.COLUMN_NAME + " PRIMARY KEY";
                    // # Se agrega la , como separador de campos
                    if (i<listaCamposdeTablaBase.Count())
                        sSqlLine += ", ";
                    // # Se agrega la nueva linea al query resultado sSql
                    sSql += Environment.NewLine + sSqlLine;
                }
                if (listaCamposdeTablaBase.Count()>1)
                        sSql += Environment.NewLine + " );";
                var ok = DataModel.Instance
                .Transaction(scope =>
                {
                    DataModel.Instance.
                    ExecuteNonQuery(sSql);
                },true);
                if (!ok)
                    throw new BusinessException(Code.Historico_CreacionTablaHistorico);
                // Armado de las primary key compuesta
                var listaCamposdeTablaBasePk = listaCamposdeTablaBase.Where(x => x.IS_PRIMARY_KEY == "YES").ToList();
                if (listaCamposdeTablaBasePk.Count() > 1)
                {
                    sSql = string.Empty;
                    string camposPk = string.Empty;
                    string nombre_camposPk = string.Empty;
                    int j =1;
                    foreach (var item in listaCamposdeTablaBasePk)
                    {
                        camposPk += item.COLUMN_NAME;
                        nombre_camposPk += item.COLUMN_NAME;
                        if (j < listaCamposdeTablaBasePk.Count())
                        {
                            camposPk += ",";
                            nombre_camposPk += "_";
                        }
                        j++;
                    }
                    string nombreTablaHisto = nombreTablaHistorico(nombreTablaBase);
                    sSql += "ALTER TABLE " + nombreTablaHisto + " ADD CONSTRAINT PK_HIST" + nombreTablaHisto + "_"  + nombre_camposPk + " PRIMARY KEY (" + camposPk + ");";
                    var oki = DataModel.Instance
                    .Transaction(scope =>
                    {
                        DataModel.Instance.
                        ExecuteNonQuery(sSql);
                    },true);
                    if (!oki)
                        throw new BusinessException(Code.Historico_ErrorTablaHistoricoCreacionPK);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string nombreTablaHistorico(string nombreTablaBase)
        {
            return string.Format("HIST_{0}", nombreTablaBase);
        }

        public static string renombreTablaHistoricoToOld(string nombreTablahistorico)
        {
            string nombreTablaRenombrada = string.Format("{0}_old", nombreTablahistorico);
            return nombreTablaRenombrada;
        }

        public string obtenerColumnasTablaHistoForInsertInto(string nombreTablaHisto)
        {
            //Objetivo: Obtener las columnas para insert into nombreTablaHisto(columnas....)

            string resultadoSelect = string.Empty;
            
            // Obtener lista de columnas de la tabla historico
            List<Entities.EstructuraTabla> listaColumnasTabla = new List<Entities.EstructuraTabla>();
            listaColumnasTabla = GetEstructurasTablasByTabla(nombreTablaHisto);
            int i = 0;
            foreach (var item in listaColumnasTabla.OrderBy(x=>x.ORDINAL_POSITION))
            {
                resultadoSelect += item.COLUMN_NAME;
                i++;
                if (i < listaColumnasTabla.Count())
                    resultadoSelect += ",";
            }
            return resultadoSelect;
        }

        public string obtenerColumnasTablaBaseForSelectInsert(string nombreTablaBase, string nombreTablaHistorica)
        {
            // Objetivo: obtener en un string las columnas que se utilizaran para el select de las columnas 
            // que se van a insertar en la tabla historica

            string resultadoSelect = string.Empty;

            // Lista de columnas de las tablas Base
            List<Entities.EstructuraTabla> listaColumnasTablaBase = new List<Entities.EstructuraTabla>();
            listaColumnasTablaBase = GetEstructurasTablasByTabla(nombreTablaBase);
            
            // Lista de columnas de la tabla Secundaria
            List<Entities.EstructuraTabla> listaColumnasTablaHistorica = new List<Entities.EstructuraTabla>();
            listaColumnasTablaHistorica = GetEstructurasTablasByTabla(nombreTablaHistorica);
            
            // Lista de datos maestro SQL: Que tener encuenta para chequear segun el DATA_TYPE
            // toma un valor por default preestablecido
            List<Entities.Data_Type_Sql> listaDataTypeMaestro = Crud.Data_Type_Sql.Instance.Get_Data_Type_Base_Sql();
            
            // iteracion que comparara cada una de las columnas tabla Base vs Tabla Secundaria
            int i = 0;
            
            // Chequear los campos uno a uno
            foreach (var itemTablaSelect in listaColumnasTablaBase.OrderBy(x => x.ORDINAL_POSITION))
            {
                // Verifica si el campo de la tabla Base esta dentro de la tabla Secundaria
                if (listaColumnasTablaHistorica.Where(x => x.COLUMN_NAME == itemTablaSelect.COLUMN_NAME).Count() == 1)
                {
                    //Tomar el nombre de la columna
                    resultadoSelect += itemTablaSelect.COLUMN_NAME;
                }
                else
                {
                    // En el caso que no este se buscara un valor default preestablecido
                    var columnDefined = listaDataTypeMaestro.Where(x => x.DATA_TYPE == itemTablaSelect.DATA_TYPE).FirstOrDefault();
                    if (columnDefined == null)
                        return  "ERROR";
                    resultadoSelect += columnDefined.VALUE_DEFAULT;
                }
                // incrementador
                i++;
                // Evalua si se le agrega la coma o no al final de cada campo
                if (i < listaColumnasTablaBase.Count())
                    resultadoSelect += ",";
            }
            return resultadoSelect;
        }
    }
}
