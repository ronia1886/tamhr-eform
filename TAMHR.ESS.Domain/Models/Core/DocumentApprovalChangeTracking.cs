using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_DOCUMENT_APPROVAL_CHANGE_TRACKING")]
    public partial class DocumentApprovalChangeTracking : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string FieldName { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string FormattedValue { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static DocumentApprovalChangeTracking Create(DocumentApprovalChangeTracking tracking, User user)
        {
            var output = new DocumentApprovalChangeTracking
            {
                Id = tracking.Id,
                DocumentApprovalId = tracking.DocumentApprovalId,
                FieldName = tracking.FieldName,
                FormattedValue = tracking.FormattedValue,
                Name = user.Name,
                CreatedBy = tracking.CreatedBy,
                CreatedOn = tracking.CreatedOn,
                ModifiedBy = tracking.ModifiedBy,
                ModifiedOn = tracking.ModifiedOn,
                RowStatus = tracking.RowStatus
            };

            return output;
        }
    }
}
