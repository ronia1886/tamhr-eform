using Agit.Common.Utility;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class AbnormalityAbsenceValidator : AbstractValidator<AbnormalityAbsenceView>
    {
        public AbnormalityAbsenceValidator(AbsenceService absenceService, EmployeeLeaveService employeeLeaveService, IStringLocalizer<AbnormalityAbsenceValidator> localizer)
        {
            //string[] exceptionPresenceCode = ["7", "8", "9", "10"];
            List<string> exceptionPresenceCode = new List<string>();
            exceptionPresenceCode.Add("7");
            exceptionPresenceCode.Add("8");
            exceptionPresenceCode.Add("9");
            exceptionPresenceCode.Add("10");

            When(x => !exceptionPresenceCode.Contains(x.AbnormalityAbsenStatus), () =>
            {
                RuleFor(x => x.AbnormaityProxyIn).NotEmpty()
                .WithMessage(localizer["Please select a time"].Value);

                RuleFor(x => x.AbnormalityProxyOut).NotEmpty()
                   .WithMessage(localizer["Please select a time"].Value)
                   .GreaterThanOrEqualTo(x => x.AbnormalityProxyOut)
                   .WithMessage(localizer["End date must be greater than or equal to start date"].Value);
            });

            RuleFor(x => x.AbnormalityAbsenStatus).NotEmpty()
                .WithMessage(localizer["Please select present code"].Value);

            RuleFor(x => x.Reason).NotEmpty()
                .WithMessage(localizer["Reason cannot be empty"].Value);
        }
    }
}
