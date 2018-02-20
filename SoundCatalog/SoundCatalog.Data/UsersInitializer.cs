using Microsoft.AspNetCore.Identity;
using SoundCatalog.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static SoundCatalog.Crosscutting.Constants.Constants;

namespace SoundCatalog.Data
{
    public class UsersInitialize
    {
        private readonly SoundCatalogContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersInitialize(
            SoundCatalogContext context,
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            this._context = context;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task Seed()
        {
            if (!_context.Roles.Any())
            {
                var adminRole = new IdentityRole { Id = "Admin", Name = Roles.Administrator };
                var contributorRole = new IdentityRole { Id = "Contributor", Name = Roles.Contributor };
                var guestRole = new IdentityRole { Id = "Guest", Name = Roles.Guest };

                await this._roleManager.CreateAsync(adminRole);
                await this._roleManager.CreateAsync(contributorRole);
                await this._roleManager.CreateAsync(guestRole);
            }

            if (!_context.Users.Any())
            {
                var adminUser1 = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "jmf.stk@gmail.com",
                    FirstName = "Jose",
                    LastName = "Martinez Fuentes",
                    Password = "Jose.1981",
                    Email = "jmf.stk@gmail.com",
                    EmailConfirmed = true,
                    TwoFactorEnabled = false
                };
                var adminUser2 = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "diego.martinez.alonso@hotmail.com",
                    FirstName = "Diego",
                    LastName = "Martinez",
                    Password = "Q1w2e3r4t5!.1",
                    Email = "diego.martinez.alonso@hotmail.com",
                    EmailConfirmed = true,
                    TwoFactorEnabled = false
                };

                await this._userManager.CreateAsync(adminUser1, adminUser1.Password);
                await this._userManager.CreateAsync(adminUser2, adminUser2.Password);

                await this._userManager.AddToRoleAsync(adminUser1, Roles.Administrator);
                await this._userManager.AddToRoleAsync(adminUser2, Roles.Administrator);
            }
        }
    }
}
