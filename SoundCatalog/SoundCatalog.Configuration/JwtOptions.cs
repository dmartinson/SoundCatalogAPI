using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace SoundCatalog.Configuration
{
    public class JwtOptions
    {
        // <summary>
        /// "iss" (Issuer) Claim - The "iss" (issuer) claim identifies the principal that issued the JWT.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// "aud" (Audience) Claim - The "aud" (audience) claim identifies the recipients that the JWT is intended for
        /// </summary>
        public string Audience { get; set; }

        public string SecretKey { get; set; }

        public SymmetricSecurityKey Key
        {
            get
            {
                return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            }
        }

        
    }
}
