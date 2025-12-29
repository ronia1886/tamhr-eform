using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ExcelConfig
    {
        public int Index { get; set; }
        public string SheetName { get; set; }
        public string SourceFile { get; set; }
        public string Name { get; set; }
        public int StartRow { get; set; }
        public IEnumerable<ExcelColumnConfig> ExcelColumnConfig { get; set; }
    }
}
