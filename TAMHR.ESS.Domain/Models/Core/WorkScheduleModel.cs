using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    public class WorkScheduleModel
    {
        public string id { get; set; }
        public DateTime tgl { get; set; }
        public string nik { get; set; }
        public string working_shift { get; set; }
    }
}
