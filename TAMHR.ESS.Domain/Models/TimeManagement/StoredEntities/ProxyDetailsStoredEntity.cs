using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_PROXY_DETAILS", DatabaseObjectType.StoredProcedure)]
    public class ProxyDetailsStoredEntity
    {
        public string NoReg { get; set; }
        public DateTime ProxyDate { get; set; }
        public DateTime ProxyTime { get; set; }
    }
}
