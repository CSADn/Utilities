using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Helpers;

namespace Exceptions
{
    public class BusinessException : Exception
    {
        public Code Code { get; private set; }
        public virtual string Description
        {
            get
            {
                return Code.Description();
            }
        }

        private string _extendedDescription;
        public virtual string ExtendedDescription
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_extendedDescription))
                    return Description;

                return Code.Description() + " " + _extendedDescription;
            }

            private set
            {
                _extendedDescription = value;
            }
        }

        public string Argument { get; private set; }

        public BusinessException(Code code)
            : this(code, string.Empty, string.Empty)
        {
            //
        }

        public BusinessException(Code code, string extendedDescription)
            : this(code, extendedDescription, string.Empty)
        {
            //
        }

        public BusinessException(Code code, string extendedDescription, string value)
        {
            Code = code;
            _extendedDescription = extendedDescription;
            Argument = value;
        }

        public override string ToString()
        {
            return
                "BusinessException: " +
                "Code: " + (int)Code + " - Description: " + ExtendedDescription + Environment.NewLine +
                base.ToString();
        }
    }
}
