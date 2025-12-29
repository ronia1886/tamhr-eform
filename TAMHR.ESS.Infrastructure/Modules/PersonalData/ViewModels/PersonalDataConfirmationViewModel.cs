using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class PersonalDataConfirmationViewModel
    {
        public PersonalDataGroupViewModel[] Groups { get; set; }
        public string Confirmation { get; set; }
        public class PersonalDataGroupViewModel
        {
            public string Url { get; set; }
            public string Group { get; set; }
            public string Field { get; set; }
            public string Value { get; set; }
        }
    }
}
