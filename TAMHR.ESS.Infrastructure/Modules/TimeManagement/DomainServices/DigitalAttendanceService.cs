using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Agit.Domain.Extensions;
using Dapper;
using Scriban;
using Z.EntityFramework.Plus;
using TAMHR.ESS.Infrastructure.Helpers;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// SPKL Service Class
    /// </summary>
    public class DigitalAttendanceService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// WorkScheduler repository
        /// </summary>
        protected IReadonlyRepository<WorkScheduleViewModel> WorkScheduleRepository => UnitOfWork.GetRepository<WorkScheduleViewModel>();
        #endregion
        private readonly ILogger<DigitalAttendanceService> logger;
        private readonly ConfigService _configService;
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructor
        public DigitalAttendanceService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public class Res
        {
            public bool Status { get; set; }
            public string Message { get; set; }
        }
        #endregion
        public List<WorkScheduleViewModel> Gets()
        {
            return UnitOfWork.GetConnection().Query<WorkScheduleViewModel>(@"SELECT TOP 10 [Id]
                  ,ews.[Date] as tgl
                  ,ews.[NoReg] as nik
                  ,,CASE WHEN hd.WorkTypeCode='wt-wfh' THEN 'WFH' ELSE ews.[ShiftCode] END  as working_shift
              FROM [dbo].[VW_EMPLOYEE_WORK_SCHEDULE] ews
              INNER JOIN TB_M_HEALTH_DECLARATION hd ON ews.NoReg=hd.NoReg AND ews.Date=hd.SubmissionDate
              WHERE getdate() between StartDate AND EndDate
              AND CONVERT(varchar(10),[Date],120)=CONVERT(varchar(10),getdate(),120)").ToList();
        }

        public int CheckDataWorkSchedule()
        {
            return UnitOfWork.GetConnection().QuerySingleOrDefault<int>(@"SELECT COUNT(*) FROM [dbo].[VW_EMPLOYEE_WORK_SCHEDULE] ews
                LEFT JOIN TB_T_EMPLOYEE_WORK_SCHEDULE tews ON ews.NoReg=tews.NoReg AND ews.[Date]=tews.[Date]
                WHERE getdate() between ews.StartDate AND ews.EndDate
              AND CONVERT(varchar(10),ews.[Date],120)=CONVERT(varchar(10),getdate(),120)
			  AND tews.ID IS NULL
              ");
        }

        public async Task CallApi()
        {
            var res = new Res();
            // Log for starting service.
            logger.LogInformation("Calling API...");
            //log.Info("Calling API...");
            try
            {
                string apiURL = "";
                string authKey = "";
                var dataAPIURL = UnitOfWork.GetConnection().Query<Config>(@"SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='DigitalAttendanceAPI'").FirstOrDefault();
                apiURL = dataAPIURL.ConfigValue;

                //var dataAuthKey = UnitOfWork.GetConnection().Query<Config>(@"SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='DigitalAttendanceKey'").FirstOrDefault();
                //authKey = dataAuthKey.ConfigValue;

                authKey = _configService.GetConfigValue<string>(Configurations.DigitalAttendanceKey);

                string jsonParam = "";


                string sqlQuery = @"SELECT ews.[Id]
                  ,ews.[NoReg] as nik
                  ,convert(datetime, ews.[Date], 103) as tgl
                  ,CASE WHEN hd.WorkTypeCode IS NULL THEN NULL 
                        WHEN hd.WorkTypeCode = 'wt-wfh' THEN 'WFH' 
                        ELSE ews.[ShiftCode] 
                    END as working_shift
              FROM[dbo].[VW_EMPLOYEE_WORK_SCHEDULE] ews
              INNER JOIN TB_M_HEALTH_DECLARATION hd ON ews.NoReg = hd.NoReg AND ews.Date = hd.SubmissionDate
              WHERE getdate() between StartDate AND EndDate
              AND CONVERT(varchar(10),[Date],120)= CONVERT(varchar(10), getdate(), 120)";
                
                var listData = UnitOfWork.GetConnection().Query<WorkScheduleModel>(sqlQuery).ToList();
                foreach (var data in listData)
                {
                    var requestBody = new
                    {
                        nik = data.nik,
                        date = data.tgl.ToString("yyyy-MM-dd"),
                        shift = data.working_shift
                    };

                    var webClient = new WebClient();
                    //webClient.Headers[HttpRequestHeader.Authorization] = @"Bearer " + authKey;
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    jsonParam = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                    var test = jsonParam.Replace("\"{", "{").Replace("}\"", "}");
                    //byte[] bytes = Encoding.ASCII.GetBytes(requestBody.ToString());
                    //webClient.Headers[HttpRequestHeader.ContentLength] = "52;";

                    webClient.Encoding = System.Text.Encoding.UTF8;
                    webClient.Headers[HttpRequestHeader.Host] = "api.greatdayhr.com";
                    webClient.Headers["api-key"] = authKey;
                    webClient.UploadString(apiURL, "POST", jsonParam);


                    UnitOfWork.GetConnection().Query(@"INSERT INTO TB_T_EMPLOYEE_WORK_SCHEDULE (Id,NoReg,Date,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
                         VALUES (newid(),'" + data.nik + "',convert(datetime, '" + data.tgl.ToString() + "', 103),'System',getdate(),null,null)");
                    
                    logger.LogInformation("URL=" + apiURL + ", Parameter=" + jsonParam);
                    logger.LogInformation("Success to call API");

                    //log.Info("URL=" + apiURL + ", Parameter=" + jsonParam);
                    //log.Info("Success to call API");
                }

                res.Status = true;
                res.Message = "Success to Call API";

            }
            catch (Exception e)
            {
                
                logger.LogInformation(e.Message);
                res.Status = true;
                res.Message = e.Message;
            }

        }
    }
}
