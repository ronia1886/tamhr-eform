using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_MENU_PERMISSION")]
    public partial class MenuPermissionView : IEntityMarker, IMenu
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid PermissionId { get; set; }
        public string MenuGroupCode { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public int OrderIndex { get; set; }
        public bool Visible { get; set; }
        public bool EnableOtp { get; set; }
        public string Roles { get; set; }
        public string Params { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
