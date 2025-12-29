using Newtonsoft.Json;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class TokenViewModel
    {
        [JsonProperty("iss")]
        public string Iss { get; set; }
        [JsonProperty("aud")]
        public string Aud { get; set; }
        [JsonProperty("sub")]
        public string Sub { get; set; }
        [JsonProperty("iat")]
        public string Iat { get; set; }
        [JsonProperty("exp")]
        public string Exp { get; set; }
        [JsonProperty("jti")]
        public string Jti { get; set; }
        [JsonProperty("unique_name")]
        public string UniqueName { get; set; }
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("EmployeeId")]
        public string NoReg { get; set; }
        [JsonProperty("roles")]
        public string[] Roles { get; set; }
    }
}
