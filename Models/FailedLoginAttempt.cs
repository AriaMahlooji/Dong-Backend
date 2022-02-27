using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class FailedLoginAttempt
    {
        public int Id { get; set; }
        public Guid UserId  { get; set; }
        public DateTime BlockedUntil { get; set; }
        public  int FailedAttemptsCounter { get; set; }
    }
}
