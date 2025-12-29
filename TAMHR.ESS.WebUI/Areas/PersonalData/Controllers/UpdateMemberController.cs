using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    [Area(ApplicationModule.PersonalData)]
    public class UpdateMemberController : MvcControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Config service
        /// </summary>
        protected PersonalDataService personalDataService => ServiceProxy.GetService<PersonalDataService>();
        #endregion

        public IActionResult Index()
        {
            List<SHEViewModel> listdata = new List<SHEViewModel>();
            //bpjs
            var dataBpjs = personalDataService.GetPersonalDataBpjsQuery();
            foreach (var item in dataBpjs.Where(x => x.FamilyMemberId != null))
            {
                listdata.Add(new SHEViewModel()
                {
                    CommonnId = item.Id,
                    Noreg = item.NoReg,
                    Type = "BPJS",
                    ActionType = item.ActionType,
                    Status = item.CompleteStatus ? "Complete" : "Pending"
                });
            }
            //insurance
            var dataInsurance = personalDataService.GetPersonalDataInsurancesQuery();
            foreach (var item in dataInsurance.Where(x => x.FamilyMemberId != null))
            {
                listdata.Add(new SHEViewModel()
                {
                    CommonnId = item.Id,
                    Noreg = item.NoReg,
                    Type = "ASURANSI",
                    ActionType = item.ActionType,
                    Status = item.CompleteStatus ? "Complete" : "Pending"
                });
            }

            //if (request.Filters.Count == 0)
            //{
            //    listdata = listdata.Where(x => x.Status == "Pending").ToList();
            //}

            return View(listdata);
        }
    }
}