using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_TRAINING_HISTORY")]
    public partial class TrainingHistoryView : IEntityMarker
    {
        [Key]
        [Column(nameof(Id), TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(nameof(CommonFileId), TypeName = "uniqueidentifier")]
        public Guid? CommonFileId { get; set; }

        [Column(nameof(TrainingCatalogId), TypeName = "uniqueidentifier")]
        public Guid? TrainingCatalogId { get; set; }

        [Column(nameof(TrainingParticipantId), TypeName = "uniqueidentifier")]
        public Guid? TrainingParticipantId { get; set; }

        [Column(nameof(Period), TypeName = "int")]
        public int Period { get; set; }

        [Column(nameof(ImageThumbUrl), TypeName = "varchar(450)")]
        [MaxLength(450)]
        public string ImageThumbUrl { get; set; }

        [Column(nameof(TrainingCode), TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string TrainingCode { get; set; }

        [Column(nameof(Participant), TypeName = "varchar(13)")]
        [MaxLength(13)]
        public string Participant { get; set; }

        [Column(nameof(TrainingName), TypeName = "varchar(150)")]
        [MaxLength(150)]
        public string TrainingName { get; set; }

        [Column(nameof(NoReg), TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string NoReg { get; set; }

        [Column(nameof(StartDate), TypeName = "date")]
        public DateTime? StartDate { get; set; }

        [Column(nameof(EndDate), TypeName = "date")]
        public DateTime? EndDate { get; set; }

        [Column(nameof(TrainingDuration), TypeName = "int")]
        public int? TrainingDuration { get; set; }

        [Column(nameof(TrainingType), TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string TrainingType { get; set; }

        [Column(nameof(StatusCode), TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string StatusCode { get; set; }

        [Column(nameof(CompetencyGroup), TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string CompetencyGroup { get; set; }

        [Column(nameof(TrainingProvider), TypeName = "varchar(150)")]
        [MaxLength(150)]
        public string TrainingProvider { get; set; }

        [Column(nameof(Location), TypeName = "varchar(150)")]
        [MaxLength(150)]
        public string Location { get; set; }

        [Column(nameof(Currency), TypeName = "varchar(10)")]
        [MaxLength(10)]
        public string Currency { get; set; }

        [Column(nameof(Cost), TypeName = "money")]
        public decimal Cost { get; set; }

        [Column(nameof(CreatedBy), TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string CreatedBy { get; set; }

        [Column(nameof(CreatedOn), TypeName = "datetime")]
        public DateTime CreatedOn { get; set; }

        [Column(nameof(ModifiedBy), TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

        [Column(nameof(ModifiedOn), TypeName = "datetime")]
        public DateTime? ModifiedOn { get; set; }

        [Column(nameof(RowStatus), TypeName = "bit")]
        public bool RowStatus { get; set; }

        [Column(nameof(Description), TypeName = "varchar(MAX)")]
        public string Description { get; set; }

        [Column(nameof(Rating), TypeName = "decimal(18,2)")]
        public decimal? Rating { get; set; }
    }
}
