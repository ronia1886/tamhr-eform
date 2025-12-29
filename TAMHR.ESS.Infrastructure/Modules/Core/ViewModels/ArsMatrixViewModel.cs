using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ArsMatrixViewModel
    {
        public string Title { get; set; }
        public IEnumerable<ArsMatrixItemViewModel> Items { get; set; }
    }

    public class ArsMatrixItemViewModel
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public string ImageUrl { get; set; }
    }
}
