using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain.Models.TimeManagement
{
    public class ProxyTimeLog
    {
        public string NoReg { get; set; }
        public DateTime? ProxyInBefore { get; set; }
        public DateTime? ProxyOutBefore { get; set; }
        public DateTime? ProxyInAfter { get; set; }
        public DateTime? ProxyOutAfter { get; set; }
        public string ChangeState { get; set; }  // "Before" atau "After"
        public DateTime ChangeTime { get; set; }
    }
}
