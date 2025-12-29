using System;
using FluentValidation;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validator
{
    public abstract class AbstractDocumentValidator<T>
        : AbstractValidator<DocumentRequestDetailViewModel<T>> where T : class
    {
        protected const int MaximumLength = ApplicationConstants.RemarksMaxLength;
    }
}
