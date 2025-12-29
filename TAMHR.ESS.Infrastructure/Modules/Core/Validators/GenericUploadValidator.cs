using System.Linq;
using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class GenericUploadValidator : AbstractValidator<GenericUploadViewModel>
    {
        public GenericUploadValidator()
        {
            RuleFor(x => x.Id)
                .Must((obj, x) => obj.Attachments != null && obj.Attachments.Any(y => y.FieldCategory == "AttachmentFilePath"))
                .WithMessage("Please upload a file")
                .OverridePropertyName("AttachmentFilePath");
        }
    }
}
