using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Agit.Common;

namespace TAMHR.ESS.Domain
{

    [Table("VW_TRAINING_RECORD")]
    public partial class TrainingRecordViewModel : IEntityMarker
    {
        public Guid Id { get; set; }
        public Int64 RowNum { get; set; }
        public int TotalPerson { get; set; }
        public string Participant { get; set; }
        public string TotalPersonSearch { get; set; }
        public string Description { get; set; }
        public string Institution { get; set; }
        public string Remark { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public string AreaId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string TrainingStartDateSearch { get; set; }
        public string TrainingEndDateSearch { get; set; }
        public string AreaName { get; set; }
        public string NamaArea { get; set; }
        public bool RowStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedOnSearch { get; set; }
    }

    [Table("TB_R_TRAINING_RECORD")]
    public partial class TrainingRecordModel : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public int TotalPerson { get; set; }
        public string Participant { get; set; }
        public string Description { get; set; }
        public string Institution { get; set; }
        public string Remark { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
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
