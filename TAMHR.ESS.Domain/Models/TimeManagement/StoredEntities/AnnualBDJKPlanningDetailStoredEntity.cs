using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_ANNUAL_BDJK_PLANNING_DETAIL", DatabaseObjectType.TableValued)]
    public class AnnualBDJKPlanningDetailStoredEntity 
    {
        public DateTime PlanDate { get; set; }
        public string Plans { get; set; }
        public string Actual { get; set; }
        public string Remark { get; set; }
    }
}
