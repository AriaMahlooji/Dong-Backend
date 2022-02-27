using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; }
        public Guid RequestorId { get; set; }
        public int BuildingId { get; set; }
        public double Price { get; set; }
        public string Abstract { get; set; }
        public string Context { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public TimeSpan Remained { get; set; }
        public double DaysRemained { get; set; }
        public double MinutesRemained { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsAllowed { get; set; }
        public int AgreeVotes { get; set; }
        public int DisAgreeVotes { get; set; }

    }
}
