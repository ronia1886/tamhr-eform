using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    [Area("Core")]
    public class SettingsController : MvcControllerBase
    {
        protected CoreService CoreService { get { return ServiceProxy.GetService<CoreService>(); } }
        protected AccessRoleService AccessRoleService { get { return ServiceProxy.GetService<AccessRoleService>(); } }
        protected ApprovalMatrixService ApprovalMatrixService { get { return ServiceProxy.GetService<ApprovalMatrixService>(); } }

        private readonly Dictionary<string, Func<Guid, IActionResult>> _handlers = new Dictionary<string, Func<Guid, IActionResult>>();

        public SettingsController()
        {
            _handlers.Add("config", LoadConfig);
            _handlers.Add("language", LoadLanguage);
            _handlers.Add("emailtemplate", LoadEmailTemplate);
            _handlers.Add("eventscalendar", LoadEventsCalendar);
            _handlers.Add("category", LoadGeneralCategory);
            _handlers.Add("approvalmatrix", LoadApprovalMatrix);
            _handlers.Add("user-impersonation", LoadUserImpersonation);
            _handlers.Add("access-role", LoadAccessRole);
        }

        [Permission(PermissionKey.ViewConfig)]
        public IActionResult Configs()
        {
            return View();
        }

        [Permission(PermissionKey.ViewGeneralCategory)]
        public IActionResult GeneralCategories()
        {
            return View();
        }

        [Permission(PermissionKey.ViewForm)]
        public IActionResult Forms()
        {
            return View();
        }

        [Permission(PermissionKey.ViewNews)]
        public IActionResult News()
        {
            return View();
        }

        [Permission(PermissionKey.ViewLanguage)]
        public IActionResult Languages()
        {
            return View();
        }

        [Permission(PermissionKey.ViewApprovalMatrix)]
        public IActionResult ApprovalMatrices()
        {
            return View();
        }

        [Permission(PermissionKey.ViewEventsCalendar)]
        public IActionResult EventsCalendar()
        {
            return View();
        }

        [Permission(PermissionKey.ViewEmailTemplate)]
        public IActionResult EmailTemplates()
        {
            return View();
        }

        [Permission(PermissionKey.ViewUserImpersonation)]
        public IActionResult UserImpersonations()
        {
            return View();
        }

        [Permission(PermissionKey.ViewAccessRole)]
        public IActionResult AccessRoles()
        {
            return View();
        }

        [Permission(PermissionKey.ListHash)]
        public IActionResult UserHash()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Load(Guid id, string handler)
        {
            return _handlers[handler](id);
        }

        [Permission(PermissionKey.ManageEventsCalendar)]
        private IActionResult LoadEventsCalendar(Guid id)
        {
            var eventsCalendar = CoreService.GetEventsCalendar(id);

            if (id == Guid.Empty)
            {
                var date = DateTime.ParseExact(Request.Form["date"], "MM/dd/yyyy", CultureInfo.CurrentCulture);

                eventsCalendar.StartDate = date;
                eventsCalendar.EndDate = date;
            }

            return PartialView("_EventsCalendarForm", eventsCalendar);
        }

        [Permission(PermissionKey.ManageEmailTemplate)]
        private IActionResult LoadEmailTemplate(Guid id)
        {
            var emailTemplate = CoreService.GetEmailTemplate(id);

            return PartialView("_EmailTemplateForm", emailTemplate);
        }

        [Permission(PermissionKey.ManageLanguage)]
        private IActionResult LoadLanguage(Guid id)
        {
            var language = CoreService.GetLanguage(id);

            return PartialView("_LanguageForm", language);
        }

        [Permission(PermissionKey.ManageGeneralCategory)]
        private IActionResult LoadGeneralCategory(Guid id)
        {
            var generalCategory = ConfigService.GetGeneralCategory(id);

            return PartialView("_GeneralCategoryForm", generalCategory);
        }

        [Permission(PermissionKey.ManageConfig)]
        private IActionResult LoadConfig(Guid id)
        {
            var config = ConfigService.GetConfig(id);

            return PartialView("_ConfigForm", config);
        }

        [Permission(PermissionKey.ManageApprovalMatrix)]
        private IActionResult LoadApprovalMatrix(Guid id)
        {
            var approvalMatrix = ApprovalMatrixService.Get(id);

            return PartialView("_ApprovalMatrixForm", approvalMatrix);
        }

        [Permission(PermissionKey.ManageUserImpersonation)]
        private IActionResult LoadUserImpersonation(Guid id)
        {
            var userImpersonation = CoreService.GetUserImpersonation(id);

            return PartialView("_UserImpersonationForm", userImpersonation);
        }

        [Permission(PermissionKey.ManageAccessRole)]
        private IActionResult LoadAccessRole(Guid id)
        {
            var accessRole = AccessRoleService.Get(id);

            return PartialView("_AccessRoleForm", accessRole);
        }
    }
}