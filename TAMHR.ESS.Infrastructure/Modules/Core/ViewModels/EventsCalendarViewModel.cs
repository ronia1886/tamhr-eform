using System;
using System.Linq;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using Agit.Common.Extensions;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class EventsViewModel
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public string Description { get; }
        public string ShiftCode { get; }

        public EventsViewModel(DateTime startDate, DateTime endDate, string description)
            : this(startDate, endDate, description, string.Empty)
        {
        }

        public EventsViewModel(DateTime startDate, DateTime endDate, string description, string shiftCode)
        {
            StartDate = startDate;
            EndDate = endDate;
            Description = description;
            ShiftCode = shiftCode;
        }
    }

    public class EventsCalendarViewModel
    {
        public int MonthIndex { get; }
        public IEnumerable<EventsViewModel> Events { get; }
        public IEnumerable<EventsViewModel> EventWithDescriptions { get { return Events.Where(x => !string.IsNullOrEmpty(x.Description)); } }
        public EventsCalendarViewModel(int monthIndex, IEnumerable<EventsViewModel> events)
        {
            MonthIndex = monthIndex;
            Events = events.OrderBy(x => x.StartDate);
        }

        public IEnumerable<DateTime> GetEventDates()
        {
            return EventWithDescriptions.SelectMany(x => Enumerable.Range(0, x.EndDate.Subtract(x.StartDate).Days + 1).Select(d => x.StartDate.AddDays(d)));
        }
    }
}
