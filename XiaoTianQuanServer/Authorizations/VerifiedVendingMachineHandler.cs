using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XiaoTianQuanServer.Authorizations
{
    public class VerifiedVendingMachineHandler : AuthorizationHandler<VerifiedVendingMachineRequirement>
    {
        private readonly ILogger<VerifiedVendingMachineHandler> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly X509Certificate2 _trustedIssuer;

        public VerifiedVendingMachineHandler(ILogger<VerifiedVendingMachineHandler> logger, Settings.AuthorizationSettings authSettings, IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _trustedIssuer = authSettings.TrustedIssuer;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifiedVendingMachineRequirement requirement)
        {
            var httpContext = _contextAccessor.HttpContext;

            var certificate = await httpContext.Connection.GetClientCertificateAsync();
            if (certificate == null)
            {
                context.Fail();
                _logger.LogDebug("No client certificate found.");
                return;
            }

            if (VerifyCertificate(certificate))
            {
                var cn = certificate.GetNameInfo(X509NameType.SimpleName, false);
                if (!Guid.TryParse(cn, out var machineId))
                {
                    _logger.LogInformation($"Certificate valid but subject invalid: {certificate.Subject}");
                    context.Fail();
                    return;
                }

                context.Succeed(requirement);
                httpContext.Items["MachineId"] = machineId;
            }
            else
            {
                _logger.LogInformation(
                    $"Client certificate invalid: issuer {certificate.Issuer}, thumbprint {certificate.Thumbprint}");
                context.Fail();
            }
        }

        private bool VerifyCertificate(X509Certificate2 certificate)
        {
            X509Chain x509Chain = new X509Chain();
            x509Chain.ChainPolicy.ExtraStore.Add(_trustedIssuer);
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority | X509VerificationFlags.IgnoreWrongUsage;

            var valid = x509Chain.Build(certificate);
            var issuedBy = x509Chain.ChainElements.Cast<X509ChainElement>()
                .Any(x => x.Certificate.Thumbprint == _trustedIssuer.Thumbprint);
            return valid && issuedBy;
        }
    }
}
