using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_CONCEPT_IDEA")]
    public partial class ConceptIdea : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string PostName { get; set; }
        public string Criteria { get; set; }
        public string Title { get; set; }
        public string Point { get; set; }
        public decimal Ammount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
