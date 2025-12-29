using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_FORM_QUESTION_DETAIL")]
    public partial class FormQuestionDetail : IEntityBase<Guid>
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid FormQuestionId { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DescriptionType { get; set; }

        [Column(TypeName = "int")]
        public int? OrderSequence { get; set; }

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
        [Column(TypeName = "varchar(max)")]
        public string MinVal { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string MaxVal { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string MinTrue { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string MaxTrue { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string CriteriaType { get; set; }
        [Column(TypeName = "bit")]
        public bool IsSubmittedDate { get; set; }

        [JsonIgnore]
        [ForeignKey("FormQuestionId")]
        public FormQuestion FormQuestion { get; set; }
    }
}
