using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class BuildingAndManager 
    {
        //Manages
        [Key]
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public Guid ManagerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public double MonthlyCharge { get; set; }

    }
}
