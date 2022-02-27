using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Classes;

namespace WebAPI.Models
{
    public class AuthenticationContext:IdentityDbContext

    {
        public AuthenticationContext(DbContextOptions options):base(options)
        {

        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Building> Buildings { get; set; }

        
        public DbSet<TenantInvitation> Invitations { get; set; }
        public DbSet<TenantTeporaryInvitation> TenantTeporaryInvitations { get; set; }
        // public DbSet<BuildingsAndManager> BuildingsAndManagers { get; set; }
        public DbSet<BuildingAndManager> BAMs { get; set; }
        public DbSet<Flat> Flats { get; set; }
        public DbSet<FlatAndTenant> FlatsAndTenants { get; set; }
        public DbSet<Request> Request { get; set; }
        public DbSet<WebAPI.Models.Voting> Voting { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<WebAPI.Models.comanagerandbuilding> comanagerandbuilding { get; set; }

        public DbSet<WebAPI.Classes.Statistic> Statistic { get; set; }
        public DbSet<Test> Tests { get; set; }

        public DbSet<FailedLoginAttempt> failedLoginAttempts { get; set; }




    }
}
