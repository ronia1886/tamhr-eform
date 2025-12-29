using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TAMHR.ESS.Domain.Models.OHS
{
    [Table("TB_R_TANYAOHS_CHAT")]
    public partial class TanyaOhsChat : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string TanyaOhsId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
