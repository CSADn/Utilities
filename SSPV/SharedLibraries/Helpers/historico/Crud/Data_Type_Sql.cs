using DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crud
{
    public class Data_Type_Sql
    {
        private static Data_Type_Sql _instance;

        public static Data_Type_Sql Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Data_Type_Sql();

                return _instance;
            }
        }

        public Data_Type_Sql()
        {
            //
        }

        public List<Entities.Data_Type_Sql> Get_Data_Type_Base_Sql()
        {
            List<Entities.Data_Type_Sql> resultado = new List<Entities.Data_Type_Sql>();
            // Exact Numerics: BIGINT  BIT SMALLINT    SMALLMONEY      INT   TINYINT     MONEY
            // -- Sin longitud: DATOS NUMERICOS CON LONGITUD FIJA 
            resultado.Add(Entities.Data_Type_Sql.Create("BIGINT", false, false, false,"0"));
            resultado.Add(Entities.Data_Type_Sql.Create("BIT", false, false, false, "0"));
            resultado.Add(Entities.Data_Type_Sql.Create("SMALLINT", false, false, false, "0"));
            resultado.Add(Entities.Data_Type_Sql.Create("SMALLMONEY", false, false, false, "0"));
            resultado.Add(Entities.Data_Type_Sql.Create("INT", false, false, false, "0"));
            resultado.Add(Entities.Data_Type_Sql.Create("TINYINT", false, false, false, "0"));
            resultado.Add(Entities.Data_Type_Sql.Create("MONEY", false, false, false, "0"));
            // -- Con longitud: DATOS NUMERICOS CON LONGITUD ESPECIFICADA
            resultado.Add(Entities.Data_Type_Sql.Create("NUMERIC", true, true, true, "0"));
            resultado.Add(Entities.Data_Type_Sql.Create("DECIMAL", true, true, true, "0"));

            // Approximate Numerics:
            //--Con longitud: float(NUMERIC_PRECISION)
            resultado.Add(Entities.Data_Type_Sql.Create("FLOAT", false, true, false, "0"));
            //--Sin longitud: real
            resultado.Add(Entities.Data_Type_Sql.Create("REAL", false, false, false, "0"));

            // Date and Time
            // -- Sin longitud: date, datetimeoffset, datetime2, smalldatetime, datetime, time, 
            resultado.Add(Entities.Data_Type_Sql.Create("DATE", false, false, false, "'1900-01-01 00:00:00.000'"));
            resultado.Add(Entities.Data_Type_Sql.Create("DATETIMEOFFSET", false, false, false, "'1900-01-01 00:00:00.000'"));
            resultado.Add(Entities.Data_Type_Sql.Create("DATETIME2", false, false, false, "'1900-01-01 00:00:00.000'"));
            resultado.Add(Entities.Data_Type_Sql.Create("SMALLDATETIME", false, false, false, "'1900-01-01 00:00:00.000'"));
            resultado.Add(Entities.Data_Type_Sql.Create("DATETIME", false, false, false, "'1900-01-01 00:00:00.000'"));
            resultado.Add(Entities.Data_Type_Sql.Create("TIME", false, false, false, "'1900-01-01 00:00:00.000'"));
            
            // Character Strings
            //-- Con Longitud: char(character_maximum_lenght), varchar(character_maximum_lenght)
            resultado.Add(Entities.Data_Type_Sql.Create("CHAR", true, false, false,"'_'"));
            resultado.Add(Entities.Data_Type_Sql.Create("VARCHAR", true, false, false, "'_'"));
            //-- Sin longitud: text
            resultado.Add(Entities.Data_Type_Sql.Create("TEXT", false, false, false, "'_'"));
            
            // Unicode Character Strings
            //-- Con longitud: nchar(character_maximum_lenght), nvarchar(character_maximum_lenght)
            resultado.Add(Entities.Data_Type_Sql.Create("NCHAR", false, false, false, ""));
            resultado.Add(Entities.Data_Type_Sql.Create("NVARCHAR", false, false, false, ""));
            // --Sin longitud: ntext
            resultado.Add(Entities.Data_Type_Sql.Create("NTEXT", false, false, false, ""));

            //--Binary Strings
            //-- Con longitud: binary(character_maximum_lenght), varbinary(character_maximum_lenght)
            resultado.Add(Entities.Data_Type_Sql.Create("BINARY", true, false, false, "0x00"));
            resultado.Add(Entities.Data_Type_Sql.Create("VARBINARY", true, false, false, "0x"));
            //-- Sin longitud: image
            resultado.Add(Entities.Data_Type_Sql.Create("IMAGE", false, false, false, "0x"));

            return resultado;
        }
    }
}

