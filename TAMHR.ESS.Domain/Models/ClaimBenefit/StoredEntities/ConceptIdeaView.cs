using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_CONCEPT_IDEA")]
    public class ConceptIdeaView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Directorate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get;set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string Class { get; set; }
        public string Superior { get; set; }
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
