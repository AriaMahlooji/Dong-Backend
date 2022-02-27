using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Classes;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class comanagerandbuildingsController : ControllerBase
    {
        private readonly AuthenticationContext _context;

        public comanagerandbuildingsController(AuthenticationContext context)
        {
            _context = context;
        }

        // GET: api/comanagerandbuildings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<comanagerandbuilding>>> Getcomanagerandbuilding()
        {
            return await _context.comanagerandbuilding.ToListAsync();
        }

        // GET: api/comanagerandbuildings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<comanagerandbuilding>> Getcomanagerandbuilding(int id)
        {
            var comanagerandbuilding = await _context.comanagerandbuilding.FindAsync(id);

            if (comanagerandbuilding == null)
            {
                return NotFound();
            }

            return comanagerandbuilding;
        }

        // PUT: api/comanagerandbuildings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Putcomanagerandbuilding(int id, comanagerandbuilding comanagerandbuilding)
        {
            if (id != comanagerandbuilding.Id)
            {
                return BadRequest();
            }

            _context.Entry(comanagerandbuilding).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!comanagerandbuildingExists(id))
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

        // POST: api/comanagerandbuildings
        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult<comanagerandbuilding>> Postcomanagerandbuilding(cab temp)
        {
            
            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var comanagerandbuildingtemp = new comanagerandbuilding();
            if(!isManagerOfThisBuilding(currentUserId, temp.BuildingId))
            {
                return StatusCode(405);//Not Allowed
            }


            //avoid repetitive comanager
            var res = _context.comanagerandbuilding.Where(m => m.BuildingId == temp.BuildingId && m.ComanagerId == temp.TenantId).ToList();
            var result = true;
            foreach (var item in res)
            {
                if (item.ExpireDate == DateTime.Parse("0001-01-01 12:00:00 AM"))
                {
                    result = false;
                }
            }
            if(result==false)
            {
                return StatusCode(501);//Tekrari Hastesh
            }
            comanagerandbuildingtemp.BuildingId = temp.BuildingId;
            comanagerandbuildingtemp.ComanagerId = temp.TenantId;
            comanagerandbuildingtemp.StartDate = DateTime.Now;
            comanagerandbuildingtemp.Adder = currentUserId;
            _context.comanagerandbuilding.Add(comanagerandbuildingtemp);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost]
        [Route("Remove")]
        public async Task<IActionResult> PostRemove(cab temp)
        {
            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            if (!isManagerOfThisBuilding(currentUserId, temp.BuildingId))
            {
                return StatusCode(405);//Not Allowed
            }
            var res = _context.comanagerandbuilding.Where(m => m.BuildingId == temp.BuildingId && m.ComanagerId == temp.TenantId).ToList();
            foreach (var item in res)
            {
                if(item.ExpireDate==DateTime.Parse("0001-01-01 12:00:00 AM"))
                {
                    item.ExpireDate = DateTime.Now;
                    _context.SaveChanges();
                    return StatusCode(200);
                }
            }
            return StatusCode(502);

        }


        // DELETE: api/comanagerandbuildings/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<comanagerandbuilding>> Deletecomanagerandbuilding(int id)
        {
            var comanagerandbuilding = await _context.comanagerandbuilding.FindAsync(id);
            if (comanagerandbuilding == null)
            {
                return NotFound();
            }

            _context.comanagerandbuilding.Remove(comanagerandbuilding);
            await _context.SaveChangesAsync();

            return comanagerandbuilding;
        }

        private bool comanagerandbuildingExists(int id)
        {
            return _context.comanagerandbuilding.Any(e => e.Id == id);
        }
        private Boolean isManagerOfThisBuilding(Guid uId,int bId)
        {
            return _context.BAMs.Where(m => m.BuildingId == bId && m.ManagerId == uId && m.ExpireDate == DateTime.Parse("01/01/0001 00:00:00")).Count() > 0;
        }
        private Boolean isCoManagerOfThisBuilding(Guid uId, int bId)
        {
            return _context.comanagerandbuilding.Where(m => m.BuildingId == bId && m.ComanagerId == uId && m.ExpireDate == DateTime.Parse("01/01/0001 00:00:00")).Count() > 0;
        }
    }
}
