using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Scheduler
{
    public  class Constanta
    {
        

        public static string ConnectionString()
        {
            //return "data source=10.85.65.24;initial catalog=TAMHR_ESS;persist security info=True;user id=sa;password=L96a1awp;MultipleActiveResultSets=True;App=EntityFramework;";
            //return "data source=13.67.70.206\\SQL2019,14344;initial catalog=TAMHR_ESS;persist security info=True;user id=sa;password=@git123456789;MultipleActiveResultSets=True;App=EntityFramework;";
            //return "data source=10.85.11.125;initial catalog=TAMHR_ESS;persist security info=True;user id=sa;password=L96a1awp;MultipleActiveResultSets=True;App=EntityFramework;";
            //return "data source=localhost;initial catalog=TAMHR_ESS;persist security info=True;user id=sa;password=Passw0rd;MultipleActiveResultSets=True;App=EntityFramework;";
            return "data source = VSV-C003-014139; initial catalog = TAMHR_ESS; persist security info = True; user id = sa; password = L96a1awp; MultipleActiveResultSets = True; App = EntityFramework;";

        }

        public static string PathLog()
        {
            return "C:\\logs\\ess\\log.txt";
        }
    }
}
