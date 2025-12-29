using System;
using System.Security.Claims;
using TAMHR.ESS.Infrastructure.Web.Extensions;

namespace TAMHR.ESS.Infrastructure.Web.ContextPrincipal
{
    /// <summary>
    /// Class that hold user claim data
    /// </summary>
    public class UserClaim
    {
        /// <summary>
        /// User noreg property
        /// </summary>
        public string NoReg { get; set; }

        /// <summary>
        /// User name property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// UserName property
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Total position property
        /// </summary>
        public int TotalPosition { get; set; }

        /// <summary>
        /// Chief property
        /// </summary>
        public bool Chief { get; set; }

        /// <summary>
        /// Has impersonation property
        /// </summary>
        public bool HasImpersonation { get; set; }

        /// <summary>
        /// User position code property
        /// </summary>
        public string PostCode { get; set; }

        /// <summary>
        /// User position name property
        /// </summary>
        public string PostName { get; set; }

        /// <summary>
        /// Property that hold user roles in string format
        /// </summary>
        public string RoleText { get; set; }

        /// <summary>
        /// User originator noreg property
        /// </summary>
        public string Originator { get; set; }

        /// <summary>
        /// List of roles property
        /// </summary>
        public string[] Roles { get { return (RoleText??string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isFirstLogin">Check First Login</param>
        public bool isFirstLogin { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="claimsPrincipal">Claims Principal Object</param>
        public UserClaim(ClaimsPrincipal claimsPrincipal)
        {
            NoReg = claimsPrincipal.GetClaim(nameof(NoReg));
            Name = claimsPrincipal.GetClaim(nameof(Name));
            Username = claimsPrincipal.GetClaim(nameof(Username));
            PostCode = claimsPrincipal.GetClaim(nameof(PostCode));
            PostName = claimsPrincipal.GetClaim(nameof(PostName));
            RoleText = claimsPrincipal.GetClaim(nameof(Roles));
            TotalPosition = int.Parse(claimsPrincipal.GetClaim(nameof(TotalPosition)) ?? "0");
            HasImpersonation = bool.Parse(claimsPrincipal.GetClaim(nameof(HasImpersonation)) ?? "False");
            Chief = bool.Parse(claimsPrincipal.GetClaim(nameof(Chief)) ?? "False");
            Originator = claimsPrincipal.GetClaim(nameof(Originator));
        }

        /// <summary>
        /// Create user claim from claims principal object
        /// </summary>
        /// <param name="claimsPrincipal">Claims Principal Object</param>
        /// <returns></returns>
        public static UserClaim CreateFrom(ClaimsPrincipal claimsPrincipal)
        {
            var userClaim = new UserClaim(claimsPrincipal);

            return userClaim;
        }
    }
}
