using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web.Responses;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using Agit.Common;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TAMHR.ESS.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //protected UserService UserService => ServiceProxy.GetService<UserService>();

        [HttpPost("token")]
        public ActionResult GetToken(string securityKey)
        {
            //security key
            //securityKey = "this is Secret key for authentication TAMHR ESS API";

            //symetric security key
            var symetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

            //signing credentials
            var signingCredentials = new SigningCredentials(symetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

            //add claims
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Role, "HR Administrator"));
            //var claims = UserService.GetClaims("rahadian", "Transformer6hap");
            //var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            

            //create token
            var token = new JwtSecurityToken(
                issuer: "rahadian",
                audience: "rahadian",
                expires: DateTime.Now.AddDays(1),
                signingCredentials: signingCredentials,
                claims: claims                
                );

            //return token
            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            //return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties());
        }
    }
}
