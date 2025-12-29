using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Agit.Common;

namespace TAMHR.ESS.Domain
{

    [Table("VW_PROJECT_ACTIVITY")]
    public partial class ProjectActivityViewModel : IEntityMarker
    {
        public Guid Id { get; set; }
        public Int64 RowNum { get; set; }
        public int TotalWorker { get; set; }
        public string RiskCategory { get; set; }
        public string ProjectName { get; set; }
        public string TotalWorkerSearch { get; set; }
        public string Contractor { get; set; }
        public string StartDateSearch { get; set; }
        public string EndDateSearch { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string NamaArea { get; set; }
        public bool RowStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedOnSearch { get; set; }
    }

    [Table("TB_R_PROJECT_ACTIVITY")]
    public partial class ProjectActivityModel : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public int TotalWorker { get; set; }
        public string RiskCategory { get; set; }
        public string ProjectName { get; set; }
        public string Contractor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public Guid? AreaId { get; set; }
        public string AreaName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
