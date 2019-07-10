using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace XiaoTianQuanServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                        {
                            listenOptions.UseHttps(new HttpsConnectionAdapterOptions
                            {
                                ServerCertificate =
                                    new System.Security.Cryptography.X509Certificates.X509Certificate2(
                                        Convert.FromBase64String(
@"MIIEXgIBAzCCBCQGCSqGSIb3DQEHAaCCBBUEggQRMIIEDTCCAs8GCSqGSIb3DQEHBqCCAsAwggK8
AgEAMIICtQYJKoZIhvcNAQcBMBwGCiqGSIb3DQEMAQYwDgQIbzZwfhParhECAggAgIICiN7rKdv/
wczAwX2K7jix/2ag6xFsJmNN9HGEB0uWvKjV5XYGlc159uD6EIPMRIWLMGHTLPv3kiLNEuMaWzCv
orf+5jcArpM84LmRyAz7QsW+LPHbT4cl1O8oH8nM398Ri86wp3iVqTiTRO6Q3LEV72rJZH28q0lX
NPsy6iSRLRhkcDHKNHU1hZWpUIaxcBHOgsK28Aol58nEHydUVzjjOPz7zPjhmpbBP3v2yY45QFmL
SMpk6ODIkeB3fS39RS7Jq2DIHrQmp/Hkh1xhkanTT7j2xRYgN2IICGi+F62jvJIeY19z0TpjO+9u
2OuZsD0bxenArUTvfRewjbeYdhEVi488+Vwx+IvL5+sCKlTKudHMpVUHCu3yrBqNUNmJNCwRzhVU
SfXnLTOQ3oDNEf1WpA3c88aLQA5H2R8+6e4Zrhi1D6ZIlfCWIiO9LZpYngLwTDSEGGKhnfQ1j6J9
AtIc4+YHAqCtXOx0WpxfwodamUlC0WI8VfRllxSExhCNRa6xsJ01J4vU0FdbLl1wKHdhEvKMi2cW
JmASjehXfM3ttaj2aNBuat5YxLfZ6A/8v/yR2YhbwPByZT6quSehdQQO1EFfmiso5fggD45Jn4Gk
Qtqhwogifb9BiI87P0N3t7e/wW/XFqiBbouHTPmQiErLkxBi/mLn83JGEpSSmbhtCWR5oyP+n1Tw
649z9IqfCKIXqgP2qPSlYDrWhokR54QkpGKEld0BLCaUcS/JXZM0AwyUPeqoTS1lcicixFDe8Klj
5PY5CIs4Mtp+l5fPSuOwlxCLKeL/+lnT2WpnvivZRwJxoBm/Fe6yzZDut3oF+lHlR76a2YcAmA+w
KVyGOCirZ+0CN6jv3/ZiLDCCATYGCSqGSIb3DQEHAaCCAScEggEjMIIBHzCCARsGCyqGSIb3DQEM
CgECoIHkMIHhMBwGCiqGSIb3DQEMAQMwDgQIK1qH/NZ0b4gCAggABIHAYm7tDfBmmA5sG9H9nwxf
b08jTvku80/nOitYQEBEyGUaSOE4jxnFX77pkE9yd67ezOZLRhBAniE7JiAJzAnghyZvjc/5i6fI
WVmrMcHwlUZEGsRe2ECoMzbrAhVs7sSYBs2k78N+RDR8qgn+GZ2GXWDgjgh4HrbFj7MAit3jDqF6
51kNTlq7gk3zZYvwO2r/DV2JBAm24rMB6gsgcNQm1toIKhr/KzuqIg+b9RoLkttTWyXpGZri2gp0
PkqO9CCSMSUwIwYJKoZIhvcNAQkVMRYEFEF6lLn83CnuYMQsSrjBUTZUE1LrMDEwITAJBgUrDgMC
GgUABBQba7lvk0y4YEmXelEEp3gmP/W/mwQI1WUZhVuwZs8CAggA"), "123"),
                                ClientCertificateMode = ClientCertificateMode.AllowCertificate
                            });
                        });
                    });
                });
    }
}
