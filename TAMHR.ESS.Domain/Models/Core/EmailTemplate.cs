using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_EMAIL_TEMPLATE")]
    public partial class EmailTemplate : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string ModuleCode { get; set; }
        public string MailKey { get; set; }
        public string MailFrom { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string MailContent { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
