using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class EyeglassesAlloanceValidator : AbstractDocumentValidator<EyeglassesAllowanceViewModel>
    {
        public EyeglassesAlloanceValidator()
        {
            When(x => !x.Object.IsLens && !x.Object.IsFrame, () =>
            {
                RuleFor(x => x.Object.AmountLens).NotEmpty();
                RuleFor(x => x.Object.LensAttachment).NotEmpty();
                RuleFor(x => x.Object.PurchaseLens).NotEmpty();
                RuleFor(x => x.Object.AmountFrame).NotEmpty();
                RuleFor(x => x.Object.FrameAttachment).NotEmpty();
                RuleFor(x => x.Object.PurchaseFrame).NotEmpty();
            });

            When(x => x.Object.IsLens, () =>
            {
                RuleFor(x => x.Object.LensType).NotEmpty();
                RuleFor(x => x.Object.AmountLens).NotEmpty();
                RuleFor(x => x.Object.LensAttachment).NotEmpty();
                RuleFor(x => x.Object.PurchaseLens).NotEmpty();
            });

            When(x => x.Object.IsFrame, () =>
            {
                RuleFor(x => x.Object.AmountFrame).NotEmpty();
                RuleFor(x => x.Object.FrameAttachment).NotEmpty();
                RuleFor(x => x.Object.PurchaseFrame).NotEmpty();
            });
        }
    }
}
