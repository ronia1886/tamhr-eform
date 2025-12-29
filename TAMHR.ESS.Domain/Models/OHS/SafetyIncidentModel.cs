using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_SAFETY_INCIDENT")]
    public partial class SafetyIncidentViewModel : IEntityMarker
    {
        //[Key]
        public Guid Id { get; set; }
        public Int64 RowNum { get; set; }
        public string IncidentDescription { get; set; }
        public string IncidentTypeCode { get; set; }
        public DateTime IncidentDate { get; set; }
        public string IncidentDateSearch { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string NamaArea { get; set; }
        public string Subject { get; set; }
        public string Remark { get; set; }
        public string TotalLoss { get; set; }
        public string AccidentType { get; set; }
        public string PropertyType { get; set; }
        public int? TotalVictim { get; set; }
        public string LossTime { get; set; }
        public int? LossTime2 { get; set; }
        public string Attachment { get; set; }
        public string CreatedBy { get; set; }
        public bool RowStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedOnSearch { get; set; }
        public string Periode { get; set; }
    }

    [Table("TB_R_SAFETY_INCIDENT")]
    public partial class SafetyIncidentModel : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string IncidentDescription { get; set; }
        public string IncidentTypeCode { get; set; }
        public DateTime IncidentDate { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public Guid AreaId { get; set; }
        public string AreaName { get; set; }
        public string Subject { get; set; }
        public string Remark { get; set; }
        public string TotalLoss { get; set; }
        public string AccidentType { get; set; }
        public string PropertyType { get; set; }
        public int? TotalVictim { get; set; }
        public string LossTime { get; set; }
        public string Attachment { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public DateTime? DeletedOn { get; set; }

    }
}

