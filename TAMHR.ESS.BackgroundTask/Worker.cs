using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using log4net;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using RestSharp;
using System.Text.Json;

namespace TAMHR.ESS.BackgroundTask
{
    public class Worker : IWorker
    {
        private readonly ILogger<Worker> logger;
        private readonly IConfiguration Configuration;
        private SqlConnection conn;

        class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
                request.ClientCertificates.Add(new  X509Certificate2("TAM Wildcard.cer"));
                return request;
            }
        }

        public class WorkScheduleModel
        {
            public string id { get; set; }
            public DateTime tgl { get; set; }
            public string nik { get; set; }
            public string working_shift { get; set; }
        }

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            this.logger = logger;
            Configuration = configuration;

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }
        public async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                //Interlocked.Increment(ref number);
                logger.LogInformation($"Worker check data...");
                if (CheckDataWorkSchedule() > 0)
                {
                    CallApi();
                }

                await Task.Delay(1000 * 5);
            }
        }

        #region Work Schedule
        private int CheckDataWorkSchedule()
        {
            string connString = this.Configuration.GetSection("ConnectionString").Value;
            conn = new SqlConnection(connString);

            conn.Open();
            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = @"SELECT COUNT(*) FROM [dbo].[VW_EMPLOYEE_WORK_SCHEDULE] ews
               INNER JOIN TB_M_HEALTH_DECLARATION hd ON ews.NoReg = hd.NoReg AND ews.Date = hd.SubmissionDate
               LEFT JOIN TB_T_EMPLOYEE_WORK_SCHEDULE tews ON ews.NoReg = tews.NoReg AND ews.[Date]= tews.[Date]
                WHERE getdate() between ews.StartDate AND ews.EndDate
              AND CONVERT(varchar(10), ews.[Date], 120) = CONVERT(varchar(10), getdate(), 120)
              AND tews.ID IS NULL AND hd.WorkTypeCode='wt-wfh'
              ";

            var checkData = (int)command.ExecuteScalar();

            conn.Close();

            return checkData;
        }

        private void CallApi()
        {
            string connString = this.Configuration.GetSection("ConnectionString").Value;

            string apiURL = "";
            string jsonParam = "";
            // Log for starting service.
            logger.LogInformation("Calling API...");
            try
            {
                conn = new SqlConnection(connString);

                conn.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                command.CommandText = @"SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='DigitalAttendanceAPI'";
                //logger.LogInformation("Call apiURL...");
                var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    apiURL = reader.GetValue(0).ToString();
                }
                
                SqlCommand command2 = new SqlCommand();
                command2.Connection = conn;
                command2.CommandText = @"SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='DigitalAttendanceKey'";

                //logger.LogInformation("Call authKey...");
                var reader2 = command2.ExecuteReader();
                string authKey = "";
                while (reader2.Read())
                {
                    authKey = reader2.GetValue(0).ToString();

                }

                jsonParam = "";
                

                string sqlQuery = @"SELECT ews.[Id]
                  ,ews.[NoReg] as nik
                  ,convert(varchar(10), ews.[Date], 120) as tgl
                  ,CASE WHEN hd.WorkTypeCode IS NULL THEN NULL 
                        WHEN hd.WorkTypeCode = 'wt-wfh' THEN 'WFH' 
                        --ELSE ews.[ShiftCode] 
                        ELSE 'WFO' 
                    END as working_shift
              FROM [dbo].[VW_EMPLOYEE_WORK_SCHEDULE] ews
              INNER JOIN TB_M_HEALTH_DECLARATION hd ON ews.NoReg = hd.NoReg AND ews.Date = hd.SubmissionDate
              LEFT JOIN TB_T_EMPLOYEE_WORK_SCHEDULE tews ON ews.NoReg = tews.NoReg AND ews.[Date]= tews.[Date]
              WHERE getdate() between  ews.StartDate AND ews.EndDate
              AND CONVERT(varchar(10),ews.[Date],120)= CONVERT(varchar(10), getdate(), 120)
              AND tews.ID IS NULL AND hd.WorkTypeCode='wt-wfh'";
                SqlCommand command3 = new SqlCommand();
                command3.Connection = conn;
                command3.CommandText = sqlQuery;
                //logger.LogInformation("Call dataWorkService...");
                var dataWorkService = command3.ExecuteReader();

                List<WorkScheduleModel> listData = new List<WorkScheduleModel>();
                
                while (dataWorkService.Read())
                {
                    var wsvModel = new WorkScheduleModel();
                    wsvModel.id = dataWorkService.GetValue(0).ToString();
                    wsvModel.nik = dataWorkService.GetValue(1).ToString();
                    wsvModel.tgl = DateTime.Parse(dataWorkService.GetValue(2).ToString());
                    wsvModel.working_shift = dataWorkService.GetValue(3).ToString();
                    listData.Add(wsvModel);
                }

                //logger.LogInformation("Call listData...");
                foreach (var data in listData)
                {
                    var requestBody = new
                    {
                        nik = data.nik,
                        date = data.tgl.ToString("yyyy-MM-dd"),
                        shift = data.working_shift
                    };

                    //var webClient = new WebClient();
                    //webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                    //var test = jsonParam.Replace("\"{", "{").Replace("}\"", "}");
                    //byte[] bytes = Encoding.ASCII.GetBytes(requestBody.ToString());

                    //webClient.Encoding = System.Text.Encoding.UTF8;
                    //webClient.Headers[HttpRequestHeader.Host] = "api.greatdayhr.com";
                    //webClient.Headers["api-key"] = authKey;


                    //webClient.UploadString(apiURL, "POST", jsonParam);

                    logger.LogInformation("POST Data to API...");
                    jsonParam = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                    var client = new RestSharp.RestClient(apiURL);
                    //client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                    var request = new RestRequest(apiURL, Method.Post);
                    request.AddHeader("api-key", authKey);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Cache-Control", "no-cache");
                    request.AddParameter("application/json", jsonParam, ParameterType.RequestBody);
                    var response = client.Execute(request);

                    if (response.StatusCode==HttpStatusCode.Created) {
                        //SqlCommand command5 = new SqlCommand();
                        //command5.Connection = conn;
                        //command5.CommandText = @"INSERT INTO [dbo].[TB_T_EMPLOYEE_WORK_SCHEDULE] (Id,NoReg,Date,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)
                        //     VALUES (newid(),'" + data.nik + "','" + data.tgl.ToString("yyyy-MM-dd") + "','System',getdate(),null,null)";
                        //command5.ExecuteNonQuery();
                        //logger.LogInformation("Success to call API");
                        SqlCommand command5 = new SqlCommand();
                        command5.Connection = conn;
                        command5.CommandText = @"
                        INSERT INTO [dbo].[TB_T_EMPLOYEE_WORK_SCHEDULE] 
                            (Id, NoReg, Date, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
                        VALUES 
                            (NEWID(), @NoReg, @Date, 'System', GETDATE(), NULL, NULL)";

                        command5.Parameters.AddWithValue("@NoReg", data.nik);
                        command5.Parameters.AddWithValue("@Date", data.tgl.ToString("yyyy-MM-dd"));

                        command5.ExecuteNonQuery();
                        logger.LogInformation("Success to call API");
                    }
                    else
                    {
                        logger.LogInformation("Failed to call API");
                    }
                    logger.LogInformation("URL=" + apiURL + ", Parameter=" + jsonParam);
                    logger.LogInformation(response.Content);

                }
                conn.Close();

            }
            //catch (Exception e)
            //{

            //    logger.LogInformation("URL=" + apiURL + ", Parameter=" + jsonParam);
            //    logger.LogInformation(e.Message);


            //}
            catch (WebException e)
            {
                using (var sr = new StreamReader(e.Response.GetResponseStream()))
                {
                    var data = sr.ReadToEnd();
                    logger.LogInformation("URL=" + apiURL + ", Parameter=" + jsonParam);
                    logger.LogInformation(e.Message + "[" + data + "]");
                }

            }
        }

        #endregion
        public static byte[] ObjectToByteArray(object obj)
        {
            string json = JsonSerializer.Serialize(obj);
            return Encoding.UTF8.GetBytes(json);
        }

    }
}
