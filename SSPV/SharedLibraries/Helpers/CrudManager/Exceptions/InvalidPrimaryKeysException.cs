using System;

namespace Helpers
{
    public class InvalidPrimaryKeysException : Exception
    {
        public InvalidPrimaryKeysException()
        {

        }

        public InvalidPrimaryKeysException(string message)
            : base(message)
        {

        }

        public InvalidPrimaryKeysException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
