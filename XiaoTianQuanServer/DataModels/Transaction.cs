using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using XiaoTianQuanProtocols;
using XiaoTianQuanProtocols.VendingMachineRequests;

namespace XiaoTianQuanServer.DataModels
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Inventory Inventory { get; set; }

        public int BasePrice { get; set; }

        public bool Settled { get; set; }

        public DateTime TransactionCreated { get; set; }

        public DateTime TransactionExpiry { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public bool Active { get; set; }
    }
}
