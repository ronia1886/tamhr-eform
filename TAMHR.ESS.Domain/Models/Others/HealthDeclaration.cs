using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_HEALTH_DECLARATION")]
    public partial class HealthDeclaration : IEntityBase<Guid>
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ReferenceDocumentApprovalId { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NoReg { get; set; }

        [Column(TypeName = "date")]
        public DateTime SubmissionDate { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string PhoneNumber { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Email { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string EmergencyFamilyStatus { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string EmergencyName { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string EmergencyPhoneNumber { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string WorkTypeCode { get; set; }

        [Column(TypeName = "bit")]
        public bool HaveFever { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string BodyTemperature { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string BodyTemperatureOtherValue { get; set; }

        [Column(TypeName = "bit")]
        public bool IsSick { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Remarks { get; set; }

        [Column(TypeName = "bit")]
        public bool Marked { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ModifiedOn { get; set; }

        [Column(TypeName = "bit")]
        public bool RowStatus { get; set; }
    }
}
