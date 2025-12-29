using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ParentMenuViewModel
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public int OrderIndex { get; set; }
        public string MenuGroupCode { get; set; }
    }
}
