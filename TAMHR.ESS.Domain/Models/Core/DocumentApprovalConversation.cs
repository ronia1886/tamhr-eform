using System;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_DOCUMENT_APPROVAL_CONVERSATION")]
    public partial class DocumentApprovalConversation : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public string MentionTo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static DocumentApprovalConversation Create(Guid documentApprovalId, string noreg, string name, string message, string mentionTo = "")
        {
            var pattern = @"@([\w.]+)";
            var regex = new Regex(pattern, RegexOptions.None);

            var documentApprovalConversation = new DocumentApprovalConversation
            {
                DocumentApprovalId = documentApprovalId,
                NoReg = noreg,
                Name = name,
                Message = regex.Replace(message, $"<b>$&</b>"),
                MentionTo = mentionTo
            };

            return documentApprovalConversation;
        }
    }
}
