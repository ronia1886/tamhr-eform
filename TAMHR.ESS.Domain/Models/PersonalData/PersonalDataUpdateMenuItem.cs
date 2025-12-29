using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_PERSONAL_DATA_UPDATE_MENU_ITEM")]
    public partial class PersonalDataUpdateMenuItem : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
		public string UpdateDataId { get; set; }
		public string UpdateDataStatus { get; set; }
		public int? Progress { get; set; }
		public string MenuCode { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string ModifiedBy { get; set; }
		public DateTime? ModifiedOn { get; set; }
		public bool RowStatus { get; set; }
	}
}
