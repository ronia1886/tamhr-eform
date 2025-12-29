using Agit.Domain;
using Microsoft.AspNetCore.Mvc;

namespace TAMHR.ESS.Infrastructure.Web.Extensions
{
    public static class ControllerBaseExtensions
    {
        public static T GetService<T>(this ControllerBase controllerBase) where T : DomainServiceBase
        {
            return (T)controllerBase.HttpContext.RequestServices.GetService(typeof(T));
        }
    }
}
