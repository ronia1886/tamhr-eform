using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    
    [Table("TB_M_VEHICLE")]
    public partial class Vehicle : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string ModelCode { get; set; }
        public string Suffix { get; set; }
        public string TypeName { get; set; }
        public string Type { get; set; }
        public bool? CPP { get; set; }
        public bool? COP { get; set; }
        public bool? SCP { get; set; }
        public decimal FinalPrice { get; set; }
        public string Colors { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(20)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        [NotMapped]
        public string[] IsUpgrade { get; set; }
        [NotMapped]
        public string[] Matrix { get; set; }
        [NotMapped]
        public string LoanType {
            get
            {
                return CPP.HasValue && CPP.Value ? "CPP" : (COP.HasValue && COP.Value ? "COP" : (SCP.HasValue && SCP.Value ? "SCP" : string.Empty));
            }
            set
            {
                if (value == "CPP")
                {
                    COP = null;
                    CPP = true;
                    SCP = null;
                }
                else if (value == "COP")
                {
                    COP = true;
                    CPP = null;
                    SCP = null;
                }
                else if (value == "SCP")
                {
                    COP = null;
                    CPP = null;
                    SCP = true;
                }
            }
        }
    }
}
