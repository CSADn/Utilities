using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Exceptions
{
    public class Base64FormatException : Exception
    {
        public Base64FormatException()
        {
            //
        }

        public Base64FormatException(string message)
            : base(message)
        {
            //
        }
    }

    public class EmailFormatException : Exception
    {
        public EmailFormatException()
        {
            //
        }

        public EmailFormatException(string message)
            : base(message)
        {
            //
        }
    }
}
