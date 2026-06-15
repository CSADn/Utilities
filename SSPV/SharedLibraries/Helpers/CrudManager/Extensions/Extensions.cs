using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class Extensions
    {
        public static List<Expression<Func<C, object>>> AddPk<C>(this List<Expression<Func<C, object>>> input, Expression<Func<C, object>> value)
            => Utilities.AddEx(input, value);

        public static List<Type> AddType(this List<Type> input, Type value)
            => Utilities.AddEx(input, value);

        public static List<Expression<Func<C, object>>> AddProp<C>(this List<Expression<Func<C, object>>> input, Expression<Func<C, object>> value)
            => Utilities.AddEx(input, value);

        public static void AddRange<C>(this List<Expression<Func<C, object>>> input, params Expression<Func<C, object>>[] values)
        {
            if (values == null || values.Length == 0)
                throw new Exception("Debe proporcionar las expresiones de la clave primaria");

            foreach (Expression<Func<C, object>> value in values)
                input.Add(value);
        }
    }
}
