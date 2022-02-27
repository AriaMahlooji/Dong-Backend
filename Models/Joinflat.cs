using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Joinflat
    {
        [Key]
        public int Id { get; set; }
        public int MyProperty { get; set; }
    }
}
