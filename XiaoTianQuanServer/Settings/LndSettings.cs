using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Settings
{
    public class LndSettings
    {
        public Uri RestfulEndpoint { get; set; }
        public string Macaroon { get; set; }
        public bool CheckCertificate { get; set; }
    }
}
