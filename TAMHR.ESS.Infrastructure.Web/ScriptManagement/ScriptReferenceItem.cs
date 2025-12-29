using Newtonsoft.Json;

namespace TAMHR.ESS.Infrastructure.Web.ScriptManagement
{
    [JsonObject]
    public class ScriptReferenceItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("scripts")]
        public string[] Scripts { get; set; }
    }
}
