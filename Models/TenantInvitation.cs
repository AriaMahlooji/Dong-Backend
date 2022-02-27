using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class TenantInvitation
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public string BuildingName { get; set; }
        public string Phonnumber { get; set; }
        public string Fullname { get; set; }
    }
}
