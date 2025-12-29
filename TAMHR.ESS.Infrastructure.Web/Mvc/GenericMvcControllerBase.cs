using System;
using Microsoft.AspNetCore.Mvc;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.Web
{
    public abstract class GenericMvcControllerBase<T, D> : MvcControllerBase
        where D : class, IEntityBase<Guid>
        where T : GenericDomainServiceBase<D>
    {
        /// <summary>
        /// Common service object
        /// </summary>
        protected T CommonService { get { return ServiceProxy.GetService<T>(); } }

        /// <summary>
        /// Main page
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load data form by id
        /// </summary>
        /// <param name="id">Data Form Id</param>
        /// <returns>Data Form</returns>
        [HttpPost]
        public virtual IActionResult Load(Guid id)
        {
            var commonData = CommonService.GetById(id);

            return GetViewData(commonData);
        }

        protected IActionResult GetViewData(object viewData)
        {
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString().Replace("Controller", string.Empty);

            return PartialView($"_{controllerName}Form", viewData);
        }
    }
}
