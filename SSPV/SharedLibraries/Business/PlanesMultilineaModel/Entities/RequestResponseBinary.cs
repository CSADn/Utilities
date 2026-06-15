using PlanesMultilinea.Enums;

namespace PlanesMultilinea.Entities
{
    public class RequestResponseBinary
    {
        public byte[] Body { get; private set; }
        public string Error { get; set; }
        public ResponseStatus Status { get; private set; }

        public RequestResponseBinary(byte[] body, ResponseStatus status)
        {
            Body = body;
            Status = status;
        }

        public RequestResponseBinary(string error, ResponseStatus status)
        {
            Body = null;
            Error = error;
            Status = status;
        }
    }
}
