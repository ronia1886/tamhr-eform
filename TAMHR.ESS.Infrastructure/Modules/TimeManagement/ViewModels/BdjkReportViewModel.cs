using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class BdjkReportViewModel
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string DocNumber { get; set; }
        public string Noreg { get; set; }
        public string NamaKaryawan { get; set; }
        public DateTime? Tanggal { get; set; }
        public string ProxyIn { get; set; }
        public string ProxyOut { get; set; }
        public string Durasi { get; set; }
        public string Aktifitas { get; set; }
        public string KodeBdjk { get; set; }
        public string Status { get; set; }
        public string UangMakan { get; set; }
        public string UangTaksi { get; set; }
        public string Approver { get; set; }
        public string Description { get; set; }

    }
}
