using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class EducationViewModel
    {
        public string LevelOfEducationCode { get; set; }
        public string CollegeName { get; set; }
        public bool IsOtherCollegeName { get; set; }
        public string OtherCollegeName { get; set; }
        public string DepartmentCode { get; set; }
        public DateTime? Period { get; set; }
        public decimal? GPA { get; set; }
        public string DiplomaPath { get; set; }
        public string Remarks { get; set; }
    }
}
