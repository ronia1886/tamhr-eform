using FluentValidation;
using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class GenericIdViewModel<T>
    {
        public T[] Ids { get; set; }
    }
}
