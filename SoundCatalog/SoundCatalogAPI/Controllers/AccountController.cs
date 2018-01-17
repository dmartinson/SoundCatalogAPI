using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SoundCatalog.Configuration;
using SoundCatalog.Models;
using SoundCatalog.Services;
using SoundCatalog.Web.ViewModels;
using SoundCatalogAPI.ViewModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoundCatalogAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("PolicySoundCatalogUI")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtOptions _jwtConfiguration;
        private readonly ClientOptions _clientConfiguration;
        private readonly IMessageServices _messageServices;


        #region Constructor

        public AccountController(
                ILogger<AccountController> logger,
                SignInManager<ApplicationUser> signInManager,
                UserManager<ApplicationUser> userManager,
                IOptions<JwtOptions> jwtConfiguration,
                IOptions<ClientOptions> clientConfiguration,
                IMessageServices messageServices)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtConfiguration = jwtConfiguration.Value;
            _clientConfiguration = clientConfiguration.Value;
            _messageServices = messageServices;
        }

        #endregion
                
        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<ActionResult> LoginAsync([FromBody]LoginViewModel loginModel)
        {
            var user = await this._userManager.FindByEmailAsync(loginModel.Email);
            if (user != null)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return BadRequest();
                }
                var result = await this._signInManager.CheckPasswordSignInAsync(user, loginModel.Password, false);
                if (result.Succeeded)
                {
                    var claims = new[]
                    {
                      new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    var creds = new SigningCredentials(this._jwtConfiguration.Key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        this._jwtConfiguration.Issuer,
                        this._jwtConfiguration.Audience,
                        claims,
                        expires: DateTime.Now.AddMinutes(30),
                        signingCredentials: creds);

                    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
                }
            }

            return Unauthorized();
        }

        [AllowAnonymous]
        [Route("isValidNewMail")]
        [HttpGet]
        public async Task<ActionResult> IsValidNewMail(string email)
        {
            var user = await this._userManager.FindByEmailAsync(email);
            return Ok(user == null);
        }

        //
        // POST: /Account/Register
        [AllowAnonymous]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Password = model.Password
            };
            var result = await this._userManager.CreateAsync(user, user.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {userName} was created.", model.Email);

                await this.SendConfirmEmailAsync(user);
                
                return this.Ok();
            }
            return this.BadRequest(result.Errors.ToList().First().Description);
        }

        //
        // GET: /Account/GenerateConfirmEmail
        [AllowAnonymous]
        [Route("generateconfirmemail")]
        [HttpGet]
        public async Task<ActionResult> GenerateConfirmEmail(string email)
        {
            var user = await this._userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return BadRequest();
            }

            await this.SendConfirmEmailAsync(user);

            return Ok();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        [Route("confirmemail")]
        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest();
            }

            var user = await this._userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest();
            }

            //Hack: The code generate when is processed by querystring, it replace '+' symbol with ' '"
            code = code.Replace(" ", "+");
            var result = await this._userManager.ConfirmEmailAsync(user, code);
            
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest();
        }

        //
        // POST: /Account/ResetPassword
        [AllowAnonymous]
        [Route("resetpassword")]
        [HttpPost]
        public async Task<ActionResult> ResetPassword([FromBody]ResetPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Code))
            {
                return BadRequest();
            }

            var user = await this._userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return BadRequest();
            }

            //Hack: The code generate when is processed by querystring, it replace '+' symbol with ' '"
            //code = code.Replace(" ", "+");
            var result = await this._userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
            {
                return Ok();
            }
            return this.BadRequest(result.Errors.ToList().First().Description);
        }       

        //
        // GET: /Account/GeneratePwdRecoveryEmail
        [AllowAnonymous]
        [Route("generatepwdrecoveryemail")]
        [HttpGet]
        public async Task<ActionResult> GeneratePasswordRecoveryEmail(string email)
        {
            var user = await this._userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return BadRequest();
            }

            await this.SendPasswordRecoveryEmailAsync(user);

            return Ok();
        }

        //
        // GET: /Account/TestAuth
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("test")]
        [HttpGet]
        public IActionResult TestAuth()
        {
            return this.Ok();
        }
        #region Private Methods

        private async Task SendConfirmEmailAsync(ApplicationUser user)
        {
            // Send an email with this link
            string code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = string.Format(_clientConfiguration.UrlVerifyEmail, user.Id, code);
            var uri = new Uri(callbackUrl);

            //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id,  code = code }, protocol: HttpContext.Request.Scheme);
            await _messageServices.SendEmailAsync(user.Email, "Confirm your account",
                "Please confirm your account by clicking this link: <a href=\"" + uri.AbsoluteUri + "\">link</a>");

        }

        private async Task SendPasswordRecoveryEmailAsync(ApplicationUser user)
        {
            // Send an email with this link
            string code = await this._userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = string.Format(_clientConfiguration.UrlResetPassword, user.Id, code);
            var uri = new Uri(callbackUrl);

            await _messageServices.SendEmailAsync(user.Email, "Reset your password",
                "Please confirm your password recovery by clicking this link: <a href=\"" + uri.AbsoluteUri + "\">link</a>");

        }

        #endregion
    }
}
