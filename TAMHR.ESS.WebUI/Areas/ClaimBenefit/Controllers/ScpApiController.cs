using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using TAMHR.ESS.Domain;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// SCP API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.Scp)]
    public class ScpApiController : FormApiControllerBase<ScpViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold claim benefit service object.
        /// </summary>
        protected ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);

            ClaimBenefitService.PreValidateScp(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.Name);
            ClaimBenefitService.PreValidateScp(ServiceProxy.UserClaim.NoReg);

        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<ScpViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostCreate(requestDetailViewModel);

            var noreg = ServiceProxy.UserClaim.NoReg != requestDetailViewModel.Requester
                ? ServiceProxy.UserClaim.NoReg
                : requestDetailViewModel.Requester;

            ClaimBenefitService.PreValidatePostScp(noreg, requestDetailViewModel);
        }

        [HttpPost("getloandocument")]
        public async Task<DataSourceResult> GetLoanDocument()
        {
            var result = ClaimBenefitService.GetInfo(ServiceProxy.UserClaim.NoReg);
            var np = Convert.ToInt32(result.FirstOrDefault().GetType().GetProperty("NP").GetValue(result.FirstOrDefault()));

            var formKey = "";

            if(np < 7)
            {
                formKey = ApplicationForm.CompanyLoan36;
            } else
            {
                formKey = ApplicationForm.CompanyLoan;
            }

            var form = FormService.Get(ApplicationForm.Scp);

            var obj = ApprovalService.GetCompleteRequest(formKey, ServiceProxy.UserClaim.NoReg, form.StartDate.Value, form.EndDate.Value);

            //foreach(DocumentApproval da in obj)
            //{
            //    DocumentRequestDetail x = ApprovalService.GetDocumentDetailApprovalById(da.Id);
            //    CompanyLoan y = JsonConvert.DeserializeObject<CompanyLoan>(x.ObjectValue);
            //
            //    if(np < 7 && y.LoanType != "")
            //}

            return await obj.ToDataSourceResultAsync(new DataSourceRequest());
        }
    }
    #endregion
}