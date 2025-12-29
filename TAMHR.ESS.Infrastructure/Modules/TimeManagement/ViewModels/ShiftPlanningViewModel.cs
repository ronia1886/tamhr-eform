using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ShiftPlanningViewModel
    {
        public string Year { get; set; }
        public string TypeShift { get; set; }
        public string Month { get; set; }
        public string UploadExcelPath { get; set; }
        public string Remarks { get; set; }
        public List<ShiftPlanningRequestViewModel> Request { get; set; }

    }

    public class ShiftPlanningRequestViewModel{
    
        //public List<DateTime> Dates { get; set; }
        //public List<ShiftPlanningDataViewModel> Shifts { get; set; }
        public DateTime date { get; set; }
        public string noreg { get; set; }
        public string name { get; set; }
        public string shiftCode { get; set; }
        public string shift { get; set; }

    }


    //public class ShiftPlanningDataViewModel
    //{
    //    public ShiftPlanningDataViewModel()
    //    {
    //        this.Shifts = new List<ShiftPlanningDetail>();
    //    }
    //    public string NoReg { get; set; }
    //    public string Name { get; set; }
    //    public List<ShiftPlanningDetail> Shifts { get; set; }
    //}

    //public class ShiftPlanningDetail
    //{
    //    public DateTime ShiftDate { get; set; }
    //    public string ShiftCode { get; set; }
    //}
}
