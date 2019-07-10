using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.DataModels
{
    public class LightningNetworkTransaction
    {
        public int Id { get; set; }

        [Required]
        public Transaction Transaction { get; set; }

        public double Amount { get; set; }

        [Required]
        public string PaymentRequest { get; set; }
    }
}
