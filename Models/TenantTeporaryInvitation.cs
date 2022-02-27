using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class TenantTeporaryInvitation
    {
        [Key]
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string BuildingName { get; set; }
        public int FlatId { get; set; }
        public string UnitName { get; set; }
        public string PostalCode { get; set; }
        public int BuildingId { get; set; }
        public string Phonenumber { get; set; }
        public string InvitationCode { get; set; }
        public int FloorNumber { get; set; }
        public int UnitNumber { get; set; }
        public Guid InviterId { get; set; }
        public string Email { get; set; }
        public bool CodeValidation { get; set; }
        public DateTime InvitationDate { get; set; }


    }
}
