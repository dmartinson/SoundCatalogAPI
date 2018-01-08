using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoundCatalog.Services
{
    public interface IMessageServices
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
