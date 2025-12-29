using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_COMMON_CONVERSATION")]
    public partial class CommonConversation : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string ChannelId { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NoReg { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string Message { get; set; }

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
