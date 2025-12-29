using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_ROLE")]
    public partial class Role : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string RoleKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RoleTypeCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static Role CreateDefault()
        {
            var role = new Role();

            return role;
        }
    }
}
