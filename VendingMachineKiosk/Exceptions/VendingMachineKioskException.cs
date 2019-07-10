using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineKiosk.Exceptions
{
    public abstract class VendingMachineKioskException : Exception
    {
        protected VendingMachineKioskException()
        {
        }

        protected VendingMachineKioskException(string message) : base(message)
        {
        }

        protected VendingMachineKioskException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VendingMachineKioskException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
