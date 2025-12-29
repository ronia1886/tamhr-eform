using Newtonsoft.Json;

namespace TAMHR.ESS.Infrastructure.Web.ScriptManagement
{
    [JsonObject]
    public class ScriptManagerOption
    {
        [JsonProperty("references")]
        public ScriptReferenceItem[] ReferenceItems { get; set; }
        [JsonProperty("dependencyPath")]
        public string DependencyPath { get; set; }
        [JsonProperty("bundle")]
        public bool Bundle { get; set; }
    }
}
