using System;
using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.Helpers
{
    public static class ServiceHelper
    {
        public static object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }

        /// <summary>
        /// Calculate proxy duration
        /// </summary>
        /// <param name="overtimeIn">Overtime In</param>
        /// <param name="overtimeOut">Overtime Out</param>
        /// <param name="overtimeInAdjust">Overtime In Adjust</param>
        /// <param name="overtimeOutAdjust">Overtime Out Adjust</param>
        /// <param name="normalTimeIn">Normal Time In</param>
        /// <param name="normalTimeOut">Normal Time Out</param>
        /// <param name="overtimeBreak">Overtime Break</param>
        /// <returns>Duration in Hour</returns>
        public static decimal CalculateProxyDuration(DateTime overtimeIn, DateTime overtimeOut, DateTime overtimeInAdjust, DateTime overtimeOutAdjust, DateTime? normalTimeIn, DateTime? normalTimeOut, int overtimeBreak)
        {
            var now = DateTime.Now;
            DateTime substractIn = now;
            DateTime substractOut = now;

            if (normalTimeIn <= overtimeInAdjust && normalTimeOut >= overtimeOutAdjust) return 0;

            if (overtimeOutAdjust >= normalTimeIn && overtimeInAdjust <= normalTimeOut)
            {
                substractIn = overtimeInAdjust <= normalTimeIn ? normalTimeIn.Value : overtimeInAdjust;
                substractOut = overtimeOutAdjust >= normalTimeOut ? normalTimeOut.Value : overtimeOutAdjust;
            }

            return CalculateDuration((decimal)(overtimeOutAdjust - overtimeInAdjust).TotalMinutes - overtimeBreak - (decimal)(substractOut - substractIn).TotalMinutes);
        }
       
        /// <summary>
        /// Calculate duration in hour
        /// </summary>
        /// <param name="minute">Input in Minute</param>
        /// <returns>Duration in Hour</returns>
        public static decimal CalculateDuration(decimal minute)
        {
            var hour = Math.Floor(minute / 60);
            var remainder = minute % 60;
            var adder = (decimal)(remainder >= 30 ? 0.5 : 0.0);

            return hour + adder;
        }

        /// <summary>
        /// Loop every date in date range
        /// </summary>
        /// <param name="dateFrom">Date From</param>
        /// <param name="dateTo">Date To</param>
        /// <returns>each date from parameter range</returns>
        public static IEnumerable<DateTime> EachDay(DateTime dateFrom, DateTime dateTo)
        {
            for (var day = dateFrom.Date; day.Date <= dateTo.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}
