using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Scheduler
{
    public  class Constanta
    {
        

        public static string ConnectionString()
        {
            return "data source=10.85.44.125;initial catalog=TAMHR_ESS;persist security info=True;user id=sa;password=L96a1awp;MultipleActiveResultSets=True;App=EntityFramework;";
            //return "data source=52.163.124.126;initial catalog=TAMHR_ESS;persist security info=True;user id=sa;password=@git123456789;MultipleActiveResultSets=True;App=EntityFramework;";
        }

        public static string PathLog()
        {
            return "C:\\logs\\ess\\log.txt";
        }
    }
}
