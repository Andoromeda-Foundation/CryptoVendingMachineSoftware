using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace XiaoTianQuanServer.Extensions
{
    public static class HubExtensions
    {
        public static Guid GetMachineId(this Hub hub)
        {
            return (Guid)hub.Context.Items["MachineId"];
        }
    }
}
