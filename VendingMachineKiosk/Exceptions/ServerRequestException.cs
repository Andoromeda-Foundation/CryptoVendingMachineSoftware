using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineKiosk.Exceptions
{
    public class ServerRequestException : VendingMachineKioskException
    {
        public ServerRequestException()
        {
        }

        public ServerRequestException(string message) : base(message)
        {
        }

        public ServerRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ServerRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
