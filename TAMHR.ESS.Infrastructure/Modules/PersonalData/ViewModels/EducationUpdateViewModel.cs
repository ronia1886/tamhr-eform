using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class EducationUpdateViewModel
    {
        public Guid Id { get; set; }
        public string Noreg { get; set; }
        public string EducationLevel { get; set; }
        public string Collage { get; set; }
        public string Other { get; set; }
        public DateTime StartEducationDate { get; set; }
        public DateTime GraduationDate { get; set; }
        public string GPA { get; set; }
        public string Major { get; set; }
        public string Country { get; set; }
        public string ModifiedBy { get; set; }
        //  public string Remarks { get; set; }
    }
}
