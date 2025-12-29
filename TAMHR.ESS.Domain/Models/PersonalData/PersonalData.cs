using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA")]
    public partial class PersonalData : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CommonAttributeId { get; set; }
        public string NoReg { get; set; }
        public string JobTitle { get; set; }
        public string MaritalStatusCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        [JsonIgnore]
        [ForeignKey("CommonAttributeId")]
        public PersonalDataCommonAttribute PersonalDataCommonAttribute { get; set; }
    }
}
