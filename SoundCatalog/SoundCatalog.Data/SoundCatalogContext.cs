using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoundCatalog.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoundCatalog.Data
{
    public class SoundCatalogContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public SoundCatalogContext(DbContextOptions<SoundCatalogContext> options)
            : base(options)
        {

        }
    }
}
