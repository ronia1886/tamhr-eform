using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{ 
    [DatabaseObject("SF_GET_SPKL_DETAIL_REPORT", DatabaseObjectType.TableValued)]
    public class SpklRequestDetailReportStoredEntity : SpklRequestDetailStoredEntity
    {
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
    }
}
