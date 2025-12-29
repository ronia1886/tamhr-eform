using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using Microsoft.AspNetCore.Authorization;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TAMHR.ESS.WebApi.Controllers
{
    [Route("api/work-schedule")]
    [Authorize(Roles = "HR Administrator")]
    //[ApiController]
    public class WorkScheduleController : ApiControllerBase
    {
        private readonly IConfiguration Configuration;
        SqlConnection conn;
        public WorkScheduleController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        public class Res
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }
        }
        
        public class WorkSchedule
        {
            public Guid id { get; set; }
            public string nik { get; set; }
            public DateTime tgl { get; set; }
            public string working_shift { get; set; }
        }
        // GET api/<WorkScheduleController>
        [HttpGet]
        public ActionResult<object> Gets([FromBody] WorkSchedule workSchedule)
        {
            var result = new Res();
            result.Status = true;

            string connString = this.Configuration.GetConnectionString("DefaultConnection");
            conn = new SqlConnection(connString);
            // Get and set current date & time.

            // Get and set current user session noreg.

            if(workSchedule.nik != "" || workSchedule.tgl != null)
            {
                conn.Open();

                SqlCommand comm = new SqlCommand();
                comm.Connection = conn;
                comm.CommandText = "SELECT COUNT(*) FROM TB_M_USER " +
                            "WHERE NoReg=@NoReg";
                comm.Parameters.AddWithValue("@NoReg", workSchedule.nik);
                var checkUserExists = (int)comm.ExecuteScalar();
                if (checkUserExists > 0)
                {
                    string sqlQuery = @"SELECT ews.[Id]
                  ,ews.[NoReg] as nik
                  ,convert(datetime, ews.[Date], 103) as tgl
                  ,CASE WHEN hd.WorkTypeCode IS NULL THEN NULL 
                        WHEN hd.WorkTypeCode = 'wt-wfh' THEN 'WFH' 
                        ELSE ews.[ShiftCode] 
                    END as working_shift
              FROM[dbo].[VW_EMPLOYEE_WORK_SCHEDULE] ews
              LEFT JOIN TB_M_HEALTH_DECLARATION hd ON ews.NoReg = hd.NoReg AND ews.Date = hd.SubmissionDate
              WHERE getdate() between StartDate AND EndDate
              AND CONVERT(varchar(10),[Date],120)= CONVERT(varchar(10), getdate(), 120)
              AND ews.NoReg=@NoReg AND CONVERT(varchar(10),ews.[Date],120)=@tanggal ";
                    SqlCommand command3 = new SqlCommand();
                    command3.Connection = conn;
                    command3.CommandText = sqlQuery;
                    command3.Parameters.AddWithValue("@NoReg", workSchedule.nik);
                    command3.Parameters.AddWithValue("@tanggal", workSchedule.tgl.ToString("yyyy-MM-dd"));

                    var dataWorkService = command3.ExecuteReader();

                    

                    var wsvModel = new WorkSchedule();
                    while (dataWorkService.Read())
                    {
                        wsvModel.nik = dataWorkService.GetValue(1).ToString();
                        wsvModel.tgl = DateTime.Parse(dataWorkService.GetValue(2).ToString());
                        wsvModel.working_shift = dataWorkService.GetValue(3).ToString();
                    }

                    if (wsvModel != null)
                    {
                        if (wsvModel.working_shift != "")
                        {
                            result.Data = wsvModel;
                        }
                        else
                        {
                            result.Status = false;
                            result.Message = "SHD form must filled first";
                        }
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "work schedule not exists";
                    }
                    
                }
                else
                {
                    result.Status = false;
                    result.Message = "nik not exists";
                }


                conn.Close();
            }
            else
            {
                result.Status = false;
                result.Message = "nik / tgl cannot empty";
            }

            return result;

        }


    }
}
