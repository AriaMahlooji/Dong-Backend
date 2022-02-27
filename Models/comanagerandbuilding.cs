using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class comanagerandbuilding
    {
        public int Id { get; set; }
        public Guid ComanagerId { get; set; }
        public int BuildingId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public Guid Adder { get; set; }
    }
}
