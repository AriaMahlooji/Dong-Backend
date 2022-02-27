using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Voting
    {
        public int Id { get; set; }
        public int ReqId { get; set; }
        public Guid VoterId { get; set; }
        public DateTime VoteDate { get; set; }
        public bool Vote { get; set; }
    }
}
