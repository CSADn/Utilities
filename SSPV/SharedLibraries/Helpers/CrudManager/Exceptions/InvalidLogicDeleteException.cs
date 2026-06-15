using System;
using System.Runtime.Serialization;

namespace Helpers
{
    [Serializable]
    class InvalidLogicDeleteException : Exception
    {
        public InvalidLogicDeleteException()
        {
        }

        public InvalidLogicDeleteException(string message) : base(message)
        {
        }

        public InvalidLogicDeleteException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidLogicDeleteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}