using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace XiaoTianQuanServer.Extensions
{
    public static class ControllerExtensions
    {
        public static Guid GetMachineId(this ControllerBase controller)
        {
            var id = (Guid)controller.HttpContext.Items["MachineId"];
            return id;
        }
    }
}
