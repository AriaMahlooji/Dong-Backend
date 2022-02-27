using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Classes
{
    public class barChartData
    {
        public int flatsNumber { get; set; }
        public int buildingsNumber { get; set; }
        public int tenantsNumber { get; set; }
        public int expiredTenantsNumber { get; set; }
        public int votingsNumber { get; set; }
    }
}
