using FluentValidation;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class MaternityLeaveValidator : AbstractDocumentValidator<MaternityLeaveViewModel>
    {
        public MaternityLeaveValidator()
        {
            RuleFor(x => x.Object.GestationalAge).NotEmpty();
            RuleFor(x => x.Object.Date).NotEmpty();
            RuleFor(x => x.Object.EstimatedDayOfBirth).NotEmpty();
            RuleFor(x => x.Object.StartHandoverOfWork).NotEmpty();
            RuleFor(x => x.Object.StartMaternityLeave).NotEmpty();
            RuleFor(x => x.Object.BackToWork).NotEmpty();
            RuleFor(x => x.Object.MedicalMertificatePath).NotEmpty();

        }
    }
}
