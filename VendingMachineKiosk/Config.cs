using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineKiosk
{
    public static class Config
    {
        public const string RequestEndpoint = "https://127.0.0.1:5001/";
        public const string CertificateIssuer = "testroot don't trust";
        public const int ProductSelectionIdleTimeout = 60;
    }
}
