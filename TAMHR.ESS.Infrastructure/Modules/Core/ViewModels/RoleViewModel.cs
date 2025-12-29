using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class RoleViewModel
    {
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
        public Guid[] Permissions { get; set; }
    }
}
