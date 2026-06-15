using DatabaseModel;
using Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crud
{
    public class Constraint
    {
        private static Constraint _instance;

        public static Constraint Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Constraint();

                return _instance;
            }
        }

        public Constraint()
        {
            //
        }

        public string Get_CONSTRAINT(string nombreTabla)
        {
            string
            sSqlContraint = @" SELECT CONSTRAINT_NAME 
                               FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                               WHERE TABLE_NAME = '" + nombreTabla + "';";

            var resultado = DataModel.Instance.Execute<Entities.Constraint>(sSqlContraint).FirstOrDefault();

            if (resultado == null)
                return "";

            return resultado.CONSTRAINT_NAME;
        }

        public bool eliminar_CONSTRAINT(string nombreTabla)
        {
            try
            {

            string nombreConstraintPK = this.Get_CONSTRAINT(nombreTabla);
            if (string.IsNullOrEmpty(nombreConstraintPK))
                return true;
            string
            sSqlContraint = " ALTER TABLE " + nombreTabla + " DROP CONSTRAINT  " + nombreConstraintPK + "; ";
            
           var ok = DataModel.Instance
            .Transaction(scope =>
            {
                DataModel.Instance.
                ExecuteNonQuery(sSqlContraint);
            },true);

            if (!ok)
                throw new BusinessException(Code.Historico_ErrorDelConstraint);
            return true;
            }
            catch (Exception)
            {
                throw;
            }

        }

    }
}

