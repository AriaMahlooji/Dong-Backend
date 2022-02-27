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
    public class StatisticsController : ControllerBase
    {
        private readonly AuthenticationContext _context;

        public StatisticsController(AuthenticationContext context)
        {
            _context = context;
        }

        // GET: api/Statistics
        [HttpGet]
        public async Task<ActionResult<Statistic>> GetStatistic()
        {
            var temp = new Statistic();
            temp.UserCount = _context.ApplicationUsers.Count();
            temp.TenantCount = _context.FlatsAndTenants.Count();
            temp.ManagerCount = _context.BAMs.Count();
            temp.CoManagerCount = _context.comanagerandbuilding.Count();
            temp.BuildingCount = _context.Buildings.Count();
            temp.FlatCount = _context.FlatsAndTenants.Count();
            temp.RequestCount = _context.Request.Count();
            temp.VoteCount = _context.Voting.Count();
            return temp;
        }
        [HttpGet]
        [Route("GetBarChartData")]
        public async Task<List<int>> GetBarChartData()
        {
            int buildingsNo = _context.Buildings.Count();
            int tenantsNo = _context.FlatsAndTenants.Count();
            int expiredTenantsNo = _context.FlatsAndTenants.Where(m => m.ExpireDate != DateTime.Parse("0001 - 01 - 01 12:00:00 AM")).Count();
            int flatsNumber = _context.Flats.Count();
            int votingsNumber = _context.Voting.Count();

            var barChartData = new List<int>();
            barChartData.Add(buildingsNo);
            barChartData.Add(tenantsNo);
            barChartData.Add(expiredTenantsNo);
            barChartData.Add(flatsNumber);
            barChartData.Add(votingsNumber);
            return barChartData;
        }

        // GET: api/Statistics/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Statistic>> GetStatistic(int id)
        {
            var statistic = await _context.Statistic.FindAsync(id);

            if (statistic == null)
            {
                return NotFound();
            }

            return statistic;
        }

        // PUT: api/Statistics/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStatistic(int id, Statistic statistic)
        {
            if (id != statistic.Id)
            {
                return BadRequest();
            }

            _context.Entry(statistic).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StatisticExists(id))
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

        // POST: api/Statistics
        [HttpPost]
        public async Task<ActionResult<Statistic>> PostStatistic(Statistic statistic)
        {
            _context.Statistic.Add(statistic);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStatistic", new { id = statistic.Id }, statistic);
        }

        // DELETE: api/Statistics/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Statistic>> DeleteStatistic(int id)
        {
            var statistic = await _context.Statistic.FindAsync(id);
            if (statistic == null)
            {
                return NotFound();
            }

            _context.Statistic.Remove(statistic);
            await _context.SaveChangesAsync();

            return statistic;
        }

        [HttpGet]
        [Route("GetDoughnutChartData")]
        public async Task<List<int>> GetDoughnutChartData()
        {
            int agreeVotes = 0;
            int disAgreeVotes = 0;
            
            foreach (var item in _context.Voting)
            {
                if(item.Vote == true)
                {
                    agreeVotes++;
                }
                else
                {
                    disAgreeVotes++;
                }
            }
            var result = new List<int>();
            result.Add(agreeVotes);
            result.Add(disAgreeVotes);
            return result;
        }

        private bool StatisticExists(int id)
        {
            return _context.Statistic.Any(e => e.Id == id);
        }
    }
}
