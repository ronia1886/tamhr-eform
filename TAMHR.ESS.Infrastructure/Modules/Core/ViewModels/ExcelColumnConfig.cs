using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ExcelColumnConfig
    {
        public string Field { get; set; }
        public string DataType { get; set; }
        public string SqlDataType { get; set; }
        public string Format { get; set; }
        public string ColumnLabel { get; set; }
        public int? ColumnIndex { get; set; }
        public bool? IsEditable { get; set; }
        public double? Width { get; set; }
        public List<string> ValueList { get; set; }
        public bool? ShowError { get; set; }
        public string ErrorMessage { get; set; }
        public string FontColor { get; set; }
        public string BackgroundColor { get; set; }
    }
}
