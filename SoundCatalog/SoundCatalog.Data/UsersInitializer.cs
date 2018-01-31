using Microsoft.AspNetCore.Identity;
using SoundCatalog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoundCatalog.Crosscutting.Constants.Constants;

namespace SoundCatalog.Data
{
    public class UsersInitialize
    {
        private SoundCatalogContext _context;

        public UsersInitialize(SoundCatalogContext context)
        {
            this._context = context;
        }

        public async Task Seed()
        {
            if (!_context.Roles.Any())
            {
                var adminRole = new IdentityRole { Id = "Admin", Name = Roles.Administrator };
                var contributorRole = new IdentityRole { Id = "Contributor", Name = Roles.Contributor };
                var guestRole = new IdentityRole { Id = "Guest", Name = Roles.Guest };

                await _context.Roles.AddRangeAsync(adminRole, contributorRole, guestRole);

                await _context.SaveChangesAsync();
            }

            if (!_context.Users.Any())
            {
                var adminRole = _context.Roles.Single(x => x.Id == "Admin");

                var adminUser1 = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "jmf.stk@gmail.com",
                    FirstName = "Jose",
                    LastName = "Martinez Fuentes",
                    Password = "Jose.1981",
                    Email = "jmf.stk@gmail.com",
                    EmailConfirmed = true
                };
                var adminUser2 = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "diego.martinez.alonso@hotmail.com",
                    FirstName = "Diego",
                    LastName = "Martinez",
                    Password = "Q1w2e3r4t5!.1",
                    Email = "diego.martinez.alonso@hotmail.com",
                    EmailConfirmed = true
                };

                var adminUser1WithAdminRole = new IdentityUserRole<string> { UserId = adminUser1.Id, RoleId = adminRole.Id };
                var adminUser2WithAdminRole = new IdentityUserRole<string> { UserId = adminUser2.Id, RoleId = adminRole.Id };

                await _context.Users.AddRangeAsync(adminUser1, adminUser2);
                await _context.UserRoles.AddRangeAsync(adminUser1WithAdminRole, adminUser2WithAdminRole);

                await _context.SaveChangesAsync();
            }
        }
    }
}
