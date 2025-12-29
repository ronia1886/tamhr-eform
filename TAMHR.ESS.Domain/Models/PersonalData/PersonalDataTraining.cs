using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    //TB_M_PERSONAL_DATA_TRAINING
    [Table("TB_M_PERSONAL_DATA_TRAINING")]
    public partial class PersonalDataTraining : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string TrainingCode { get; set; }
        public string Year { get; set; }
        public string TrainingName { get; set; }
        public DateTime? TrainingStart { get; set; }
        public DateTime? TrainingEnd { get; set; }
        public string Institution { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
