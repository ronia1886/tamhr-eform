using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ComplaintRequestViewModel
    {
        public string ComplaintCode { get; set; }
        public string ComplaintSubCode { get; set; }
        public string ComplaintDetail { get; set; }
        public string Solution { get; set; }
        public string Expectation { get; set; }
        public string Remarks { get; set; }
        public string FilePath { get; set; }
        public Guid CommonFileId { get; set; }
    }
}
