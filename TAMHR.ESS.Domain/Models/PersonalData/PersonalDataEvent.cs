using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA_EVENT")]
    public partial class PersonalDataEvent : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public Guid? FamilyMemberId { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
