using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{ 
    [DatabaseObject("SF_GET_AYO_SEKOLAH_REPORT", DatabaseObjectType.TableValued)]
    public class AyoSekolahReportStoredEntity : AyoSekolahStoredEntity
    {
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
    }
}
