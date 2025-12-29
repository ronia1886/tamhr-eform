using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    #region API Controller
    /// <summary>
    /// Others personal data API controller.
    /// </summary>
    [Route("api/drivers-license")]
    //[Permission(PermissionKey.ManageDriversLicensePersonalData)]
    public class DriversLicenseController : FormApiControllerBase<DriverLicenseViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold personal data service object.
        /// </summary>
        protected PersonalDataService personalDataService => ServiceProxy.GetService<PersonalDataService>();
        protected DriversLicenseService driversLicenseService => ServiceProxy.GetService<DriversLicenseService>();

        //[HttpPost("save-datas")]
        //public IActionResult SaveDriversLicense([FromBody]DriverLicenseViewModel driverslicense)
        //{
        //    //trainingIdp.TrainingParticipantId = id;
        //    var output = personalDataService.SaveDriversLicense(driverslicense);

        //    return NoContent();
        //}
        [HttpPost("submit-all")]
        public IActionResult Confirm([FromBody]DriverLicenseViewModel entity)
        {
            var isValid = driversLicenseService.SaveDriverLicense(entity);
            return Ok(isValid);
        }
        #endregion
    }
    #endregion
}