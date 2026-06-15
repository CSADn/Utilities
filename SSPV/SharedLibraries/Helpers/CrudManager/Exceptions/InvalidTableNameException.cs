using System;

namespace Helpers
{
    public class InvalidTableNameException : Exception
    {
        public InvalidTableNameException()
        {

        }

        public InvalidTableNameException(string message)
            : base(message)
        {

        }

        public InvalidTableNameException(string message, Exception inner)
            :base(message, inner)
        {

        }
    }
}
