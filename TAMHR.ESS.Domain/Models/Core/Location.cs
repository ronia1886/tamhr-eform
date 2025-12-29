using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_LOCATION")]
    public partial class Location : IEntityBase<Guid>
	{
        [Key]
		[Column(nameof(Id), TypeName = "uniqueidentifier")]
		public Guid Id { get; set; }
		public Guid? LocationGroupID { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool RowStatus { get; set; }
		public DateTime CreatedOn { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? ModifiedOn { get; set; }
		public string ModifiedBy { get; set; }
		public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
