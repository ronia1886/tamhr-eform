using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;
using System.Globalization;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class HealthDeclarationValidator : AbstractDocumentValidator<HealthDeclarationViewModel>
    {
        public HealthDeclarationValidator(ConfigService configService, FormService formService, IStringLocalizer<HealthDeclarationValidator> localizer)
        {
            RuleFor(x => x.Object.PhoneNumber).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .Must(x => new Regex("^[0-9]+$").IsMatch(x))
                .WithMessage(localizer["Must be numeric value"].Value);

            RuleFor(x => x.Object.Email).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .EmailAddress()
                .WithMessage(localizer["Must be a valid email address"].Value);

            RuleFor(x => x.Object.EmergencyFamilyStatus).NotEmpty()
                .WithMessage(localizer["Must select this field"].Value);
            
            RuleFor(x => x.Object.EmergencyName).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value);
            
            RuleFor(x => x.Object.EmergencyPhoneNumber).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
            .Must((data, x) =>
             {
                 var output = true;
                 if (data.Object.PhoneNumber == data.Object.EmergencyPhoneNumber)
                 {
                     output = false;
                 }
                 return output;
             })
                .WithMessage(localizer["This field cannot same as Phone Number"].Value)
            .Must(x => new Regex("^[0-9]+$").IsMatch(x))
                .WithMessage(localizer["Must be numeric value"].Value);

            RuleFor(x => x.Object.HaveFever).NotNull()
                .WithMessage(localizer["Must select this field"].Value);

            RuleFor(x => x.Object.HealthDeclarationFilled).NotNull()
                .WithMessage(localizer["Must select this field"].Value);

            RuleFor(x => x.Object.WorkTypeCode).NotEmpty()
                .WithMessage(localizer["Must select this field"].Value);

            RuleFor(x => x.Object.BodyTemperature).NotEmpty()
                .WithMessage(localizer["Must select this field"].Value);

            RuleFor(x => x.Object.FormAnswers).Custom((data, context) =>
            {
                var formQuestions = formService.GetFormQuestions(ApplicationForm.HealthDeclaration)
                    .Where(x => x.RowStatus);

                var roots = formQuestions.Where(x => x.ParentFormQuestionId == null);
                
                var questionIds = data.Select(x => x.FormQuestionId).ToList();

                foreach (var root in roots)
                {
                    var items = formQuestions.Where(x => x.ParentFormQuestionId == root.Id)
                        .Select(x => x.Id);

                    if (items.Intersect(questionIds).Count() != items.Count())
                    {
                        context.AddFailure("Object.FormAnswers." + root.Id, localizer["* This question requires one response per line"].Value);
                    }
                }
            });

            When(x => x.Object.BodyTemperature == "others", () =>
            {
                var formatProvider = new CultureInfo("en-US");

                // Get and set minimum temperature value.
                var min = configService.GetConfigValue<double>("HealthDeclaration.MinTemperatureValue", 0, formatProvider: formatProvider);

                // Get and set maximum temperature value.
                var max = configService.GetConfigValue<double>("HealthDeclaration.MaxTemperatureValue", 42, formatProvider: formatProvider);

                RuleFor(x => x.Object.BodyTemperatureOtherValue).NotEmpty()
                    .WithMessage(localizer["This field cannot be empty"].Value)
                    .Must(x => double.TryParse(x, NumberStyles.AllowDecimalPoint, formatProvider, out double num))
                    .WithMessage(localizer["Must be numeric value"].Value)
                    .Must(x =>
                    {
                        double.TryParse(x, NumberStyles.AllowDecimalPoint, formatProvider, out double num);

                        return num >= min && num <= max;
                    })
                    .WithMessage(string.Format(localizer["Must be between {0} and {1}"].Value, min, max));
            });
        }
    }
}
