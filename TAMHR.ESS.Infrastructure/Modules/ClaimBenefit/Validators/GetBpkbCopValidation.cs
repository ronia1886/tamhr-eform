using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class GetBpkbCopValidation : AbstractDocumentValidator<GetBpkbCopViewModel>
    {
        public GetBpkbCopValidation()
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
            //RuleFor(x => x.Object.Necessity).NotEmpty();
            //RuleFor(x => x.Object.LoanDate).NotEmpty();
            //RuleFor(x => x.Object.ReturnDate).NotEmpty();
        }
    }
}
