using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_ANNUAL_PLANNING_ABNORMALITY", DatabaseObjectType.StoredProcedure)]
    public class AnnualPlanningAbnormalitiesStoredEntity
    {
        public string AnnualType { get; set; }

        public DateTime PlanDate { get; set; }
        public string Plans { get; set; }
        public string Actual { get; set; }
        public string Remark { get; set; }
    }
}
