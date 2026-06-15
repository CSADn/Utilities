namespace Helpers
{
    public class CrudSqlCondition
    {
        #region Propiedades

        public string FieldName { get; set; }

        public string Operator { get; set; }

        public object Value { get; set; }
        
        #endregion
        
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public CrudSqlCondition()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        public CrudSqlCondition(string fieldName, string op, object value)
        {
            this.FieldName = fieldName;
            this.Operator = op;
            this.Value = value;
        }

        #endregion
    }
}
