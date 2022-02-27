using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Classes;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlatAndTenantController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        private UserManager<ApplicationUser> _userManager;


        public FlatAndTenantController(AuthenticationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/FlatAndTenant
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlatAndTenant>>> GetFlatsAndTenants()
        {
            return await _context.FlatsAndTenants.ToListAsync();
        }

        //Get: api/FlatAndTenant/6
       

        // GET: api/FlatAndTenant/5
        // [HttpGet("{id}")]
        //public async Task<ActionResult<IEnumerable<TenantDetail>>> GetTenant(int id)
        //{
        //    var tenantdetail = new TenantDetail();
        //    List<TenantDetail> Tenants = new List<TenantDetail>();
        //    var flats = _context.Flats.Where(m => m.BuildingId == id);
        //    foreach (var item in _context.FlatsAndTenants)
        //    {
        //        foreach (var item1 in flats)
        //        {
        //            if(item1.Id==item.FlatId)
        //            {
        //                tenantdetail.FloorNumber = item1.FloorNumber;
        //                tenantdetail.UnitName = item1.Name;
        //                tenantdetail.UnitNumber = item1.UnitNumber;
        //               // tenantdetail.FullName=_context.ApplicationUsers.FirstOrDefault(m=>Guid.Parse(m.Id)==item.TenantId)
        //            }
        //        }
        //    }
        //}

        // PUT: api/FlatAndTenant/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlatAndTenant(int id, FlatAndTenant flatAndTenant)
        {
            if (id != flatAndTenant.Id)
            {
                return BadRequest();
            }

            _context.Entry(flatAndTenant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlatAndTenantExists(id))
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
        //flat and teannt expiration
        [HttpPost("{id}")]
        [Route("EXPDate/{id}")]
        public async Task<IActionResult> PostEXPDate(int id)
        {
            var bulidingId = _context.Flats.Where(m => m.Id == id).FirstOrDefault().BuildingId;
            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            if(!isManagerOfThisBuilding(currentUserId,bulidingId))
            {
                //Only Manager can Remove
                return StatusCode(405);
            }
          
            
            
            
            
                var temp = _context.FlatsAndTenants.Where(m => m.FlatId == id).FirstOrDefault();

                
                var tenantId = _context.FlatsAndTenants.Where(m => m.FlatId == id).FirstOrDefault().TenantId;
                
                if(isManagerOfThisBuilding(tenantId,bulidingId))
                {
                //can't remove manager
                    return StatusCode(518);
                }
                if(IsComanagerOfBuilding(bulidingId,tenantId.ToString()))
                 {
                //can't remove co-Manager
                return StatusCode(519);
                 }
               
                temp.ExpireDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return StatusCode(200);
            
            
            
        }

        // POST: api/FlatAndTenant
        [HttpPost]
        public async Task<ActionResult<FlatAndTenant>> PostFlatAndTenant(TenantJoint tenantjoint)
        {
            var TenantId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var user = _userManager.FindByIdAsync(TenantId.ToString()).Result;
            var chosen = _context.TenantTeporaryInvitations.Where(m => m.InvitationCode == tenantjoint.RegistrationCode && m.Phonenumber == tenantjoint.Phonenumber ).FirstOrDefault();
           // var chechforvalidation = _context.TenantTeporaryInvitations.Where(m => m.InvitationCode == tenantjoint.RegistrationCode && m.Phonenumber == tenantjoint.Phonenumber && m.CodeValidation == false);
            
            if(chosen==null)
            {
                return StatusCode(419);
            }
            else if(chosen.CodeValidation==false)
            {
                return StatusCode(418);
            }
            var flatAndTenant = new FlatAndTenant();
            flatAndTenant.StartDate = DateTime.Now;
            flatAndTenant.TenantId = TenantId;
            flatAndTenant.FlatId = chosen.FlatId;
            
            _context.FlatsAndTenants.Add(flatAndTenant);
           
            chosen.CodeValidation = false;
            await _userManager.AddToRoleAsync(user, "Tenant");

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/FlatAndTenant/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<FlatAndTenant>> DeleteFlatAndTenant(int id)
        {
            var flatAndTenant = await _context.FlatsAndTenants.FindAsync(id);
            if (flatAndTenant == null)
            {
                return NotFound();
            }


            _context.FlatsAndTenants.Remove(flatAndTenant);
            
            await _context.SaveChangesAsync();

            return flatAndTenant;
        }

        private bool FlatAndTenantExists(int id)
        {
            return _context.FlatsAndTenants.Any(e => e.Id == id);
        }

        [HttpGet]
        [Route("GetBuildingsOfTenant")]
        public ActionResult<IEnumerable<Building>> GetBuildingsOfTenant()
        {
            var TenantId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var flatsandtenants = _context.FlatsAndTenants.Where(m => m.TenantId == TenantId && m.ExpireDate == DateTime.Parse("0001-01-01 12:00:00 AM")).ToList();
            List<Flat> flats = new List<Flat>();
            foreach (var item in _context.Flats)
            {
                foreach (var item1 in flatsandtenants)
                {
                    if (item1.FlatId == item.Id)
                    {
                        flats.Add(item);
                    }
                }
            }
            List<Building> buildings = new List<Building>();

            foreach (var item in flats)
            {
                foreach (var item1 in _context.Buildings)
                {
                    if (item.BuildingId == item1.Id)
                    {
                        if (!buildings.Contains(item1))
                        {
                            buildings.Add(item1);
                        }
                    }
                }
            }
            Console.WriteLine(buildings);
            return buildings;

        }
        [HttpGet]
        [Route("GetBuildingsOfTheUserAsManagerOrComanager")]

        public ActionResult<IEnumerable<Building>> GetBuildingsOfTheUserAsManagerOrComanager()
        {
            var TenantId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var flatsandtenants = _context.FlatsAndTenants.Where(m => m.TenantId == TenantId && m.ExpireDate == DateTime.Parse("0001-01-01 12:00:00 AM")).ToList();
            List<Flat> flats = new List<Flat>();
            foreach (var item in _context.Flats)
            {
                foreach (var item1 in flatsandtenants)
                {
                    if (item1.FlatId == item.Id)
                    {
                        flats.Add(item);
                    }
                }
            }
            List<Building> buildings = new List<Building>();

            foreach (var item in flats)
            {
                foreach (var item1 in _context.Buildings)
                {
                    if (item.BuildingId == item1.Id)
                    {
                        if (!buildings.Contains(item1))
                        {
                            buildings.Add(item1);
                        }
                    }
                }
            }

            var chosenBuildings =new List<Building>();
            foreach (var item in buildings)
            {
                if(IsComanagerOfBuilding(item.Id,TenantId.ToString())||IsManagerOfBuilding(item.Id,TenantId.ToString()))
                {
                    chosenBuildings.Add(item);
                }
            }
            return chosenBuildings;
        }

        [HttpGet]
        [Route("GetFlatsOfTenant")]
        public ActionResult<IEnumerable<Flat>> GetFlatsOfTenant()
        {
            var TenantId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var flatsandtenants = _context.FlatsAndTenants.Where(m => m.TenantId == TenantId && m.ExpireDate == DateTime.Parse("0001-01-01 12:00:00 AM")).ToList();
            var flats = new List<Flat>();
            foreach (var item in flatsandtenants)
            {
                if (_context.Flats.Find(item.FlatId) != null)
                {
                    flats.Add(_context.Flats.Find(item.FlatId));
                }
            }
            Console.WriteLine(flats);
            return flats;
        }

        [HttpGet("{tid}/{fid}")]
        [Route("GetThisTenantResidenceDuration/{tid}/{fid}")]
        public ActionResult<int> GetThisTenantResidenceDuration(string tid, int fid )
        {
            var tenantId = Guid.Parse(tid);
            var residences = _context.FlatsAndTenants.Where(m => m.TenantId == tenantId && m.FlatId == fid);
            var ThisTenantResidenceDurationInDays = 0;
            TimeSpan residenceDuration = new TimeSpan();
            foreach (var item in residences)
            {
                if (item.ExpireDate == DateTime.Parse("0001 - 01 - 01 12:00:00 AM"))
                {
                    residenceDuration += DateTime.Now - item.StartDate;
                }
                else
                {
                    residenceDuration += item.ExpireDate - item.StartDate;
                }
            }
            ThisTenantResidenceDurationInDays = residenceDuration.Days;
            return ThisTenantResidenceDurationInDays;
        }


        [HttpGet]
        [Route("GetFlat/{Id}")]
        public ActionResult<Object> GetFlat(int Id)
        {
            var TenantId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString()); 
            bool belongs = _context.FlatsAndTenants.Where(m => m.FlatId == Id && m.TenantId == TenantId && m.ExpireDate== DateTime.Parse("0001-01-01 12:00:00 AM")).Count()>0;
            if (belongs)
            {
                var flat =_context.Flats.FirstOrDefault(m => m.Id == Id);
                var flatBuildingName = _context.Buildings.Find(flat.BuildingId).Name;
                return new
                {
                    flat.Name,
                    flatBuildingName,
                    flat.FloorNumber,
                    flat.UnitNumber

                }; 
            }
            else
            {
                return StatusCode(503);//this flat is not for logged in User
            }

        }
        private Boolean isManagerOfThisBuilding(Guid uId, int bId)
        {
            return _context.BAMs.Where(m => m.BuildingId == bId && m.ManagerId == uId && m.ExpireDate == DateTime.Parse("01/01/0001 00:00:00")).Count() > 0;
        }


        [HttpGet("{bid}/{mid}")]
        [Route("IsManagerOfBuilding/{bid}/{mid}")]
        public bool IsManagerOfBuilding(int bid, string mid)
        {
            var userId = Guid.Parse(mid);
            var result = _context.BAMs.Where(m => m.ManagerId == userId && m.BuildingId == bid && m.ExpireDate == DateTime.Parse("0001-01-01 12:00:00 AM")).ToList().Count() > 0;
            return result;
        }


        [HttpGet("{bid}/{cid}")]
        [Route("IsComanagerOfBuilding/{bid}/{cid}")]
        public bool IsComanagerOfBuilding(int bid, string cid)
        {
            var userid = Guid.Parse(cid);
            if (_context.comanagerandbuilding.ToList().Count == 0)
            {
                return false;
            }
            var result = _context.comanagerandbuilding.Where(m => m.BuildingId == bid && m.ComanagerId == userid && m.ExpireDate == DateTime.Parse("0001-01-01 12:00:00 AM")).ToList();
            if (result.Count() > 0)
                return true;
            else
                return false;
        }


    }
}
