using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [Table("VW_VACCINE_QUESTION")]
    public partial class VaccineQuestionView : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }
        public Guid VaccineId { get; set; }
        public string Title { get; set; }
        public string Answer { get; set; }
    }
}