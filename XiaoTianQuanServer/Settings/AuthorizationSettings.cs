using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Settings
{
    public class AuthorizationSettings
    {
        public X509Certificate2 TrustedIssuer { get; set; }
    }
}
