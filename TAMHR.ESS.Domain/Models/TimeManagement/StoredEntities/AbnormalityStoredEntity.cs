using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_ABNORMALITY", DatabaseObjectType.TableValued)]
    public class AbnormalityStoredEntity
    {
        public string NoReg { get; set; }
        public string Type { get; set; }
        public int TotalAbnormality { get; set; }
        public string ViewURL { get; set; }
        public string CreateURL { get; set; }
    }
}
