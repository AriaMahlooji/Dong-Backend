using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Classes
{
    public class Statistic
    {
        [Key]
        public int Id { get; set; }
        public int UserCount { get; set; }
        public int TenantCount { get; set; }
        public int ManagerCount { get; set; }
        public int CoManagerCount { get; set; }
        public int BuildingCount { get; set; }
        public int FlatCount { get; set; }
        public int RequestCount { get; set; }
        public int VoteCount { get; set; }

    }
}
