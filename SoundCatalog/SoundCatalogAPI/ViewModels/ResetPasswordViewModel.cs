using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoundCatalogAPI.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string UserId { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
    }
}
