using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;


namespace TAMHR.ESS.Infrastructure.Validators
{
    class ReturnBpkbCOPValidation : AbstractDocumentValidator<ReturnBpkbCOPViewModel>
    {
        public ReturnBpkbCOPValidation()
        {
            RuleFor(x => x.Object.PoliceNumber).NotEmpty();
            RuleFor(x => x.Object.Type).NotEmpty();
            RuleFor(x => x.Object.Model).NotEmpty();
            RuleFor(x => x.Object.ProductionYear).NotEmpty();
            RuleFor(x => x.Object.Color).NotEmpty();
            RuleFor(x => x.Object.FrameNumber).NotEmpty();
            RuleFor(x => x.Object.MachineNumber).NotEmpty();
            RuleFor(x => x.Object.Name).NotEmpty();
            RuleFor(x => x.Object.Address).NotEmpty();
            RuleFor(x => x.Object.Type).NotEmpty();
            RuleFor(x => x.Object.LoanDate).NotEmpty();
            RuleFor(x => x.Object.ReturnDate).NotEmpty();
            RuleFor(x => x.Object.NecessityCode).NotEmpty();
            When(x => x.Object.IsOtherNecessity.Equals(true), () =>
            {
                RuleFor(x => x.Object.OtherNecessity).NotEmpty();
            });
            When(x => x.Object.IsOtherNecessity.Equals(false), () =>
            {
                RuleFor(x => x.Object.NecessityCode).NotEmpty();
            });
        }
    }
}
