using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_FORM_QUESTION")]
    public partial class FormQuestion : IEntityBase<Guid>
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid? ParentFormQuestionId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid FormId { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CategoryCode { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string QuestionTypeCode { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string Title { get; set; }

        [Column(TypeName = "int")]
        public int OrderSequence { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string DefaultValue { get; set; }

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

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string KeyCode { get; set; }

        [JsonIgnore]
        [ForeignKey("ParentFormQuestionId")]
        public FormQuestion ParentFormQuestion { get; set; }

        [JsonIgnore]
        [ForeignKey("FormId")]
        public Form Form { get; set; }
    }
}
