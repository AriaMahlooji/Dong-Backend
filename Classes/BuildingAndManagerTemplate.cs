using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Classes
{
    public class BuildingAndManagerTemplate
    {
        [Key]
        public int Id { get; set; }
        public string Postalcode { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string More { get; set; }
        public double MonthlyCharge { get; set; }
    }
}
