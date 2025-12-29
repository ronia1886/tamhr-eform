using Agit.Common.Utility;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class AbsenceValidator : AbstractDocumentValidator<AbsenceViewModel>
    {
        public AbsenceValidator(AbsenceService absenceService, EmployeeLeaveService employeeLeaveService, IStringLocalizer<AbsenceValidator> localizer)
        {
            RuleFor(x => x.Object.IsPlanning).NotEmpty();

            RuleFor(x => x.Object.StartDate).NotEmpty()
                .WithMessage(localizer["Please select a date"].Value);

            RuleFor(x => x.Object.EndDate).NotEmpty()
                .WithMessage(localizer["Please select a date"].Value)
                .GreaterThanOrEqualTo(x => x.Object.StartDate)
                .WithMessage(localizer["End date must be greater than or equal to start date"].Value);

            RuleFor(x => x.Object.ReasonId).NotEmpty()
                .WithMessage(localizer["Please select reason"].Value)
                .Must(x => absenceService.Contains(x.HasValue ? x.Value : Guid.Empty))
                .WithMessage(localizer["Reason is not found"].Value);

            RuleFor(x => x.Object.Reason).NotEmpty()
                .WithMessage(localizer["Reason cannot be empty"].Value);
            When(x => x.Object.ReasonId == new Guid("8267D8F5-4D84-4A53-9C4C-F5FBCC90969B") || x.Object.ReasonId == new Guid("66424F5B-F521-47D2-9958-D6798ADA9690"), () =>
            {
                RuleFor(x => x.Object.KategoriPenyakit).NotEmpty()
                .WithMessage(localizer["Kategori Penyakit cannot be empty"].Value);
            });
            RuleFor(x => x.Object.TotalAbsence).NotEmpty()
                .WithMessage(localizer["Total absence cannot be empty"].Value);

            When(x => x.Object.MandatoryAttachment.Equals(true), () =>
            {
                RuleFor(x => x.Object.SupportingAttachmentPath).NotEmpty();
            });
        }
    }
}
