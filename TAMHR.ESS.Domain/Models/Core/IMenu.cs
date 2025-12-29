using System;

namespace TAMHR.ESS.Domain
{
    public interface IMenu
    {
        Guid Id { get; set; }
        Guid? ParentId { get; set; }
        Guid PermissionId { get; set; }
        string MenuGroupCode { get; set; }
        string Title { get; set; }
        string Url { get; set; }
        string Description { get; set; }
        string IconClass { get; set; }
        string Params { get; set; }
        int OrderIndex { get; set; }
        bool Visible { get; set; }
        bool EnableOtp { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
        bool RowStatus { get; set; }
    }
}
