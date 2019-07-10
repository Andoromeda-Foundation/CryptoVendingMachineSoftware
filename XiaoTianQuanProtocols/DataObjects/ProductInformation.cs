using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoTianQuanProtocols.DataObjects
{
    public class ProductInformation
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public Uri Image { get; set; }
        public string Slot { get; set; }
        public Dictionary<PaymentType, double> Prices { get; set; }
    }
}
