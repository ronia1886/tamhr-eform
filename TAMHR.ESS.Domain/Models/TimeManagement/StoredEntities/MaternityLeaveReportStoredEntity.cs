using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{ 
    [DatabaseObject("SF_GET_MATERNITY_LEAVE_REPORT", DatabaseObjectType.TableValued)]
    public class MaternityLeaveReportStoredEntity : AyoSekolahStoredEntity
    {
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
    }
}
