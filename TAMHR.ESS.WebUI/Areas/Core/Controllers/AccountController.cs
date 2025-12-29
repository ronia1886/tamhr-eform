using Agit.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using TAMHR.ESS.Infrastructure.Web.Responses;
using TAMHR.ESS.WebUI.Areas.OHS;
using TAMHR.ESS.WebUI.Areas.OHS.ChatHub;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for user authentication
    /// </summary>
    [Route("api/account")]
    public class AccountApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// User service
        /// </summary>
        protected UserService UserService => ServiceProxy.GetService<UserService>();
        #endregion
        private readonly IHubContext<TanyaOhsChatHub> _hubContext;
        public AccountApiController(IHubContext<TanyaOhsChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        #region Public API
        /// <summary>
        /// Authenticate user and return claims if succedd.
        /// </summary>
        /// <param name="loginViewModel">Login Request Object.</param>
        /// <returns>OkResult Object if succedd.</returns>
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<RedirectResponse> Authenticate([FromBody]LoginViewModel loginViewModel)
        {
            var httpOnly = bool.Parse(ServiceProxy.Configuration["Authentication:HttpOnly"]);
            if (ModelState.IsValid)
            {
                var claims = UserService.GetClaims(loginViewModel.Username, loginViewModel.Password);

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties());
            }

            Response.Cookies.Append("PopUpAbnormality", "True",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            var isoLanguageName = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var culture = "en-US";

            if(isoLanguageName == "id")
            {
                culture = "id-ID";
            }

            Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("en-US", culture)),
            //new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict }
            new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None }
            );

            TanyaOhsHelper.Orm.UpdatePicStatus(loginViewModel.Username,"Available", _hubContext);

            return new RedirectResponse(loginViewModel.ReturnUrl);
        }

        /// <summary>
        /// Proxy as another user.
        /// </summary>
        /// <param name="proxyViewModel">This <see cref="ProxyViewModel"/> object.</param>
        [HttpPost("proxy")]
        [Permission(PermissionKey.ProxyAs)]
        public async Task<IActionResult> Proxy([FromBody]ProxyViewModel proxyViewModel)
        {
            var originator = ServiceProxy.UserClaim.Originator;
            var claims = UserService.ProxyAs(proxyViewModel.Username, originator);
            var logProxy = ConfigService.GetConfigValue("Application.LogProxy", false, false);

            if (logProxy)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

                CoreService.UpsertProxyLog(ProxyLog.Create(originator, proxyViewModel.Username, ipAddress));
            }

            return await UpdateClaims(claims);
        }

        /// <summary>
        /// Switch organization.
        /// </summary>
        /// <param name="proxyViewModel">This <see cref="ProxyViewModel"/> object.</param>
        [HttpPost("switch")]
        public async Task<IActionResult> Switch([FromBody]ProxyViewModel proxyViewModel)
        {
            var username = ServiceProxy.UserClaim.Username;
            var originator = ServiceProxy.UserClaim.Originator;
            var claims = UserService.ProxyAs(username, originator, proxyViewModel.Username);

            return await UpdateClaims(claims);
        }

        /// <summary>
        /// Impersonate as another user.
        /// </summary>
        /// <param name="proxyViewModel">This <see cref="ProxyViewModel"/> object.</param>
        [HttpPost("impersonate")]
        public async Task<IActionResult> Impersonate([FromBody]ProxyViewModel proxyViewModel)
        {
            Assert.ThrowIf(!CoreService.CanImpersonateAs(ServiceProxy.UserClaim.NoReg, proxyViewModel.Username), $"Cannot impersonate as <b>{proxyViewModel.Username}</b>");

            return await Proxy(proxyViewModel);
        }

        /// <summary>
        /// Return to original user.
        /// </summary>
        [HttpPost("self")]
        public async Task<IActionResult> BecameSelf()
        {
            var userClaim = ServiceProxy.UserClaim;

            Assert.ThrowIf(userClaim.NoReg == userClaim.Originator, "NoReg and originator cannot be same");

            var claims = UserService.ProxyAs(ServiceProxy.UserClaim.Originator);

            return await UpdateClaims(claims);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Update user claims.
        /// </summary>
        /// <param name="claims">This list of <see cref="Claim"/> objects.</param>
        private async Task<IActionResult> UpdateClaims(IEnumerable<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignOutAsync();
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties());

            return NoContent();
        }
        #endregion
    }
    #endregion

    #region MVC Controller
    [Area("Core")]
    public class AccountController : CommonControllerBase
    {
        #region Domain Services
        /// <summary>
        /// User service object
        /// </summary>
        protected UserService UserService => ServiceProxy.GetService<UserService>();
        #endregion

        /// <summary>
        /// Login page
        /// </summary>
        /// <returns>Login Page</returns>
        public IActionResult Login()
        {
            var maintenance = bool.Parse(ServiceProxy.Configuration["Maintenance"]);

            if (User.Identity.IsAuthenticated || maintenance)
            {
                return Redirect("~/");
            }

            var loginType = ServiceProxy.Configuration["LoginType"];

            return View(loginType == "form" ? "Login" : "TamLogin");
        }

        [HttpPost("/core/Account/TamLoginAsync")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> TamLoginAsync([FromForm]string returnUrl, [FromForm] string TAMSignOnToken)
        {
            var httpOnly = bool.Parse(ServiceProxy.Configuration["Authentication:HttpOnly"]);
            var loginType = ServiceProxy.Configuration["LoginType"];

            if (User.Identity.IsAuthenticated || loginType != "sso")
            {
                return Redirect("~/");
            }

            var splitter = TAMSignOnToken.Split('.');
            var encodedToken = splitter[1];
            int mod4 = encodedToken.Length % 4;
            if (mod4 > 0)
            {
                encodedToken += new string('=', 4 - mod4);
            }
            var base64EncodedBytes = Convert.FromBase64String(encodedToken);
            var content = Encoding.UTF8.GetString(base64EncodedBytes);

            if (!string.IsNullOrEmpty(content))
            {
                var token = JsonConvert.DeserializeObject<TokenViewModel>(content);

                var claims = UserService.GetClaims(token.Sub, string.Empty, true).ToList();

                claims.Add(new Claim("Jti", token.Jti));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties());
            }

            var isoLanguageName = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var culture = "en-US";

            if (isoLanguageName == "id")
            {
                culture = "id-ID";
            }

            Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("en-US", culture)),
            new CookieOptions { HttpOnly = true, Secure = true, Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            string safeUrl = "/";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                safeUrl = returnUrl;
            }

            return Redirect("~" + safeUrl);
        }

        private readonly IHubContext<TanyaOhsChatHub> _hubContext;
        public AccountController(IHubContext<TanyaOhsChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Clear session cookie and return to login page
        /// </summary>
        /// <returns>Login Page</returns>
        //[Authorize]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync();
                TanyaOhsHelper.Orm.UpdatePicStatus(User.GetClaim("Username"), "Tidak Available", _hubContext);
            }
            catch { }

            return Redirect("~/");
        }
    }
    #endregion
}