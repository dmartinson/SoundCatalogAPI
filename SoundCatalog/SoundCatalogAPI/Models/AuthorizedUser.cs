using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoundCatalogAPI.Models
{
    public class AuthorizedUser
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
