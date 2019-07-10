using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.DataModels
{
    public class Inventory
    {
        public int Id { get; set; }

        public int VendingMachineId { get; set; }
        public VendingMachine VendingMachine { get; set; }

        [Required]
        public string Slot { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public string Picture { get; set; }

        public int Quantity { get; set; }

        /// <summary>
        /// Minimum unit of official currency
        /// </summary>
        public int BasePrice { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
