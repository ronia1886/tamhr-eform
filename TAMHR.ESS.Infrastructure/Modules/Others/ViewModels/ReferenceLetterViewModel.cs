using FluentValidation;
using System;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ReferenceLetterViewModel
    {
        public string ReferenceLetterPurposeCode { get; set; }
        public string ReferenceLetterNumber { get; set; }
        public string Remarks { get; set; }
        public string FilePath { get; set; }
    }
}
