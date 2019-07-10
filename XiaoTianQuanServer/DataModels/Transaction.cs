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
        public VendingMachine VendingMachine { get; set; }

        public DateTime TransactionCreated { get; set; }

        public PaymentType PaymentType { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
