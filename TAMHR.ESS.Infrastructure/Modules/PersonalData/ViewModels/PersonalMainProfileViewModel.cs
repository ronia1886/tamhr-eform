using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class PersonalMainProfileViewModel
    {
        public string Noreg { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime AstraDate { get; set; }
        public string WorkLocation { get; set; }
        public string ModifiedBy { get; set; }
    }
}
