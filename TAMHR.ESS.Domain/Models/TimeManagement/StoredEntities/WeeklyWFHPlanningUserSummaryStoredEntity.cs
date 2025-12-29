using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_WEEKLY_WFH_EMAIL_SUMMARY", DatabaseObjectType.StoredProcedure)]
    public class WeeklyWFHPlanningUserSummaryStoredEntity
    {
        public string ParentNoReg { get; set; }
        public string Name { get; set; }
        public string NoReg { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string Directorate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public string Class { get; set; }
    }
}
