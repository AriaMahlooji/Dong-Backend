using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class FlatAndTenant
    {
        //joint
        [Key]
        public int Id { get; set; }
        public int FlatId { get; set; }
        public Guid TenantId { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; }

    }
}
