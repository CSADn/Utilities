using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Helpers
{
    public class ExpressionConverter : GenericSingleton<ExpressionConverter>
    {
        #region Delegados

        protected delegate void OperatorHandler(List<string> whereStr);

        protected delegate void MethodInfoHandler(List<string> whereStr);

        protected delegate void MethodParamsHandler(IList<object> parameters, object value);

        #endregion

        #region Diccionarios Metodos Expression

        private Dictionary<ExpressionType, OperatorHandler> _dicOperators = null;

        private Dictionary<string, MethodInfoHandler> _dicMethods = null;

        private Dictionary<string, MethodParamsHandler> _dicMethodsParams = null;

        #endregion

        #region Constructor

        protected ExpressionConverter()
        {
            _dicOperators = new Dictionary<ExpressionType, OperatorHandler>()
            {
                { ExpressionType.Add, NoAddToWhereStr },
                { ExpressionType.AddChecked, NoAddToWhereStr },
                { ExpressionType.And, AddAndToWhereStr },
                { ExpressionType.AndAlso, AddAndToWhereStr },
                { ExpressionType.ArrayIndex, NoAddToWhereStr },
                { ExpressionType.ArrayLength, NoAddToWhereStr },
                { ExpressionType.Call, NoAddToWhereStr },
                { ExpressionType.Coalesce, NoAddToWhereStr },
                { ExpressionType.Conditional, NoAddToWhereStr },
                { ExpressionType.Constant, NoAddToWhereStr },
                { ExpressionType.Convert, NoAddToWhereStr },
                { ExpressionType.ConvertChecked, NoAddToWhereStr },
                { ExpressionType.Divide, NoAddToWhereStr },
                { ExpressionType.Equal, AddEqualToWhereStr },
                { ExpressionType.ExclusiveOr, AddOrToWhereStr },
                { ExpressionType.GreaterThan, AddGreaterThanToWhereStr },
                { ExpressionType.GreaterThanOrEqual, AddGreaterThanOrEqualToWhereStr },
                { ExpressionType.Invoke, NoAddToWhereStr },
                { ExpressionType.Lambda, NoAddToWhereStr },
                { ExpressionType.LeftShift, NoAddToWhereStr },
                { ExpressionType.LessThan, AddLessThanToWhereStr },
                { ExpressionType.LessThanOrEqual, AddLessThanOrEqualToWhereStr },
                { ExpressionType.ListInit, NoAddToWhereStr },
                { ExpressionType.MemberAccess, NoAddToWhereStr },
                { ExpressionType.MemberInit, NoAddToWhereStr },
                { ExpressionType.Modulo, NoAddToWhereStr },
                { ExpressionType.Multiply, NoAddToWhereStr },
                { ExpressionType.MultiplyChecked, NoAddToWhereStr },
                { ExpressionType.Negate, AddNotEqualToWhereStr },
                { ExpressionType.NegateChecked, NoAddToWhereStr },
                { ExpressionType.New, NoAddToWhereStr },
                { ExpressionType.NewArrayBounds, NoAddToWhereStr },
                { ExpressionType.NewArrayInit, NoAddToWhereStr },
                { ExpressionType.Not, AddNotEqualToWhereStr },
                { ExpressionType.NotEqual, AddNotEqualToWhereStr },
                { ExpressionType.Or, AddOrToWhereStr },
                { ExpressionType.OrElse, AddOrToWhereStr },
                { ExpressionType.Parameter, NoAddToWhereStr },
                { ExpressionType.Power, NoAddToWhereStr },
                { ExpressionType.Quote, NoAddToWhereStr },
                { ExpressionType.RightShift, NoAddToWhereStr },
                { ExpressionType.Subtract, NoAddToWhereStr },
                { ExpressionType.SubtractChecked, NoAddToWhereStr },
                { ExpressionType.TypeAs, NoAddToWhereStr },
                { ExpressionType.TypeIs, NoAddToWhereStr },
                { ExpressionType.UnaryPlus, NoAddToWhereStr },
            };

            _dicMethods = new Dictionary<string, MethodInfoHandler>()
            {
                { "Contains", ProcessContainsMethod },
                { "StartsWith", ProcessStartsWith },
                { "EndsWith", ProcessEndsWith },
                { "Equals", ProcessEquals },
                { "NotContains", ProcessNotContainsMethod },
                { "NotStartsWith", ProcessNotStartsWith },
                { "NotEndsWith", ProcessNotEndsWith },
                { "NotEquals", ProcessNotEquals },
                { "ToLower", ProcessToLower },
                { "ToUpper", ProcessToUpper },
            };

            _dicMethodsParams = new Dictionary<string, MethodParamsHandler>()
            {
                { "Contains", AddContainsParm },
                { "StartsWith", AddStartsWithParm },
                { "EndsWith", AddEndsWithParm },
                { "NotContains", AddContainsParm },
                { "NotStartsWith", AddStartsWithParm },
                { "NotEndsWith", AddEndsWithParm },
            };
        }

        #endregion

        #region Diccionario Parametros

        private void AddContainsParm(IList<object> parameters, object value)
        {
            string like = "%";

            parameters.Add(Convert.ChangeType(like + value.ToString() + like, TypeCode.Object));
        }

        private void AddStartsWithParm(IList<object> parameters, object value)
        {
            string like = "%";

            parameters.Add(Convert.ChangeType(like + value.ToString(), TypeCode.Object));
        }

        private void AddEndsWithParm(IList<object> parameters, object value)
        {
            string like = "%";

            parameters.Add(Convert.ChangeType(value.ToString() + like, TypeCode.Object));
        }

        #endregion

        #region Diccionario Metodos

        private void ProcessNotEquals(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " <> ?");
        }

        private void ProcessNotEndsWith(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " NOT LIKE ?");
        }

        private void ProcessNotStartsWith(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " NOT LIKE ?");
        }

        private void ProcessNotContainsMethod(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " NOT LIKE ?");
        }

        private void ProcessEquals(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " = ?");
        }

        private void ProcessEndsWith(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " LIKE ?");
        }

        private void ProcessStartsWith(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " LIKE ?");
        }

        private void ProcessContainsMethod(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " LIKE ?");
        }

        private void ProcessToLower(List<string> whereStr)
        {
            whereStr[whereStr.Count() - 1] = $"LOWER({whereStr.Last()})";
        }

        private void ProcessToUpper(List<string> whereStr)
        {
            whereStr[whereStr.Count()-1] = $"UPPER('{whereStr.Last()}')";
        }

        #endregion

        #region Metodos Diccionario operadores

        private void NoAddToWhereStr(List<string> whereStr)
        {
            //Do nothing
        }

        private void AddToWhereStr(List<string> whereStr, string op)
        {
            whereStr.Add(op);
        }

        private void AddAndToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " AND ");
        }

        private void AddEqualToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " = ");
        }

        private void AddNotEqualToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " <> ");
        }

        private void AddOrToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " OR ");
        }

        private void AddGreaterThanToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " > ");
        }

        private void AddGreaterThanOrEqualToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " >= ");
        }

        private void AddLessThanToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " < ");
        }

        private void AddLessThanOrEqualToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " <= ");
        }

        private void AddNegateToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " NOT ");
        }

        private void AddIsNullToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " IS NULL ");
        }

        private void AddIsNotNullToWhereStr(List<string> whereStr)
        {
            AddToWhereStr(whereStr, " IS NOT NULL ");
        }

        #endregion

        #region Proceso de Expresiones

        private void ProcessBinaryExpression<E>(Expression bodyExpr, List<string> whereStr, IList<object> listadoParametros) where E : class, new()
        {
            if (bodyExpr.ToString().StartsWith("("))
                whereStr.Add("(");

            GetWhereFromLamdaExpression<E>((bodyExpr as BinaryExpression).Left, whereStr, listadoParametros);

            if ((bodyExpr as BinaryExpression).Right is ConstantExpression && ((bodyExpr as BinaryExpression).Right as ConstantExpression).Value == null)
                ProcessNullConstantExpression(bodyExpr, whereStr);
            else
            {
                _dicOperators[(bodyExpr as BinaryExpression).NodeType].Invoke(whereStr);
                GetWhereFromLamdaExpression<E>((bodyExpr as BinaryExpression).Right, whereStr, listadoParametros);
            }

            if (bodyExpr.ToString().EndsWith(")"))
                whereStr.Add(")");
        }

        private void ProcessMemberExpression<E>(Expression bodyExpr, List<string> whereStr, IList<object> listadoParametros) where E : class, new()
        {
            if ((bodyExpr as MemberExpression).Expression is MemberExpression && ((bodyExpr as MemberExpression).Expression as MemberExpression).Expression is ConstantExpression)
            {
                ProcessMemberExpressionAsConstant(bodyExpr, whereStr, listadoParametros);
            }
            else if ((bodyExpr as MemberExpression).Expression is MemberExpression && ((bodyExpr as MemberExpression).Expression as MemberExpression).Expression is MemberExpression)
            {
                whereStr.Add("?");
                ProcessBothMemberExpression(bodyExpr, listadoParametros);
            }
            else if ((bodyExpr as MemberExpression).Member.ReflectedType == typeof(E))
            {
                string propName = (bodyExpr as MemberExpression).Member.Name;

                whereStr.Add($"[{propName.ToLowerInvariant()}]");
            }
            else if ((bodyExpr as MemberExpression).Expression == null && (bodyExpr as MemberExpression).NodeType == ExpressionType.MemberAccess)
            {
                whereStr.Add("?");
                ProcessMemberAccess(bodyExpr, listadoParametros);
            }
            else
            {
                whereStr.Add("?");
                ProcessMemberExpressionAsConstant(bodyExpr, listadoParametros);
            }
        }

        private void ProcessBothMemberExpression(Expression bodyExpr, IList<object> listadoParametros)
        {
            PropertyInfo propInf = ((bodyExpr as MemberExpression).Expression as MemberExpression).Member as PropertyInfo;
            object objValue = null;
            object obj = ((((bodyExpr as MemberExpression).Expression as MemberExpression).Expression as MemberExpression).Expression as ConstantExpression).Value;
            string memName = (((bodyExpr as MemberExpression).Expression as MemberExpression).Expression as MemberExpression).Member.Name;
            object realObj = obj.GetType().GetProperty(memName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);

            objValue = propInf.GetValue(realObj, null);
            listadoParametros.Add(objValue);
        }

        private void ProcessMemberAccess(Expression bodyExpr, IList<object> listadoParametros)
        {
            MemberInfo member = (bodyExpr as MemberExpression).Member;

            if (member != null)
            {
                BindingFlags bFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
                
                if(member.MemberType == MemberTypes.Field)
                {
                    FieldInfo field = member.DeclaringType.GetField(member.Name, bFlags | BindingFlags.NonPublic);
                    if (field != null)
                    {
                        object value = field.GetValue(null);
                        listadoParametros.Add(value);
                    }
        
                }
                else if(member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo prop = member.DeclaringType.GetProperty(member.Name, bFlags | BindingFlags.NonPublic);
                    if(prop != null)
                        listadoParametros.Add(prop.GetValue(null, null));
                }
                else
                {
                    throw new NotSupportedException("Member Type No Soportado");
                }
            }
            else
                throw new NotSupportedException("");
        }

        private void ProcessMemberExpressionAsConstant(Expression bodyExpr, List<string> whereStr, IList<object> listadoParametros)
        {
            object obj = (((bodyExpr as MemberExpression).Expression as MemberExpression).Expression as ConstantExpression).Value;

            string memName = ((bodyExpr as MemberExpression).Expression as MemberExpression).Member.Name;
            string propName = (bodyExpr as MemberExpression).Member.Name;
            object realObj = obj.GetType().GetField(memName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);

            if (realObj.GetType().IsPrimitive)
            {
                whereStr.Add("?");
                listadoParametros.Add(realObj);
                return;
            }

            PropertyInfo prop = realObj.GetType().GetProperty(propName);
            FieldInfo field = realObj.GetType().GetField(propName);

            if (prop == null && field == null)
                throw new NotSupportedException();

            object objValue = null;
            if (prop != null)
                objValue = prop.GetValue(realObj, null);
            else if (field != null)
                objValue = field.GetValue(realObj);

            if (objValue is Enum)
                    objValue = (objValue as Enum).GetHashCode();
            
            whereStr.Add("?");
            listadoParametros.Add(objValue);
        }

        private void ProcessMemberExpressionAsConstant(Expression bodyExpr, IList<object> listadoParametros, string method = null)
        {
            PropertyInfo prop = (bodyExpr as MemberExpression).Member.ReflectedType.GetProperty((bodyExpr as MemberExpression).Member.Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

            object obj = ((bodyExpr as MemberExpression).Expression as ConstantExpression).Value;
            string propName = (bodyExpr as MemberExpression).Member.Name;
            object objValue = (bodyExpr as MemberExpression).Member.ReflectedType.GetField(propName)?.GetValue(obj);

            if (objValue is Enum)
                objValue = (objValue as Enum).GetHashCode();

            if (!string.IsNullOrWhiteSpace(method) && _dicMethodsParams.ContainsKey(method))
                _dicMethodsParams[method].Invoke(listadoParametros, objValue);
            else
            {
                if (prop != null)
                    listadoParametros.Add(prop.GetValue(null, null));
                else if (objValue != null)
                    listadoParametros.Add(objValue);
                else throw new NotSupportedException("El motor no soporta llamadas a miembros de variables no estaticas.");
            }
        }

        private void ProcessParameterExpression(Expression bodyExpr, List<string> whereStr, bool setStatement)
        {
            string propName = (bodyExpr as ParameterExpression).Name;
            string oper = setStatement ? ", " : " AND ";

            whereStr.Add(oper + $"[{propName.ToLowerInvariant()}]");
        }

        private void ProcessMethodCallExpression<E>(Expression bodyExpr, List<string> whereStr, IList<object> listadoParametros) where E : class, new()
        {
            MethodCallExpression me = bodyExpr as MethodCallExpression;

            if (me != null)
            {
                if (!(me.Object is MemberExpression))
                    throw new NotSupportedException("Error en el metodo invocado");

                ProcessMemberExpression<E>(me.Object as Expression, whereStr, listadoParametros);

                _dicMethods[me.Method.Name].Invoke(whereStr);

                var arg = me.Arguments.FirstOrDefault();

                if (arg is MemberExpression)
                    ProcessMemberExpressionAsConstant(arg, listadoParametros, me.Method.Name);
                else if (arg is ConstantExpression)
                {
                    if (_dicMethodsParams.ContainsKey(me.Method.Name))
                        _dicMethodsParams[me.Method.Name].Invoke(listadoParametros, (arg as ConstantExpression).Value);
                    else
                        listadoParametros.Add((arg as ConstantExpression).Value);
                }

            }
        }

        private void ProcessUnaryExpression<E>(Expression bodyExpr, List<string> whereStr, IList<object> listadoParametros) where E : class, new()
        {
            if ((bodyExpr as UnaryExpression).Operand != null)
            {
                UnaryExpression unary = bodyExpr as UnaryExpression;
                if (unary.NodeType == ExpressionType.Not && unary.Operand is MethodCallExpression)
                {
                    MethodCallExpression me = unary.Operand as MethodCallExpression;

                    if (me != null)
                    {
                        if (!(me.Object is MemberExpression))
                            throw new NotSupportedException("Error en el metodo invocado");

                        ProcessMemberExpression<E>(me.Object as Expression, whereStr, listadoParametros);

                        var arg = me.Arguments[0];

                        _dicMethods["Not" + me.Method.Name].Invoke(whereStr);
                        listadoParametros.Add((arg as ConstantExpression).Value);
                    }
                }
                else
                    GetWhereFromLamdaExpression<E>(unary.Operand, whereStr, listadoParametros);
            }
            else
                throw new NotSupportedException();
        }

        private void ProcessConstantExpression(Expression bodyExpr, List<string> whereStr, IList<object> listadoParametros)
        {
            whereStr.Add("?");
            listadoParametros.Add((bodyExpr as ConstantExpression).Value);
        }

        private void ProcessNullConstantExpression(Expression bodyExpr, List<string> whereStr)
        {
            if ((bodyExpr as BinaryExpression).NodeType == ExpressionType.Equal)
                AddIsNullToWhereStr(whereStr);

            else if ((bodyExpr as BinaryExpression).NodeType == ExpressionType.NotEqual)
                AddIsNotNullToWhereStr(whereStr);
            else
                throw new InvalidOperationException("Operador invalido para valores null");
        }

        /// <summary>
        /// Generar clausula WHERE en formato SQL
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="bodyExpr"></param>
        /// <param name="whereStr"></param>
        /// <param name="listadoParametros"></param>
        private void GetWhereFromLamdaExpression<E>(Expression bodyExpr, List<string> whereStr, IList<object> listadoParametros, bool setStatement = false) where E : class, new()
        {
            if (bodyExpr is BinaryExpression)
            {
                ProcessBinaryExpression<E>(bodyExpr, whereStr, listadoParametros);
            }
            else if (bodyExpr is MemberExpression)
            {
                ProcessMemberExpression<E>(bodyExpr, whereStr, listadoParametros);
            }
            else if (bodyExpr is ParameterExpression)
            {
                ProcessParameterExpression(bodyExpr, whereStr, setStatement);
            }
            else if (bodyExpr is MethodCallExpression)
            {
                ProcessMethodCallExpression<E>(bodyExpr, whereStr, listadoParametros);
            }
            else if (bodyExpr is UnaryExpression)
            {
                ProcessUnaryExpression<E>(bodyExpr, whereStr, listadoParametros);
            }
            else if (bodyExpr is ConditionalExpression)
            {
                throw new NotSupportedException();
            }
            else if (bodyExpr is ConstantExpression)
            {
                ProcessConstantExpression(bodyExpr, whereStr, listadoParametros);
            }
            else if (bodyExpr is NewExpression)
            {
                throw new NotSupportedException();
            }
            else if (bodyExpr is NewArrayExpression)
            {
                throw new NotSupportedException();
            }
            else if (bodyExpr is TypeBinaryExpression)
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region Metodos publicos

        public WhereClause GenerarClausulaWhere<E>(Expression<Func<E, bool>> whereExpr, bool setStatement = false) where E : class, new()
        {
            List<string> whereStr = new List<string>();
            IList<object> listadoParametros = new List<object>();

            GetWhereFromLamdaExpression<E>(whereExpr.Body, whereStr, listadoParametros, setStatement);

            string finalWhereStr = string.Join(" ", whereStr);

            return new WhereClause(finalWhereStr, listadoParametros.ToList());
        }

        #endregion
    }
}
