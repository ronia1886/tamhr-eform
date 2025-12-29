using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_HIERARCHIES", DatabaseObjectType.TableValued)]
    public class AnnualPlanningHierarchiesStoredEntity
    {
        public long OrderRank { get; set; }
        public int HierarchyLevel { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
    }
}
