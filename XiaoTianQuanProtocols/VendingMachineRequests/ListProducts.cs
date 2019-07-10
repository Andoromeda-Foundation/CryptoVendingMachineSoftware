using System;
using System.Collections.Generic;
using System.Text;
using XiaoTianQuanProtocols.DataObjects;

namespace XiaoTianQuanProtocols.VendingMachineRequests
{
    public class ListProductsRequest
    {
    }

    public class ListProductsResponse : ResponseBase
    {
        public IList<ProductInformation> Products;
    }
}
