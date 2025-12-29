using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ShiftPlanningValidator : AbstractDocumentValidator<ShiftPlanningViewModel>
    {
        public ShiftPlanningValidator()
        {
            When(x => x.Object.TypeShift.Equals("form"), () => {
                RuleFor(x => x.Object.TypeShift).NotEmpty();
            });
            When(x => x.Object.TypeShift.Equals("upload"), () => {
                RuleFor(x => x.Object.Year).NotEmpty();
                RuleFor(x => x.Object.Month).NotEmpty();
                RuleFor(x => x.Object.UploadExcelPath).NotEmpty();
                RuleFor(x => x.Object.Request).NotNull().WithMessage("Detail Shift tidak boleh kosong / data belum di upload.");
            });
        }
    }
}
