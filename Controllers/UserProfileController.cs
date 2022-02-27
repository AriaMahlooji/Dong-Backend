using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly AuthenticationContext _context;

        public UserProfileController(UserManager<ApplicationUser> userManager, AuthenticationContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        

        
        [HttpGet]
        [Authorize]
        //Get :/api/UserProfile
        public async Task<Object> GetUserProfile()
        {
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            var flatsId = _context.FlatsAndTenants.Where(m => m.TenantId == Guid.Parse(userId)).Select(m => m.FlatId);
            var flats = new List<Flat>();
            var buildings = new List<Building>();

            foreach (var item in flatsId)
            {
                flats.Add((Flat)_context.Flats.FirstOrDefault(m => m.Id == item));
            }
            foreach (var item in flats)
            {
                if (!buildings.Contains((Building)_context.Buildings.FirstOrDefault(m => m.Id == item.BuildingId)))
                {
                    buildings.Add((Building)_context.Buildings.FirstOrDefault(m => m.Id == item.BuildingId));
                }
            }

            

            var user = await _userManager.FindByIdAsync(userId);
            return new
            {
                user.Fullname,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.SSN,
                buildingNames= buildings.Select(m=>m.Name)
            };
        }

        [HttpGet]
        [Authorize(Roles ="Manager")]
        [Route("ForManager")]
        public string   GetForManager()
        {
            return "Just Manager";
        }

    }
}