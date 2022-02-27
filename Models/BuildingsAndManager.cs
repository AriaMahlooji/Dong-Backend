using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class BuildingsAndManager
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public int ManagerId { get; set; }
        public double MonthlyCharge { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
