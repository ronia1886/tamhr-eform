using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ProfileViewModel
    {
        public PersonalDataEducation PersonalDataEducation { get; set; }
        public PersonalDataFamilyMember PersonalDataFamilyMember { get; set; }
    }
}
