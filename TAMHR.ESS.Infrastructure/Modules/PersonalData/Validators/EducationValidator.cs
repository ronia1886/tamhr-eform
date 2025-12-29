using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class EducationValidator : AbstractDocumentValidator<EducationViewModel>
    {
        public EducationValidator()
        {
            RuleFor(x => x.Object.LevelOfEducationCode).NotEmpty();
            RuleFor(x => x.Object.DepartmentCode).NotEmpty();
            RuleFor(x => x.Object.Period).NotEmpty();
            RuleFor(x => x.Object.GPA).NotEmpty();
            RuleFor(x => x.Object.DiplomaPath).NotEmpty();

            When(x => x.Object.IsOtherCollegeName.Equals(true), () => {
                RuleFor(x => x.Object.OtherCollegeName).NotEmpty();
            });
            When(x => x.Object.IsOtherCollegeName.Equals(false), () => {
                RuleFor(x => x.Object.CollegeName).NotEmpty();
            });

        }
    }
}
