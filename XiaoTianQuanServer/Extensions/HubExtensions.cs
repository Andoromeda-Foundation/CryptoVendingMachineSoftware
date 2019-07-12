using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;

namespace XiaoTianQuanServer.Extensions
{
    public static class HubExtensions
    {
        public static Guid GetMachineId(this Hub hub)
        {
            // TODO: this is actually not a supported usage
            // SignalR is decoupled with HTTP
            var contextFeature =
                hub.Context.Features.SingleOrDefault(f => f.Key == typeof(IHttpContextFeature)).Value as
                    IHttpContextFeature;
            var id = contextFeature?.HttpContext.Items["MachineId"];
            return (Guid?)id ?? Guid.Empty;
        }
    }
}
