using System.Collections.Generic;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class DashboardHomeViewModel
    {
        private string _clockIn;
        private string _clockOut;

        public int TotalNotice { get; set; }
        public int TotalTask { get; set; }
        public string ShiftCode { get; set; }
        public string ClockIn {
            get
            {
                return string.IsNullOrEmpty(_clockIn) ? "?" : _clockIn;
            }
            set
            {
                _clockIn = value;
            }
        }
        public string ClockOut
        {
            get
            {
                return string.IsNullOrEmpty(_clockOut) || _clockIn == _clockOut ? "?" : _clockOut;
            }
            set
            {
                _clockOut = value;
            }
        }
        public string TaxStatus { get; set; }
        public int AnnualLeave { get; set; }
        public int LongLeave { get; set; }
        public int TotalAbsence { get; set; }
        public IEnumerable<TimeManagementDashboardStoredEntity> TimeManagementDashboard { get; set; }
    }
}
