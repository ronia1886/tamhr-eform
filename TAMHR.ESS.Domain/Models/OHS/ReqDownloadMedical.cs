using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{

    [Table("TB_R_REQUEST_MEDICAL_RECORD")]
    public partial class ReqDownloadMedical : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Approver { get; set; }
        public string Requestor { get; set; }
        public string StatusRequest { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }


    }

}
