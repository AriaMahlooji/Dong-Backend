using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Classes
{
    public class TenantDetail
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public int FloorNumber { get; set; }
        public int UnitNumber { get; set; }
        public string UnitName { get; set; }
        public int BuildingId { get; set; }
        public int FlatId { get; set; }
        public int ResidenceDurationInDays { get; set; }
        public bool IsActive { get; set; }
        public bool IsManager { get; set; }
        public bool IsComanager { get; set; }

    }
}
