using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_BDJK_DETAIL_REPORT", DatabaseObjectType.TableValued)]
    public class BdjkRequestDetailReportStoredEntity : BdjkRequestDetailStoredEntity
    {
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
    }
}
