using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Services.LightningNetwork.Models
{
    public class HopHint
    {
        public string NodeId { get; set; }
        public string ChanId { get; set; }
        public long FeeBaseMsat { get; set; }
        public long FeeProportionalMillionths { get; set; }
        public long CltvExpiryDelta { get; set; }
    }
}
