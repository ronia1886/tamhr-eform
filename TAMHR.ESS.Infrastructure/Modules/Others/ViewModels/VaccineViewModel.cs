using System;
using System.Collections.Generic;
namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class VaccineViewModel
    {
        public FormAnswerViewModel[] FormAnswers { get; set; } = Array.Empty<FormAnswerViewModel>();
        public List<VaccineDetailViewModel> VaccineDetailViewModels { get; set; } = new List<VaccineDetailViewModel>();
    }
}
