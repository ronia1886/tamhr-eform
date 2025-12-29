using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_FORM_QUESTION_GROUP_ANSWER")]
    public partial class FormQuestionGroupAnswer : IEntityBase<Guid>
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid FormQuestionId { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string GroupAnswer { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Value { get; set; }

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

        [ForeignKey("FormQuestionId")]
        public FormQuestion FormQuestion { get; set; }
    }
}
