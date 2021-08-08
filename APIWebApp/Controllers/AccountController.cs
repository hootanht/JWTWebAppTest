using APIWebApp.Context;
using APIWebApp.User;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace APIWebApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        #region Dependency Injections
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly APIWebAppDbContext _dbContext;
        #endregion

        #region Constructor
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, APIWebAppDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }
        #endregion


        #region Users

        /// <summary>
        /// Sign in user with username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SignInUser([FromForm] string username, [FromForm] string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null && await _userManager.CheckPasswordAsync(user, password))
                {
                    var result = await _signInManager.PasswordSignInAsync(username, password, true, false);
                    if (result.Succeeded)
                    {
                        var claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.NameIdentifier,user.Id),
                            new Claim(ClaimTypes.Name,user.UserName),
                            new Claim(ClaimTypes.Email,user.Email),
                        };
                        await _userManager.AddClaimsAsync(user, claims);
                        //await _signInManager.SignInAsync(user, true);
                    }
                    var tokenString = GenerateJSONWebToken(User.Claims);
                    return Ok(new { token = tokenString });
                }
            }
            return NotFound();
        }

        /// <summary>
        /// Create user with username , email and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser(string username, string email, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var user = new AppUser()
                {
                    Email = email,
                    UserName = username
                };
                var resultCreatedUser = await _userManager.CreateAsync(user, password);
                return Ok(user.Id);
            }
            return NotFound();
        }

        /// <summary>
        /// Check is user login or not
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public IActionResult IsUserLogIn(string username)
        {
            if (!string.IsNullOrEmpty(username) && User.Identity.IsAuthenticated && User.FindFirstValue(ClaimTypes.Name) == username)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(userId);
            }
            return NotFound();
        }

        /// <summary>
        /// Generate JWT with HmacSha256 Algorithm
        /// </summary>
        /// <param name="claims">Send user claims for insert in JWT</param>
        /// <returns></returns>
        private string GenerateJSONWebToken(IEnumerable<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}
