using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using Microsoft.AspNetCore.Authorization;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TAMHR.ESS.WebApi.Controllers
{
    

    //[Route("api/[controller]")]
    [Route("api/digital-attendance")]
    [Authorize(Roles = "HR Administrator")]
    //[ApiController]
    //public class DigitalAttendanceController : ControllerBase
    public class DigitalAttendanceController : ApiControllerBase
    {
        private readonly IConfiguration Configuration;
        SqlConnection conn;
        public DigitalAttendanceController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        public class Res
        {
            public bool Status { get; set; }
            public string Message { get; set; }
        }

        // POST api/<DigitalAttendanceController>
        [HttpPost]
        public ActionResult<object> PostProxy([FromBody] ProxyDigitalAttendance proxyDigitalAttendance)
        {
            var result = new Res();
            result.Status = true;

            try
            {
                string connString = this.Configuration.GetConnectionString("DefaultConnection");
                conn = new SqlConnection(connString);
                // Get and set current date & time.
                var now = DateTime.Now;

                // Get and set current user session noreg.
                var noreg = proxyDigitalAttendance.NoReg;

                if (proxyDigitalAttendance.NoReg != "")
                {
                    conn.Open();

                    SqlCommand comm = new SqlCommand();
                    comm.Connection = conn;
                    comm.CommandText = "SELECT COUNT(*) FROM TB_M_USER " +
                            "WHERE NoReg=@NoReg";
                    comm.Parameters.AddWithValue("@NoReg", proxyDigitalAttendance.NoReg);
                    var checkUserExists = (int)comm.ExecuteScalar();
                    if (checkUserExists > 0)
                    {
                        SqlCommand command = new SqlCommand();
                        command.Connection = conn;
                        command.CommandText = "SELECT COUNT(*) FROM TB_R_PROXY_TIME WHERE NoReg=@NoReg AND WorkingDate=@WorkingDate";
                        command.Parameters.AddWithValue("@NoReg", proxyDigitalAttendance.NoReg);
                        command.Parameters.AddWithValue("@WorkingDate", proxyDigitalAttendance.Tanggal);
                        string strWFH = "";
                        if (proxyDigitalAttendance.Status.ToLower() == "wfh")
                        {
                            strWFH = "'wt-wfh'";
                        }else if (proxyDigitalAttendance.Status.ToLower() == "wff")
                        {
                            strWFH = "'wt-wff'";
                        }
                        else
                        {
                            strWFH = "'wt-"+proxyDigitalAttendance.Status.ToLower()+"'";
                        }

                        var checkData = (int)command.ExecuteScalar();

                        string timeIn = proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + " " + proxyDigitalAttendance.TimeIn.Value.ToString("HH:mm:ss");

                        if (proxyDigitalAttendance.TimeOut == null)
                        {
                            // Proxy in
                            if (checkData > 0)
                            {
                                result.Status = false;
                                result.Message = "Already proxy in, you can only proxy in once";
                            }
                            else
                            {
                                SqlCommand command2 = new SqlCommand();
                                command2.Connection = conn;
                                command2.CommandText = @"
                                    INSERT INTO TB_R_PROXY_TIME 
                                    (Id, NoReg, WorkingDate, ProxyIn, ProxyOut, Geolocation, WorkingTypeCode, CreatedBy, CreatedOn, RowStatus)
                                    VALUES 
                                    (NEWID(), @NoReg, @WorkingDate, @ProxyIn, @ProxyOut, @Geolocation, @WorkingTypeCode, @CreatedBy, GETDATE(), 1);
                                ";

                                command2.Parameters.AddWithValue("@NoReg", proxyDigitalAttendance.NoReg);
                                command2.Parameters.AddWithValue("@WorkingDate", proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd"));
                                command2.Parameters.AddWithValue("@ProxyIn", timeIn);
                                command2.Parameters.AddWithValue("@ProxyOut", timeIn);
                                command2.Parameters.AddWithValue("@Geolocation", proxyDigitalAttendance.GeoTimeIn);
                                command2.Parameters.AddWithValue("@WorkingTypeCode", strWFH);
                                command2.Parameters.AddWithValue("@CreatedBy", proxyDigitalAttendance.NoReg);

                                command2.ExecuteNonQuery();
                                //SqlCommand command2 = new SqlCommand();
                                //command2.Connection = conn;
                                //command2.CommandText = "INSERT INTO TB_R_PROXY_TIME (Id,NoReg,WorkingDate,ProxyIn,ProxyOut,Geolocation,WorkingTypeCode,CreatedBy,CreatedOn,RowStatus)" +
                                //"VALUES(newid(),'" + proxyDigitalAttendance.NoReg + "','" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "','" + timeIn + "','" + timeIn + "','" + proxyDigitalAttendance.GeoTimeIn + "'," + strWFH + ",'" + proxyDigitalAttendance.NoReg + "',GETDATE(),1)";
                                //command2.ExecuteNonQuery();

                                //20241101|Roni|insert update ke tb_r_time_managent diganti ke SP_GENERATE_PROXY
                                //upsert TB_R_TIME_MANAGEMENT
                                //SqlCommand commCheckTM = new SqlCommand();
                                //commCheckTM.Connection = conn;
                                //commCheckTM.CommandText = "SELECT COUNT(*) FROM TB_R_TIME_MANAGEMENT " +
                                //        "WHERE NoReg='" + proxyDigitalAttendance.NoReg + "' AND WorkingDate='" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "'";
                                //var checkDataTM = (int)commCheckTM.ExecuteScalar();
                                //if (checkDataTM == 0)
                                //{
                                //    SqlCommand commTM = new SqlCommand();
                                //    commTM.Connection = conn;
                                //    commTM.CommandText = "INSERT INTO TB_R_TIME_MANAGEMENT " +
                                //        "(Id,NoReg,WorkingDate,WorkingTimeIn,WorkingTimeOut,ShiftCode,AbsentStatus,Description, " +
                                //        "CreatedBy,CreatedOn,RowStatus,NormalTimeIn,NormalTimeOut) " +
                                //        "SELECT newid(),ews.NoReg,ews.Date,'" + timeIn + "' as WorkingTimeIn,'" + timeIn + "' as WorkingTimeOut,ews.ShiftCode,NULL,NULL, " +
                                //        "'" + proxyDigitalAttendance.NoReg + "' as CreatedBy, GETDATE(),1,ews.NormalTimeIN,ews.NormalTimeOUT " +
                                //        "FROM VW_EMPLOYEE_WORK_SCHEDULE ews WHERE NoReg = '" + proxyDigitalAttendance.NoReg + "' " +
                                //        "AND Date = '" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "' ";
                                //    commTM.ExecuteNonQuery();
                                //}
                                //else
                                //{

                                //    SqlCommand commTM = new SqlCommand();
                                //    commTM.Connection = conn;
                                //    commTM.CommandText = "UPDATE TB_R_TIME_MANAGEMENT " +
                                //        "SET WorkingTimeIn = '" + timeIn + "', " +
                                //        "ModifiedBy='" + proxyDigitalAttendance.NoReg + "', " +
                                //        "ModifiedOn = GETDATE() " +
                                //        "WHERE NoReg = '" + proxyDigitalAttendance.NoReg + "' " +
                                //        "AND CONVERT(Varchar(10),WorkingDate,120)='" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "' ";
                                //    commTM.ExecuteNonQuery();
                                //}


                            }
                        }
                        else
                        {
                            // Proxy out
                            if (checkData > 0)
                            {
                                //SqlCommand command2 = new SqlCommand();
                                //command2.Connection = conn;
                                //command2.CommandText = "UPDATE TB_R_PROXY_TIME SET ProxyIn ='" + proxyDigitalAttendance.TimeIn.Value.ToString("yyyy-MM-dd HH:mm:ss") + "',  ProxyOut='" + proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                //    " GeolocationOut='"+proxyDigitalAttendance.GeoTimeOut+"', "+
                                //    " WorkingTypeCode=" + strWFH + ", " +
                                //    " ModifiedBy='" +proxyDigitalAttendance.NoReg+"', "+
                                //    " ModifiedOn=GETDATE() "+
                                //"WHERE NoReg='" + proxyDigitalAttendance.NoReg + "' AND WorkingDate='" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "'";
                                //command2.ExecuteNonQuery();
                                SqlCommand command2 = new SqlCommand();
                                command2.Connection = conn;
                                command2.CommandText = @"
                                    UPDATE TB_R_PROXY_TIME 
                                    SET 
                                        ProxyIn = @ProxyIn,
                                        ProxyOut = @ProxyOut,
                                        GeolocationOut = @GeolocationOut,
                                        WorkingTypeCode = @WorkingTypeCode,
                                        ModifiedBy = @ModifiedBy,
                                        ModifiedOn = GETDATE()
                                    WHERE 
                                        NoReg = @NoReg 
                                        AND WorkingDate = @WorkingDate";

                                command2.Parameters.AddWithValue("@ProxyIn", proxyDigitalAttendance.TimeIn.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                                command2.Parameters.AddWithValue("@ProxyOut", proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                                command2.Parameters.AddWithValue("@GeolocationOut", proxyDigitalAttendance.GeoTimeOut);
                                command2.Parameters.AddWithValue("@WorkingTypeCode", strWFH);
                                command2.Parameters.AddWithValue("@ModifiedBy", proxyDigitalAttendance.NoReg);
                                command2.Parameters.AddWithValue("@NoReg", proxyDigitalAttendance.NoReg);
                                command2.Parameters.AddWithValue("@WorkingDate", proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd"));

                                command2.ExecuteNonQuery();
                            }
                            else
                            {
                                //result.Status = false;
                                //result.Message = "Data Not Found";
                                //SqlCommand command2 = new SqlCommand();
                                //command2.Connection = conn;
                                //command2.CommandText = "INSERT INTO TB_R_PROXY_TIME (Id,NoReg,WorkingDate,ProxyIn,ProxyOut,Geolocation,GeolocationOut,WorkingTypeCode,CreatedBy,CreatedOn,RowStatus)" +
                                //"VALUES(newid(),'" + proxyDigitalAttendance.NoReg + "','" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "','" + timeIn + "','" + proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss") + "','" + proxyDigitalAttendance.GeoTimeIn + "','" + proxyDigitalAttendance.GeoTimeOut + "'," + strWFH + ",'" + proxyDigitalAttendance.NoReg + "',GETDATE(),1)";
                                //command2.ExecuteNonQuery();
                                SqlCommand command2 = new SqlCommand();
                                command2.Connection = conn;
                                command2.CommandText = @"
                                    INSERT INTO TB_R_PROXY_TIME 
                                    (Id, NoReg, WorkingDate, ProxyIn, ProxyOut, Geolocation, GeolocationOut, WorkingTypeCode, CreatedBy, CreatedOn, RowStatus)
                                    VALUES
                                    (NEWID(), @NoReg, @WorkingDate, @ProxyIn, @ProxyOut, @Geolocation, @GeolocationOut, @WorkingTypeCode, @CreatedBy, GETDATE(), 1)";

                                command2.Parameters.AddWithValue("@NoReg", proxyDigitalAttendance.NoReg);
                                command2.Parameters.AddWithValue("@WorkingDate", proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd"));
                                command2.Parameters.AddWithValue("@ProxyIn", timeIn);
                                command2.Parameters.AddWithValue("@ProxyOut", proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                                command2.Parameters.AddWithValue("@Geolocation", proxyDigitalAttendance.GeoTimeIn);
                                command2.Parameters.AddWithValue("@GeolocationOut", proxyDigitalAttendance.GeoTimeOut);
                                command2.Parameters.AddWithValue("@WorkingTypeCode", strWFH);
                                command2.Parameters.AddWithValue("@CreatedBy", proxyDigitalAttendance.NoReg);

                                command2.ExecuteNonQuery();
                            }

                            //20241101|Roni|insert update ke tb_r_time_managent diganti ke SP_GENERATE_PROXY
                            //upsert TB_R_TIME_MANAGEMENT
                            //SqlCommand commCheckTM = new SqlCommand();
                            //commCheckTM.Connection = conn;
                            //commCheckTM.CommandText = "SELECT COUNT(*) FROM TB_R_TIME_MANAGEMENT " +
                            //        "WHERE NoReg='" + proxyDigitalAttendance.NoReg + "' AND WorkingDate='" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "'";
                            //var checkDataTM = (int)commCheckTM.ExecuteScalar();
                            //if (checkDataTM == 0)
                            //{
                            //    string parAbsentStatus = "(CASE " +
                            //    "WHEN NormalTimeIn IS NULL OR '" + proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' = '' THEN NULL " +
                            //        "WHEN DATEDIFF(mi, NormalTimeOut, CAST('" + proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' as DATETIME)) < 0 THEN 36 " +
                            //        "WHEN DATEDIFF(mi, NormalTimeIn, CAST('" + timeIn + "' as DATETIME)) > 180 THEN 35 " +
                            //        "WHEN DATEDIFF(mi, NormalTimeIn, CAST('" + timeIn + "' as DATETIME)) BETWEEN 1 AND 180 THEN 34 " +
                            //        "ELSE 1 " +
                            //    "END)";
                            //    SqlCommand commTM = new SqlCommand();
                            //    commTM.Connection = conn;
                            //    commTM.CommandText = "INSERT INTO TB_R_TIME_MANAGEMENT " +
                            //        "(Id,NoReg,WorkingDate,WorkingTimeIn,WorkingTimeOut,ShiftCode,AbsentStatus,Description, " +
                            //        "CreatedBy,CreatedOn,RowStatus,NormalTimeIn,NormalTimeOut) " +
                            //        "SELECT newid(),ews.NoReg,ews.Date,'" + timeIn + "' as WorkingTimeIn,'" + proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' as WorkingTimeOut,ews.ShiftCode," + parAbsentStatus + ",(CASE " +
                            //        "    WHEN " + parAbsentStatus + " = 0 THEN 'Off' " +
                            //        "    WHEN " + parAbsentStatus + " IN(34, 35) THEN 'Late' " +
                            //        "    WHEN " + parAbsentStatus + " = 36 THEN 'Early Leave' " +
                            //        "    WHEN " + parAbsentStatus + " = 37 THEN 'Absent' " +
                            //        "    ELSE NULL " +
                            //        "END), " +
                            //        "'" + proxyDigitalAttendance.NoReg + "' as CreatedBy, GETDATE(),1,ews.NormalTimeIN,ews.NormalTimeOUT " +
                            //        "FROM VW_EMPLOYEE_WORK_SCHEDULE ews WHERE NoReg = '" + proxyDigitalAttendance.NoReg + "' " +
                            //        "AND Date = '" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "' ";
                            //    commTM.ExecuteNonQuery();
                            //}
                            //else
                            //{
                            //    //update TB_R_TIME_MANAGEMENT

                            //    string parAbsentStatus = "(CASE " +
                            //    "WHEN NormalTimeIn IS NULL OR '" + proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' = '' THEN AbsentStatus " +
                            //        "WHEN DATEDIFF(mi, NormalTimeOut, CAST('" + proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' as DATETIME)) < 0 THEN 36 " +
                            //        "WHEN DATEDIFF(mi, NormalTimeIn, CAST('" + timeIn + "' as DATETIME)) > 180 THEN 35 " +
                            //        "WHEN DATEDIFF(mi, NormalTimeIn, CAST('" + timeIn + "' as DATETIME)) BETWEEN 1 AND 180 THEN 34 " +
                            //        "ELSE 1 " +
                            //    "END)";
                            //    SqlCommand commTM = new SqlCommand();
                            //    commTM.Connection = conn;

                            //    commTM.CommandText = "UPDATE TB_R_TIME_MANAGEMENT " +
                            //        "SET WorkingTimeIn = '" + timeIn + "', " +
                            //        "WorkingTimeOut = '" + proxyDigitalAttendance.TimeOut.Value.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                            //        "AbsentStatus = " + parAbsentStatus + "," +
                            //        " Description = (CASE " +
                            //        "    WHEN " + parAbsentStatus + " = 0 THEN 'Off' " +
                            //        "    WHEN " + parAbsentStatus + " IN(34, 35) THEN 'Late' " +
                            //        "    WHEN " + parAbsentStatus + " = 36 THEN 'Early Leave' " +
                            //        "    WHEN " + parAbsentStatus + " = 37 THEN 'Absent' " +
                            //        "    ELSE NULL " +
                            //        "END)," +
                            //        "ModifiedBy='" + proxyDigitalAttendance.NoReg + "', " +
                            //        "ModifiedOn=GETDATE() " +
                            //        "WHERE NoReg = '" + proxyDigitalAttendance.NoReg + "' " +
                            //        "AND CONVERT(Varchar(10),WorkingDate,120)='" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "' ";

                            //    commTM.ExecuteNonQuery();
                            //}

                        }

                        //20241101|Roni|insert update ke tb_r_time_managent diganti ke SP_GENERATE_PROXY
                        //SqlCommand commTM = new SqlCommand();
                        //commTM.Connection = conn;
                        //commTM.CommandText = "EXEC SP_GENERATE_PROXY '" + proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd") + "','" + proxyDigitalAttendance.NoReg + "' ";
                        //commTM.ExecuteNonQuery();
                        SqlCommand commTM = new SqlCommand();
                        commTM.Connection = conn;
                        commTM.CommandText = "EXEC SP_GENERATE_PROXY @Tanggal, @NoReg";

                        commTM.Parameters.AddWithValue("@Tanggal", proxyDigitalAttendance.Tanggal.ToString("yyyy-MM-dd"));
                        commTM.Parameters.AddWithValue("@NoReg", proxyDigitalAttendance.NoReg);

                        commTM.ExecuteNonQuery();

                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "NoReg Not exists";
                    }


                    conn.Close();
                }
                else
                {
                    result.Status = false;
                    result.Message = "NoReg cannot empty";
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            
            return result;

        }


    }
}
