using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
    }
}
