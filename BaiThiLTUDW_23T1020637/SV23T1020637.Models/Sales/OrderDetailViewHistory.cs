using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV23T1020637.Models.Sales
{
    public class OrderDetailViewHistory : Order
    {
        public List<OrderDetailViewInfo> LstOrderDetails { get; set; }
    }
}
