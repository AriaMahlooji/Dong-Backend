using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public int FlatId { get; set; }
        public DateTime PaymentDate { get; set; }
        public double Amount { get; set; }
    }
}
