using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.Web.Models
{
    public class ExceptionInfo
    {
        public bool Exception { get; set; } = true;
        public string Title { get; set; }
        public string Message { get; set; }

        public ExceptionInfo(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}
