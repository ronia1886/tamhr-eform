using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_WEEKLY_WFH_PLANNING_DETAIL")]
    public partial class WeeklyWFHPlanningDetail : IEntityBase<Guid>
    {
        public Guid Id { get; set; }
        public Guid WeeklyWFHPlanningId { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Plan { get; set; }
        public string Actual { get; set; }
        public string Division { get; set; }
        public string Departement { get; set; }
        public string Section { get; set; }
        public string NoReg { get; set; }
        public DateTime WorkingDate { get; set; }
        public string WorkPlace { get; set; }
        //public int Days { get; set; }
        public bool IsOff { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Directorate { get; set; }
        public string ClassName { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public string PlanWorkPlace { get; set; }
        public string ActualWorkPlace { get; set; }
        public string CreatedWith { get; set; }
    }
}
