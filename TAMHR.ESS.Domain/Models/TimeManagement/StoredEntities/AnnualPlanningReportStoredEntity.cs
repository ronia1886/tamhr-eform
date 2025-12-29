using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_ANNUAL_PLANNING_REPORT", DatabaseObjectType.StoredProcedure)]
    public class AnnualPlanningReportStoredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set;  }
        public string JobName { get; set; }
        public string Directorate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public string Class { get; set; }
        public int SequenceNo { get; set; }
        public int Period { get; set; }
        public string Title { get; set; }
        public string Unit { get; set; }
        public string JanPlan { get; set; }
        public string FebPlan { get; set; }
        public string MarPlan { get; set; }
        public string AprPlan { get; set; }
        public string MayPlan { get; set; }
        public string JunPlan { get; set; }
        public string JulPlan { get; set; }
        public string AugPlan { get; set; }
        public string SepPlan { get; set; }
        public string OctPlan { get; set; }
        public string NovPlan { get; set; }
        public string DecPlan { get; set; }
        public string TotPlan { get; set; }
        public string JanAct { get; set; }
        public string FebAct { get; set; }
        public string MarAct { get; set; }
        public string AprAct { get; set; }
        public string MayAct { get; set; }
        public string JunAct { get; set; }
        public string JulAct { get; set; }
        public string AugAct { get; set; }
        public string SepAct { get; set; }
        public string OctAct { get; set; }
        public string NovAct { get; set; }
        public string DecAct { get; set; }
        public string TotAct { get; set; }
        public string PositionLevel { get; set; }
    }
}
