using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class Calendar : ViewComponent
    {
        private readonly CoreService _coreService;

        private readonly DailyWorkScheduleService _dailyWorkScheduleService;

        public Calendar(CoreService coreService, DailyWorkScheduleService dailyWorkScheduleService)
        {
            _coreService = coreService;
            _dailyWorkScheduleService = dailyWorkScheduleService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string noreg = "", int period = 0, bool editMode = false, string workScheduleRule = "")
        {
            period = period == 0 ? DateTime.Now.Year : period;

            var output = !string.IsNullOrEmpty(workScheduleRule)
                ? _coreService.GetEventsCalendarByRule(workScheduleRule, period)
                : (string.IsNullOrEmpty(noreg) ? await _coreService.GetEventsCalendar(period) : _coreService.GetEventsCalendar(noreg, period));

            if (!string.IsNullOrEmpty(noreg) || !string.IsNullOrEmpty(workScheduleRule))
            {
                var shiftCodes = output.SelectMany(x => x.Events).Select(x => x.ShiftCode).Distinct();
                var shifts = _dailyWorkScheduleService.GetDailyWorkSchedules(shiftCodes);

                ViewBag.Shifts = shifts;
            }

            ViewBag.Period = period;
            ViewBag.EditMode = editMode;

            return View(output);
        }
    }
}
