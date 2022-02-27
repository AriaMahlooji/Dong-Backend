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
    public class RequestController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        private UserManager<ApplicationUser> _userManager;


        public RequestController(AuthenticationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Request/BuildingReqList/5
        [Route("BuildingReqList/{id}")]
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Request>>> GetBuildingReqList(int id)
        {
            var Id = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());

            // return _context.Request.Where(m => m.BuildingId == id).ToList();
            var list = _context.Request.Where(m => m.BuildingId == id).ToList();
            foreach (var item in list)
            {
                if (item.ExpirationDate < DateTime.Now)
                {
                    item.IsAllowed = false;
                }
                else
                    item.IsAllowed = true;
            }
            foreach (var item in list)
            {
               
                foreach (var item1 in _context.Voting)
                {
                    if(item1.ReqId ==item.Id)
                    {
                        if( item1.VoterId == Id)
                        {
                            item.IsAllowed = false;
                        }
                       
                    }
                }
            }

            return list.OrderByDescending(d=>d.IsAllowed==true).ToArray();
        }

        // GET: api/Request/5
        [Route("ReqResults/{id}")]
        [HttpGet("{id}")] 
        public async Task<ActionResult<IEnumerable<string>>> GetReqResults(int id)
        {
            var Id = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var request = new Request();
            if(id > 0)
            {
               request = _context.Requests.FirstOrDefault(m => m.Id == id);
            }
            else
            {
               request = _context.Requests.FirstOrDefault(m => m.Id == -id);
            }

            //If the request is private the requlat tenants of building(neither manager or comanager) are not allowed to see name of voters; 
            if(request.IsPrivate == true && !isManagerOrComanagerOf(request.BuildingId,Id))
            {
                return StatusCode(518);
            }


            List<Voting> Reqs = new List<Voting>();
            if (id > 0)
            {
                 Reqs = _context.Voting.Where(m => m.ReqId == id && m.Vote == true).ToList();
            }
            else if(id < 0)
            {
                id = -id;
                Reqs = _context.Voting.Where(m => m.ReqId == id && m.Vote == false).ToList();

            }
            List<string> Names = new List<string>();
            foreach (var item in _userManager.Users)
            {
                foreach (var item1 in Reqs)
                {
                    if(item1.VoterId==Guid.Parse(item.Id))
                    {
                        Names.Add(item.Fullname);  
                    }
                }
            }

          
            return  Names;
            
        }

        // PUT: api/Request/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequest(int id, Request request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            _context.Entry(request).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequestExists(id))
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

        // POST: api/Request
        [HttpPost]
        public async Task<ActionResult<Request>> PostRequest(RequestTemplate request)
        {
            var RequestorId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var NewReq = new Request();
            NewReq.BuildingId = request.BuildingId;
            NewReq.Abstract = request.Abstract;
            NewReq.Context = request.Context;
            NewReq.IsPrivate = request.IsPrivate;
            NewReq.RequestedDate = DateTime.Now;
            NewReq.Price = request.Price;
            NewReq.RequestorId = RequestorId;
            NewReq.ExpirationDate = DateTime.Now.AddDays(request.DeadLine);
            NewReq.MinutesRemained = 0;
            NewReq.DaysRemained = 0;
      
            _context.Request.Add(NewReq);
           await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Request/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Request>> DeleteRequest(int id)
        {
            var request = await _context.Request.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            _context.Request.Remove(request);
            await _context.SaveChangesAsync();

            return request;
        }

        private bool RequestExists(int id)
        {
            return _context.Request.Any(e => e.Id == id);
        }
        private bool isManagerOrComanagerOf(int buildingId, Guid userId)
        {
            Boolean result = false;
            if(_context.BAMs.Where(m=>m.BuildingId == buildingId && m.ExpireDate == DateTime.Parse("0001 - 01 - 01 12:00:00 AM") && m.ManagerId == userId).Count()>0)
            {
                result = true;
            }
            if(_context.comanagerandbuilding.Where(m=> m.BuildingId == buildingId && m.ExpireDate == DateTime.Parse("0001 - 01 - 01 12:00:00 AM") && m.ComanagerId == userId).Count() > 0)
            {
                result = true;
            }
            return result;
        }
    }
}
