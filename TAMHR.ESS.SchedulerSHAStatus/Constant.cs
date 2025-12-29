using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.SchedulerSHAStatus
{
    public  class Constanta
    {
        

        public static string ConnectionString()
        {
            //return "data source=10.85.44.24;initial catalog=TAMHR_ESS_PRD_20210223;persist security info=True;user id=sa;password=L96a1awp;MultipleActiveResultSets=True;App=EntityFramework;";
            return "data source=52.163.124.126;initial catalog=TAMHR_ESS;persist security info=True;user id=sa;password=@git123456789;MultipleActiveResultSets=True;App=EntityFramework;";
        }

        public static string PathLog()
        {
            return "C:\\logs\\ess\\log.txt";
        }
    }
}
