using DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crud
{
    public class TablaFamilia
    {
        private static TablaFamilia _instance;

        public static TablaFamilia Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TablaFamilia();

                return _instance;
            }
        }

        public TablaFamilia()
        {
            //
        }

        public List<Entities.TablaFamilia> GetTablaFamiliaByClave(string clave)
        {
            try
            {
                string sSql = string.Empty;
                //sSql = @"
                //        SELECT DISTINCT
                //        OBJECT_NAME (fk.referenced_object_id) AS NombreTablaPadre,
                //        COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS NombreColumnaPadre,
                //        OBJECT_NAME(fk.parent_object_id) AS NombreTablaHijo,
                //        COL_NAME(fc.parent_object_id,fc.parent_column_id) AS NombreColumnaHijo
                //        --,fk.name AS FK
                //        FROM sys.foreign_keys AS fk
                //        INNER JOIN sys.foreign_key_columns  fc ON (fk.OBJECT_ID = fc.constraint_object_id)
                //        WHERE OBJECT_NAME (fk.referenced_object_id) = '" + clave.ToUpper() + "' ORDER BY 1,3";
                sSql = @"
                        SELECT DISTINCT
                        OBJECT_NAME (fk.referenced_object_id) AS NombreTablaPadre,
                        COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS NombreColumnaPadre,
                        OBJECT_NAME(fk.parent_object_id) AS NombreTablaHijo,
                        COL_NAME(fc.parent_object_id,fc.parent_column_id) AS NombreColumnaHijo
                        ,fk.name AS FK
                        FROM sys.foreign_keys AS fk
                        INNER JOIN sys.foreign_key_columns  fc ON (fk.OBJECT_ID = fc.constraint_object_id)
                        WHERE 
                        OBJECT_NAME (fk.referenced_object_id) = '@CLAVE@' 
                        AND
                        (	( 
                        	CASE when (
                        					SELECT COUNT(*) 
                        					FROM sys.foreign_keys AS fk1
                        					INNER JOIN sys.foreign_key_columns  fc1 ON (fk1.OBJECT_ID = fc1.constraint_object_id)
                        					WHERE OBJECT_NAME (fk1.referenced_object_id) = OBJECT_NAME (fk.referenced_object_id)	-- NombreTablaPadre
                        					AND OBJECT_NAME(fk1.parent_object_id) = OBJECT_NAME(fk.parent_object_id)				-- NombreTablaHijo 
                        				)>1
                        	THEN 1 ELSE 0 END = 1
                        	)
                        	AND 
                        	(
                        	EXISTS
                        		(
                        		SELECT 1 FROM sys.foreign_keys AS fk2
                        		INNER JOIN sys.foreign_key_columns  fc2 ON (fk2.OBJECT_ID = fc2.constraint_object_id)
                        		WHERE 
                        		OBJECT_NAME(fk2.parent_object_id) = OBJECT_NAME (fk.referenced_object_id) -- NombreTablaPadre buscado como HIJO
                        		AND 
                        		COL_NAME(fc2.referenced_object_id, fc2.referenced_column_id) = COL_NAME(fc.referenced_object_id, fc.referenced_column_id)
                        		)
                        	)
                        	OR
                        	(
                        	CASE when 
                        			(
                        			SELECT COUNT(*) 
                        			FROM sys.foreign_keys AS fk1
                        			INNER JOIN sys.foreign_key_columns  fc1 ON (fk1.OBJECT_ID = fc1.constraint_object_id)
                        			WHERE OBJECT_NAME (fk1.referenced_object_id) = OBJECT_NAME (fk.referenced_object_id)	-- NombreTablaPadre
                        			AND OBJECT_NAME(fk1.parent_object_id) = OBJECT_NAME(fk.parent_object_id)				-- Hijo
                        			) =1
                        	THEN 1 ELSE 0 END = 1
                        	)
                        )
                        ORDER BY 1,3;
                        ";
                sSql = sSql.Replace("@CLAVE@", clave.ToUpper());
                var resultado = DataModel.Instance.Execute<Entities.TablaFamilia>(sSql);

                return resultado;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
