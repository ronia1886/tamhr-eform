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
    [Route("api/familyregist")]
    [Permission(PermissionKey.ManageFamilyRegistPersonalData)]
    public class FamilyRegistController : FormApiControllerBase<FamilyRegistrationViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold personal data service object.
        /// </summary>
        protected PersonalDataService personalDataService => ServiceProxy.GetService<PersonalDataService>();
        //protected PassportService passportService => ServiceProxy.GetService<PassportService>();

        //[HttpPost("save-datas")]
        //public IActionResult SaveDriversLicense([FromBody]PassportViewModel passport)
        //{
        //    //trainingIdp.TrainingParticipantId = id;
        //    var output = personalDataService.SaveDriversLicense(driverslicense);

        //    return NoContent();
        //}
        #endregion
    }
    #endregion
}