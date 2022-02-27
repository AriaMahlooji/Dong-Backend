using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kavenegar;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantTeporaryInvitationsController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly AuthenticationContext _context;

        public TenantTeporaryInvitationsController(AuthenticationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/TenantTeporaryInvitations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TenantTeporaryInvitation>>> GetTenantTeporaryInvitations()
        {
            return await _context.TenantTeporaryInvitations.ToListAsync();
        }

        // GET: api/TenantTeporaryInvitations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TenantTeporaryInvitation>> GetTenantTeporaryInvitation(int id)
        {
            var tenantTeporaryInvitation = await _context.TenantTeporaryInvitations.FindAsync(id);

            if (tenantTeporaryInvitation == null)
            {
                return NotFound();
            }

            return tenantTeporaryInvitation;
        }

        // PUT: api/TenantTeporaryInvitations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTenantTeporaryInvitation(int id, TenantTeporaryInvitation tenantTeporaryInvitation)
        {
            if (id != tenantTeporaryInvitation.Id)
            {
                return BadRequest();
            }

            _context.Entry(tenantTeporaryInvitation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantTeporaryInvitationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TenantTeporaryInvitations
        [HttpPost]
        public async Task<ActionResult<TenantTeporaryInvitation>> PostTenantTeporaryInvitation(TenantTeporaryInvitation tenantTeporaryInvitation)
        {
            var flat = new Flat();

            var Inviterid = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());

            if(_context.BAMs.Where(m=>m.BuildingId==tenantTeporaryInvitation.BuildingId && m.ManagerId == Inviterid && m.ExpireDate== DateTime.Parse("0001 - 01 - 01 12:00:00 AM")).Count()==0 && _context.comanagerandbuilding.Where(m=>m.ComanagerId == Inviterid && m.BuildingId ==tenantTeporaryInvitation.BuildingId && m.ExpireDate == DateTime.Parse("0001 - 01 - 01 12:00:00 AM")).Count()==0)
            {
                return StatusCode(403);//The Inviter Is not Manager or Comanager of the Building
            }

            

            var Invier = await _userManager.FindByIdAsync(Inviterid.ToString());
            var building = _context.Buildings.Find(tenantTeporaryInvitation.BuildingId);

            var flatsWithSameUnitNumber = _context.Flats.Where(m => m.BuildingId == tenantTeporaryInvitation.BuildingId && m.UnitNumber == tenantTeporaryInvitation.UnitNumber).ToList();

            foreach (var item in flatsWithSameUnitNumber)
            {
                foreach (var item1 in _context.FlatsAndTenants)
                {
                    if(item.Id == item1.FlatId && item1.ExpireDate == DateTime.Parse("0001 - 01 - 01 12:00:00 AM"))
                    {
                        return StatusCode(433);//There is already an active Tenant with this unitNumber
                    }
                }
            }
            
            tenantTeporaryInvitation.PostalCode = building.Postalcode;
            tenantTeporaryInvitation.BuildingName = building.Name;
            tenantTeporaryInvitation.InviterId =Inviterid;
            
            Random r = new Random();
            int randNum = r.Next(1000000);
            string sixDigitNumber = randNum.ToString("D6");
            tenantTeporaryInvitation.InvitationCode = sixDigitNumber;
            tenantTeporaryInvitation.CodeValidation = true;
            
            flat.Name = tenantTeporaryInvitation.UnitName;
            flat.UnitNumber = tenantTeporaryInvitation.UnitNumber;
            flat.FloorNumber = tenantTeporaryInvitation.FloorNumber;
            flat.BuildingId = tenantTeporaryInvitation.BuildingId;
            _context.Flats.Add(flat);
            building.Flats.Add(flat);
            await _context.SaveChangesAsync();
           // _context.SaveChanges();

            var lastflat = _context.Flats.LastOrDefault();
            tenantTeporaryInvitation.FlatId = lastflat.Id;

            _context.TenantTeporaryInvitations.Add(tenantTeporaryInvitation);
            await _context.SaveChangesAsync();
            //sending Email
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Building Charge", "buildingcharge@gmail.com"));
            message.To.Add(new MailboxAddress(tenantTeporaryInvitation.Fullname, tenantTeporaryInvitation.Email));
            message.Subject = "Welcome To Our Website";
            message.Body = new TextPart("plain")
            {
                Text = "Dear " + tenantTeporaryInvitation.Fullname + Environment.NewLine + "You were Invited to Join " + tenantTeporaryInvitation.BuildingName + " Building" + Environment.NewLine + "Floor:" + tenantTeporaryInvitation.FloorNumber + Environment.NewLine + "Unit.No:" + tenantTeporaryInvitation.UnitNumber + Environment.NewLine + "Postalcode:" + tenantTeporaryInvitation.PostalCode + Environment.NewLine + "Inviter : " + Invier.Fullname + Environment.NewLine + "Registration Code To Join This Flat Is : " + "\"" + sixDigitNumber + "\""
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("buildingcharge", "hamedan8");
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception e)
            {

                throw e;
            }

           /* var sender = "10007000600050";
            var receptor = tenantTeporaryInvitation.Phonenumber;
            var Message = "Dear " + tenantTeporaryInvitation.Fullname + Environment.NewLine + "You were Invited to Join " + tenantTeporaryInvitation.BuildingName + " Building" + Environment.NewLine + "Floor:" + tenantTeporaryInvitation.FloorNumber + Environment.NewLine + "Unit.No:" + tenantTeporaryInvitation.UnitNumber + Environment.NewLine + "Postalcode:" + tenantTeporaryInvitation.PostalCode + Environment.NewLine + "Inviter : " + Invier.Fullname + Environment.NewLine + "Registration Code To Join This Flat Is : " + sixDigitNumber;
            var api = new KavenegarApi("6A4B7943505448744C67564474574B6D744356724E776B4B34544847724A6879");
            api.Send(sender, receptor, Message);
*/
            return Ok();
        }

        // DELETE: api/TenantTeporaryInvitations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TenantTeporaryInvitation>> DeleteTenantTeporaryInvitation(int id)
        {
            var tenantTeporaryInvitation = await _context.TenantTeporaryInvitations.FindAsync(id);
            if (tenantTeporaryInvitation == null)
            {
                return NotFound();
            }

            _context.TenantTeporaryInvitations.Remove(tenantTeporaryInvitation);
            await _context.SaveChangesAsync();

            return tenantTeporaryInvitation;
        }

        private bool TenantTeporaryInvitationExists(int id)
        {
            return _context.TenantTeporaryInvitations.Any(e => e.Id == id);
        }
    }
}
