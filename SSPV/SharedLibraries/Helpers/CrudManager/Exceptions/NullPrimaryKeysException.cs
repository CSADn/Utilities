using System;

namespace Helpers
{
    public class NullPrimaryKeysException : NullReferenceException
    {
        public NullPrimaryKeysException()
        {

        }

        public NullPrimaryKeysException(string message)
            : base(message)
        {

        }   

        public NullPrimaryKeysException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
