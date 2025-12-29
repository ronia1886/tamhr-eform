
using Agit.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.ComponentModel.DataAnnotations;

namespace TAMHR.ESS.Domain
{
    [Table("VW_TOTAL_WORK_DAY_CHART_FRSR")]
    public partial class TotalWorkDayChartFRSRModel : IEntityMarker
    {
        [Key] // Tambahkan atribut ini untuk menandai RowNum sebagai primary key
        public int totalWorkDay { get; set; }
    }

    [Table("VW_DASHBOARD_CHART_AREA_ACTIVITY_BY_FILTER")]
    public partial class DashboardChartAreaActivityByFilterViewModel : IEntityMarker
    {
        [Key] // Tambahkan atribut ini untuk menandai RowNum sebagai primary key
        public string categoryChart { get; set; }
        public string category { get; set; }
        public Int32 value { get; set; }
        public string periode { get; set; }
        public string AreaId { get; set; }
        public string DivisionCode { get; set; }
    }

    [Table("VW_DASHBOARD_CHART_AREA_ACTIVITY")]
    public partial class DashboardChartAreaActivityViewModel : IEntityMarker
    {
        [Key] // Tambahkan atribut ini untuk menandai RowNum sebagai primary key
        public string categoryChart { get; set; }
        public string category{ get; set; }
        public Int32 value { get; set; }
        public string periode { get; set; }
    }

    [Table("VW_DASHBOARD_LIST_AREA_ACTIVITY")]
    public partial class DashboardListAreaActivityViewModel : IEntityMarker
    {
        [Key] // Tambahkan atribut ini untuk menandai RowNum sebagai primary key
        public Int64 RowNum { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public Guid AreaId { get; set; }
        public string AreaName { get; set; }
        public string Periode { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }

    }
}