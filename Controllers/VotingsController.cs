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
    public class VotingsController : ControllerBase
    {
        private readonly AuthenticationContext _context;

        public VotingsController(AuthenticationContext context)
        {
            _context = context;
        }

        // GET: api/Votings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Voting>>> GetVoting()
        {
            return await _context.Voting.ToListAsync();
        }

        //// GET: api/Votings/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Voting>> GetVoting(int id)
        //{
        //    var voting = await _context.Voting.FindAsync(id);

        //    if (voting == null)
        //    {
        //        return NotFound();
        //    }

        //    return voting;
        //}

        // PUT: api/Votings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVoting(int id, Voting voting)
        {
            if (id != voting.Id)
            {
                return BadRequest();
            }

            _context.Entry(voting).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VotingExists(id))
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

        // POST: api/Votings
        [HttpPost]
        public async Task<ActionResult<Voting>> PostVoting(VotingTemplate voting)
        {

            var Voterid = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var vote = new Voting();
            var req = _context.Request.FindAsync(voting.RequestId).Result;
 
            if (req.ExpirationDate > DateTime.Now && ((_context.Voting.Where(m => m.ReqId == voting.RequestId && m.VoterId == Voterid)).ToList().Count() == 0))
            {
                if (voting.Vote == true)
                {
                    req.AgreeVotes++;
                }
                else
                {
                    req.DisAgreeVotes++;
                }
                vote.VoterId = Voterid;
                vote.VoteDate = DateTime.Now;
                vote.Vote = voting.Vote;
                vote.ReqId = req.Id;
                _context.Voting.Add(vote);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else if(req.ExpirationDate<DateTime.Now)
            {
                return StatusCode(419);
            }

            else if ((_context.Voting.Where(m => m.ReqId == voting.RequestId && m.VoterId == Voterid)).ToList().Count() > 0)
            {
                return StatusCode(418);
            }
            

            return Ok();

        }
        //Get Requests for tenant
        [Route("GetRequests")]
        [HttpGet("{Id}")]
        public async Task<ActionResult<IEnumerable<Request>>> GetRequests(int Id)
        {
            return await _context.Request.Where(m => m.BuildingId == Id).ToListAsync();
        }
         //is allowed to vote or not
         [Route("IsAllowed/{Id}")]
         [HttpGet("{Id}")]
         public async Task<IActionResult> GetIsAllowed(int Id)
        {
            if(_context.Request.Where(m=>m.Id==Id).FirstOrDefault().ExpirationDate<DateTime.Now)
            {
                return StatusCode(501);//Dead line is expired
            }
            var id = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            if (_context.Voting.Where(m => m.VoterId == id && m.ReqId == Id).ToList().Count > 0)
            {
                return StatusCode(502);//Already voted

            }
            else
            {
                return StatusCode(200);//Allowed

            }

        }
        // DELETE: api/Votings/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Voting>> DeleteVoting(int id)
        {
            var voting = await _context.Voting.FindAsync(id);
            if (voting == null)
            {
                return NotFound();
            }

            _context.Voting.Remove(voting);
            await _context.SaveChangesAsync();

            return voting;
        }

        [HttpGet]
        [Route("GetAllAvailableVotingsCount")]
        public async Task<int> GetAllAvailableVotings()
        {
            //getting buildings of tenant
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

            //getting all requests of those buildings
            var allReqs = new List<Request>();
            foreach (var bItem in buildings)
            {
                foreach (var rItem in _context.Request)
                {
                    if(rItem.BuildingId == bItem.Id)
                    {
                        allReqs.Add(rItem);
                    }
                }
            }
            //getting allowed reqs
            var allAllowedReqs = new List<Request>();
            allAllowedReqs = allReqs;
            int counter = 0;
            int expiredReqsCounter = allReqs.Where(m => m.ExpirationDate < DateTime.Now).Count();
            int votedReqsCounter = 0;
            
            int votedAndExpired = 0;
            foreach (var rItem in allReqs)
            {
                foreach (var vItem in _context.Voting)
                {
                    if(vItem.ReqId==rItem.Id && vItem.VoterId==TenantId)
                    {
                        votedReqsCounter++;
                    }
                    if (vItem.ReqId == rItem.Id && vItem.VoterId == TenantId && rItem.ExpirationDate < DateTime.Now)
                    {
                        votedAndExpired++;
                    }
                  
                }
            }
            
           

            return allReqs.Count()-(votedReqsCounter+expiredReqsCounter-votedAndExpired);
        }

        //getting allowed reqs to vote for a specific Building
        [HttpGet]
        [Route("GetAllAvailabelVotingsArrayCount")]
        public async Task<object> GetAllAvailabelVotingsArrayCount()
        {
            //getting Buildings of tenant
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
            
            var resultArray = new int[2,buildings.Count()];
            for (int i = 0; i < buildings.Count(); i++)
            {
                resultArray[0,i] = buildings[i].Id;
                resultArray[1, i] = availableVotingsOfThisBuilding(buildings[i].Id);
            }

            return resultArray;

        }


        private bool VotingExists(int id)
        {
            return _context.Voting.Any(e => e.Id == id);
        }
        private int availableVotingsOfThisBuilding(int bId)
        {
            var TenantId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var buildingAllReqs = _context.Requests.Where(m => m.BuildingId == bId);
            int expiredReqsCounter = buildingAllReqs.Where(m => m.ExpirationDate < DateTime.Now).Count();
            int votedReqsCounter = 0;

            int votedAndExpired = 0;
            foreach (var rItem in buildingAllReqs)
            {
                foreach (var vItem in _context.Voting)
                {
                    if (vItem.ReqId == rItem.Id && vItem.VoterId == TenantId)
                    {
                        votedReqsCounter++;
                    }
                    if (vItem.ReqId == rItem.Id && vItem.VoterId == TenantId && rItem.ExpirationDate < DateTime.Now)
                    {
                        votedAndExpired++;
                    }

                }
            }
            return buildingAllReqs.Count() - (votedReqsCounter + expiredReqsCounter - votedAndExpired);
        }

    }
}
