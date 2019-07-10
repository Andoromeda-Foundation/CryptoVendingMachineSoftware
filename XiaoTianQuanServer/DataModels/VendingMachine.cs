using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.DataModels
{
    public class VendingMachine
    {
        public int Id { get; set; }
        public Guid MachineId { get; set; }

        public Guid ExclusiveUseLock { get; set; }

        public List<Inventory> Inventories { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
