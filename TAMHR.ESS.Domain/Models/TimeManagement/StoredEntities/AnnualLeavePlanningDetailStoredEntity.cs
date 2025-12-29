using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_ANNUAL_LEAVE_PLANNING_DETAIL", DatabaseObjectType.TableValued)]
    public class AnnualLeavePlanningDetailStoredEntity 
    {
        public DateTime PlanDate { get; set; }
        public DateTime WorkingDate { get; set; }
        public string Plans { get; set; }
        public string Actual { get; set; }
        public string Remark { get; set; }
    }
}
