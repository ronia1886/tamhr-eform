using System;

namespace TAMHR.ESS.Infrastructure.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime;
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime;

            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
    }
}
