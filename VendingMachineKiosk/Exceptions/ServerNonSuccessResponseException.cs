using System;
using System.Runtime.Serialization;

namespace VendingMachineKiosk.Exceptions
{
    public class ServerNonSuccessResponseException : VendingMachineKioskException
    {
        public ServerNonSuccessResponseException()
        {
        }

        public ServerNonSuccessResponseException(string message) : base(message)
        {
        }

        public ServerNonSuccessResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ServerNonSuccessResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}