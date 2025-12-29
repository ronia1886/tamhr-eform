using FluentValidation;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class CopExplorationBpkbValidation : AbstractValidator<CopExplorationBpkbViewModel>
    {
        public CopExplorationBpkbValidation()
        {
            RuleFor(x => x.PoliceNumber).NotEmpty();
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.Model).NotEmpty();
            RuleFor(x => x.ProductionYear).NotEmpty();
            RuleFor(x => x.Color).NotEmpty();
            RuleFor(x => x.FrameNumber).NotEmpty();
            RuleFor(x => x.MachineNumber).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Address).NotEmpty();

        }
    }
}
