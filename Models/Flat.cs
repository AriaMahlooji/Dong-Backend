using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Flat
    {
        [Key]
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public int FloorNumber  { get; set; }
        public int UnitNumber { get; set; }
        public string Name { get; set; }

    }
}
