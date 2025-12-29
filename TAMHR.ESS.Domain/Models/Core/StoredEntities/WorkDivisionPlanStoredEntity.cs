using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_WORK_DIVISION_PLAN", DatabaseObjectType.TableValued)]
    public partial class WorkDivisionPlanStoredEntity
    {
        public Guid Id { get; set; }
        public string ObjectId { get; set; }
        public string ObjectText { get; set; }
        public string ObjectDescription { get; set; }
        public string Abbreviation { get; set; }
        public bool Active { get; set; }
    }
}
