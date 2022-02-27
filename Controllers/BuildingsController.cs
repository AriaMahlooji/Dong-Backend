using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Classes;
using WebAPI.Models;



namespace WebAPI.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class BuildingsController : ControllerBase
    {

    private UserManager<ApplicationUser> _userManager;
    private SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationSettings _appSettings;
        private readonly AuthenticationContext _context;

        public BuildingsController(UserManager<ApplicationUser>userManager,SignInManager<ApplicationUser>signInManager, AuthenticationContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }


        [HttpPost]
        [Authorize]
      
        public async Task<Object>PostBuilding(BuildingAndManagerTemplate model)
        {
            var id = User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString();
            var building = new Building();
            building.More = model.More;
            building.Name = model.Name;
            building.Postalcode = model.Postalcode;
            building.Street = model.Street;
            building.City = model.City;
            var result = await _context.Buildings.AddAsync(building);
            await _context.SaveChangesAsync();
            Building TempBuild = _context.Buildings.LastOrDefault<Building>();
            var tempforboth = new BuildingAndManager();
            tempforboth.ManagerId = Guid.Parse(id);
            tempforboth.BuildingId = TempBuild.Id;
            tempforboth.MonthlyCharge = model.MonthlyCharge;
            tempforboth.StartDate = DateTime.Now;

            var tempManager = new ApplicationUser();
            tempManager = await _userManager.FindByIdAsync(id);
            var resultt = await _userManager.IsInRoleAsync(tempManager, "Manager");
            if (!resultt)
            {
                await _userManager.AddToRoleAsync(tempManager, "Manager");
            }
            await _context.SaveChangesAsync();
            await _context.BAMs.AddAsync(tempforboth);    
            // BAMs is BuildingAndManager table
            await _context.SaveChangesAsync();
           
            return Ok();           
        }

        // GET: api/Buildings
        [HttpGet]
        [Route("GetBuildings")]
        public async  Task<ActionResult<IEnumerable<Building>>> GetBuildings()
        {
            var id = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());

            var temp1 = _context.BAMs.Where(m => m.ManagerId == id && m.ExpireDate == DateTime.Parse("0001 - 01 - 01 12:00:00 AM")).Select(m => m.BuildingId).ToList(); //List Of buildings that user is manager of
            var temp2 = _context.comanagerandbuilding.Where(m => m.ComanagerId == id && m.ExpireDate == DateTime.Parse("0001 - 01 - 01 12:00:00 AM")).Select(m => m.BuildingId).ToList(); // List Of Buildings that user is co-Manager of


            //removing repetetive buildings
            foreach (var item in temp2)
            {
                if(!temp1.Contains(item))
                {
                    temp1.Add(item);
                }
            } 

            List<Building> returnList = new List<Building>();
            foreach (var item in _context.Buildings)
            {
                foreach (var item1 in temp1)
                {
                    if(item1==item.Id && item.Name !=null)
                    {
                        returnList.Add(item);
                    }
                }
            }
            return returnList;

       
        }
        [HttpGet("{id}")]
        [Route("TenantOfBuilding/{id}")]
        public async Task<ActionResult<IEnumerable<TenantDetail>>> GetTenantOfBuilding(int id)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());

           
           // This task is really time consuming as the database is excessively normalized
           // It would be better to have a List of FlatAndTenant In Building Class

            List<TenantDetail> Tenants = new List<TenantDetail>();
            List<Flat> Flats = new List<Flat>();
            foreach (var item in _context.Flats)
            {
                if(item.BuildingId==id)
                {
                    
                    foreach (var item1 in _context.FlatsAndTenants)
                    {
                        if(item1.FlatId==item.Id )
                        {
                            var td = new TenantDetail();
                            td.Id = item1.TenantId;
                            
                            td.FloorNumber = item.FloorNumber;
                            var user = _context.ApplicationUsers.FirstOrDefault(m => m.Id == td.Id.ToString());
                            td.FullName = user.Fullname;
                            td.UnitName = item.Name;
                            td.UnitNumber = item.UnitNumber;
                            td.BuildingId = id;
                            td.FlatId = item.Id;
                            td.ResidenceDurationInDays = GetResidenceDuration(td.Id, td.FlatId);
                            if (item1.ExpireDate != DateTime.Parse("0001-01-01 12:00:00 AM"))
                            {
                                td.IsActive = false;
                            }
                            else
                                td.IsActive = true;
                            td.IsComanager = false;
                            td.IsManager = false;
                            if(_context.BAMs.Where(m=>m.BuildingId == td.BuildingId && m.ManagerId == td.Id && m.ExpireDate == DateTime.Parse("0001-01-01 12:00:00 AM")).Count()>0)
                            {
                                td.IsManager = true;
                            }
                            if(_context.comanagerandbuilding.Where(m=>m.BuildingId == td.BuildingId &&m.ComanagerId==td.Id && m.ExpireDate == DateTime.Parse("0001-01-01 12:00:00 AM")).Count()>0)
                            {
                                td.IsComanager = true;
                            }

                            Tenants.Add(td);
                        }
                    }
                }
                
            }
            return Tenants.OrderByDescending(m=>m.IsActive==true).ToList();
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
        [Route("ComanagerOfBuilding/{bid}/{cid}")]
        public  bool IsComanagerOfBuilding(int bid,string cid)
        {
            var userid = Guid.Parse(cid);
            if(_context.comanagerandbuilding.ToList().Count==0)
            {
                return false;
            }
            var result = _context.comanagerandbuilding.Where(m => m.BuildingId == bid && m.ComanagerId==userid && m.ExpireDate==DateTime.Parse("0001-01-01 12:00:00 AM")).ToList();
            if (result.Count() > 0)
                return true;
            else
                return false;
        }
        [HttpGet("{bid}")]
        [Route("GetBuildingName/{bid}")]
        public string GetBuildingName (int bid)
        {

            return _context.Buildings.FirstOrDefault(m => m.Id == bid).Name;
        }

        [HttpGet]
        [Route("IsManagerOrComanager")]

        public object IsManagerOrCoManager()
        {
            var id = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            if((_context.BAMs.Where(m=>m.ManagerId == id && m.ExpireDate == DateTime.Parse("0001 - 01 - 01 12:00:00 AM")).Count()>0 ||  _context.comanagerandbuilding.Where(m=>m.ComanagerId == id && m.ExpireDate == DateTime.Parse("0001-01-01 12:00:00 AM")).ToList().Count() > 0))
            {
                return new
                {
                    isManagerOrComanager = true
                };
            }
            else
            {
                return new
                {
                    isManagerOrComanager = false
                };
            }
        }

        private int GetResidenceDuration(Guid tid, int fid)
        {


            var residences = _context.FlatsAndTenants.Where(m => m.TenantId == tid && m.FlatId == fid);
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



    }

    

}