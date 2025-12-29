using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Others.Controllers
{
    #region API Controller
    /// <summary>
    /// Data request API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.DataRequest)]
    [Permission(PermissionKey.CreateDataRequest)]
    public class DataRequestApiController : FormApiControllerBase<DataRequestViewModel>
    {
        protected override void ValidateOnCreate(string formKey) { }
    }
    #endregion
}