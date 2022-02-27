using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;
using WebAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using WebAPI.Classes;

using Kavenegar;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationSettings _appSettings;
        private readonly AuthenticationContext _context;

        public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,IOptions<ApplicationSettings>appSettings, AuthenticationContext context)
        {

            //dependency injection
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
            _context = context;
        }
        [HttpPost]
        [Route("Register")]
        //Post : /api/ApplicationUser/Register
        public async Task<Object> PostApplicationUser( ApplicationUserModel  model)

        {
            //Preparing message (only useful for Iran)

            //var sender = "1000596446";
            //var receptor = "09123553866";
            //var message = ".وب سرویس پیام کوتاه کاوه نگار";
            //var api = new kavenegar.KavenegarApi("6A4B7943505448744C67564474574B6D744356724E776B4B34544847724A6879");
            //api.Send(sender, receptor, message);
            //var sender = "10007000600050";
            //var receptor = model.Phonenumber;
            //var Message = "Welcome To My Website "+Environment.NewLine+"Developed By @ClassicMan97";
            //var api = new KavenegarApi("6A4B7943505448744C67564474574B6D744356724E776B4B34544847724A6879");

            
            var applicationuser = new ApplicationUser();
            applicationuser.Email = model.Email;
            applicationuser.Fullname = model.Fullname;
            applicationuser.UserName = model.Username;
            applicationuser.PhoneNumber = model.Phonenumber;
            applicationuser.SSN = model.SSN; 
            

            //sending Email

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Building Charge", "buildingcharge@gmail.com"));
            message.To.Add(new MailboxAddress(model.Username, model.Email));
            message.Subject = "Welcome To Our Website";
            message.Body = new TextPart("plain")
            {
                Text = "Dear " + model.Fullname +  ",you have Registered Successfully .This Website is developed By @ClassicMan97"
            };
            try
                //try to add user (user registration)
            {
                var result = await _userManager.CreateAsync(applicationuser, model.Password);
                await _userManager.AddToRoleAsync(applicationuser, "User");

                if (result.Succeeded)
                {
                    try
                    {
                        // if user is registered successfully we will send him/her an email
                        using (var client = new SmtpClient())
                        {
                            client.Connect("smtp.gmail.com", 587, false);
                            client.Authenticate("buildingcharge", "hamedan8");
                            client.Send(message);
                            client.Disconnect(true);
                        }
                    }
                    catch(Exception e)
                    {
                        _userManager.DeleteAsync(applicationuser);
                        _userManager.RemoveFromRoleAsync(applicationuser, "User");
                        throw e;
                    }
                    
                }
               // if (result.Succeeded)
                //sending the message
               // {
                    // await api.Send(sender, receptor, Message); 
               
               // }


                return Ok(result);
            }
            catch(Exception ex)
            {
                throw ex; 
            }
        }

        ////Post : /api/ApplicationUser/Login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            
            if(user == null )
            {
                return StatusCode(501);//Username not Found
            }
            var failedLoginAttempts = _context.failedLoginAttempts.FirstOrDefault(m => m.UserId == Guid.Parse(user.Id));
            if (failedLoginAttempts != null && failedLoginAttempts.FailedAttemptsCounter == 5 && failedLoginAttempts.BlockedUntil > DateTime.Now)
            {
                //User is blocked and must wait for it to be unblocked
                var  mustWaitInMinutes =((failedLoginAttempts.BlockedUntil - DateTime.Now).TotalMinutes);
                int waitPeriod =Decimal.ToInt16((Math.Ceiling( decimal.Parse(mustWaitInMinutes.ToString()))));
                return StatusCode(507, new { waitPeriod });
            }


            if (failedLoginAttempts != null && failedLoginAttempts.FailedAttemptsCounter == 5 && failedLoginAttempts.BlockedUntil < DateTime.Now)
            {
                failedLoginAttempts.FailedAttemptsCounter = 0;
            }

                await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
            
                IdentityOptions _options = new IdentityOptions();
                var roles = await _userManager.GetRolesAsync(user);
                while(roles.Count!=5)
                {
                    roles.Add("Bikar");
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {

                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID", user.Id.ToString()),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, roles[0]),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, roles[1]),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, roles[2]),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, roles[3]),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, roles[4]),
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890123456")), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                var attemptsInfoOfThisUser = _context.failedLoginAttempts.FirstOrDefault(m => m.UserId == Guid.Parse(user.Id));
                if (attemptsInfoOfThisUser != null)
                {
                    attemptsInfoOfThisUser.BlockedUntil = DateTime.Parse("0001 - 01 - 01 12:00:00 AM");
                    attemptsInfoOfThisUser.FailedAttemptsCounter = 0;
                }
                await _context.SaveChangesAsync();
                return Ok(new { token });
            }
            else
            {
                var attemptsInfoOfThisUser=_context.failedLoginAttempts.FirstOrDefault(m => m.UserId == Guid.Parse(user.Id));
                if (attemptsInfoOfThisUser != null)
                {
                    attemptsInfoOfThisUser.FailedAttemptsCounter++;
                    if (attemptsInfoOfThisUser.FailedAttemptsCounter >= 5)
                    {
                        attemptsInfoOfThisUser.BlockedUntil = DateTime.Now + new TimeSpan(0, 30, 0);//User is blocked for 30 minutes;
                        await _context.SaveChangesAsync();
                        return StatusCode(508, new { mustWaitInMinutes = 30 });//Now User Is Blocked For 30 minutes
                    }
                    if (attemptsInfoOfThisUser.FailedAttemptsCounter < 5)
                    {
                        //Warning only limitted number of failed login attempts are allowed
                        
                        await _context.SaveChangesAsync();
                        return StatusCode(509, new { failedCounter = attemptsInfoOfThisUser.FailedAttemptsCounter });
                    }
                }
                else
                {
                    //has never had a failed login attempt and this is first time.
                    var failedLoginAttemptTempt = new FailedLoginAttempt();
                    failedLoginAttemptTempt.UserId = Guid.Parse(user.Id);
                    failedLoginAttemptTempt.FailedAttemptsCounter++;
                    
                    _context.failedLoginAttempts.Add(failedLoginAttemptTempt);
                    await _context.SaveChangesAsync();
                    return StatusCode(510, new { failedCounter = 1 });
                }
                await _context.SaveChangesAsync();
                
                return StatusCode(501);//Invalid Username and Password
            }
        }
        [HttpGet]
        public async Task<string> GetCurrentUserId()
        {
            ApplicationUser usr = await GetCurrentUserAsync();
            return usr?.Id;
        }

        [HttpPost]
        [Route("CheckMyPassword/{Password}")]
        public async Task<Boolean> IsPasswordCorrect(String password)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString());
            var user =await _userManager.FindByIdAsync(userId.ToString());
            return await _userManager.CheckPasswordAsync(user, password);
        }
        [HttpPut]
        [Route("EditUserProfile")]
        public async Task<object> EditUserProfile(ApplicationUserModel model)
        {
            var userId = User.Claims.FirstOrDefault(m => m.Type == "UserID").Value.ToString();
            ApplicationUser applicationUser =await _userManager.FindByIdAsync(userId);
            applicationUser.Email = model.Email;
            applicationUser.PasswordHash = _userManager.PasswordHasher.HashPassword(applicationUser, model.Password);
            applicationUser.PhoneNumber = model.Phonenumber;
            return await _userManager.UpdateAsync(applicationUser);        
        }


        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);


    }
}