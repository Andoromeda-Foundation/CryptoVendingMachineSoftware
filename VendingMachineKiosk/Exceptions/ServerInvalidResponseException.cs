using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineKiosk.Exceptions
{
    public class ServerInvalidResponseException : VendingMachineKioskException
    {
        public ServerInvalidResponseException()
        {
        }

        public ServerInvalidResponseException(string message) : base(message)
        {
        }

        public ServerInvalidResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ServerInvalidResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
