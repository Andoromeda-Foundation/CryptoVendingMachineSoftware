using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineKiosk.Exceptions
{
    public class ConfigException : VendingMachineKioskException
    {
        public ConfigException()
        {
        }

        public ConfigException(string message) : base(message)
        {
        }

        public ConfigException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConfigException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
