using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TAMHR.ESS.Infrastructure;

namespace HybridWorkPlanAPI.Controllers
{
    [ApiController]
    [Route("api/get-plan")]
    [Authorize]
    public class GetPlanController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DbContextOptions<UnitOfWork> _dbContextOptions;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GetPlanController> _logger;

        public GetPlanController(IConfiguration configuration, ILogger<GetPlanController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public class WorkPlanRequest
        {
            public DateTime? LastUpdate { get; set; } // createdon
            public DateTime? ShiftDateStart { get; set; } // startdate
            public DateTime? ShiftDateEnd { get; set; } // enddate
            public List<string> EmployeeNo { get; set; } // submitter
            public List<string> ShiftType { get; set; } // shiftType
        }

        public class WorkPlanResponse
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public List<WorkPlanData> Data { get; set; }
        }

        public class WorkPlanData
        {
            public string EmployeeNo { get; set; } // noreg
            public string Shift { get; set; } // workplace
            public DateTime ShiftDate { get; set; } // workingdate
        }

        [HttpPost]
        [Authorize(Policy = "Allowed")]
        public IActionResult GetHybridWorkPlan([FromBody] WorkPlanRequest request)
        {
            if (request == null)
            {
                _logger.LogError("Request is null.");
                return BadRequest(new { Message = "Request cannot be null." });
            }

            var responseData = new List<WorkPlanData>();

            try
            {
                //string connString = _configuration.GetConnectionString("DefaultConnection");
                var unitOfWork = new UnitOfWork(_dbContextOptions, _httpContextAccessor);
                var connection = unitOfWork.GetConnection() as SqlConnection;

                //using (SqlConnection conn = new SqlConnection(connection))
                using (connection)
                {
                    connection.Open();

                    var employeeNos = new List<string>();
                    if (request.EmployeeNo == null || !request.EmployeeNo.Any())
                    {
                        using (SqlCommand command = new SqlCommand("SELECT NoReg FROM MDM_ACTUAL_ORGANIZATION_STRUCTURE WHERE Staffing = 100", connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    employeeNos.Add(reader["NoReg"].ToString());
                                }
                            }
                        }

                        if (!employeeNos.Any())
                        {
                            _logger.LogInformation("No employees found with Staffing = 100.");
                            return Ok(new WorkPlanResponse { Status = true, Message = "No employees found with Staffing = 100", Data = responseData });
                        }
                    }
                    else
                    {
                        employeeNos = request.EmployeeNo;
                    }

                    var employeeNoParams = string.Join(", ", employeeNos.Select((e, i) => $"@EmployeeNo{i}"));

                    var shiftTypeParams = "";
                    if (request.ShiftType != null && request.ShiftType.Any())
                    {
                        shiftTypeParams = "AND WorkPlace IN (" + string.Join(", ", request.ShiftType.Select((s, i) => $"@ShiftType{i}")) + ")";
                    }

                    string query;
                    if (request.LastUpdate.HasValue)
                    {
                        query = $@"
                            WITH LatestVersion AS
                            (
                                SELECT 
                                d.NoReg, 
                                d.WorkPlace, 
                                d.WorkingDate, 
                                d.WeeklyWfhPlanningId, 
                                p.Version,
                                ROW_NUMBER() OVER (PARTITION BY d.NoReg, d.WorkingDate ORDER BY p.Version DESC) AS lv
                                FROM TB_R_WEEKLY_WFH_PLANNING_DETAIL d
                                INNER JOIN TB_R_WEEKLY_WFH_PLANNING p ON d.WeeklyWfhPlanningId = p.Id
                                WHERE d.CreatedOn >= @LastUpdate
                                AND d.WorkingDate >= @ShiftDateStart
                                AND d.WorkingDate <= @ShiftDateEnd
                                
                            )
                            SELECT NoReg, WorkPlace, WorkingDate
                            FROM LatestVersion
                            WHERE lv = 1
                                AND WorkPlace!='ABS'
                                AND NoReg IN ({employeeNoParams})
                                {shiftTypeParams}
                            ORDER BY WorkingDate ASC";
                    }
                    else
                    {
                        query = $@"
                            WITH LatestVersion AS
                            (
                                SELECT 
                                d.NoReg, 
                                d.WorkPlace, 
                                d.WorkingDate, 
                                d.WeeklyWfhPlanningId, 
                                p.Version,
                                ROW_NUMBER() OVER (PARTITION BY d.NoReg, d.WorkingDate ORDER BY p.Version DESC) AS lv
                                FROM TB_R_WEEKLY_WFH_PLANNING_DETAIL d
                                INNER JOIN TB_R_WEEKLY_WFH_PLANNING p ON d.WeeklyWfhPlanningId = p.Id
                                WHERE d.WorkingDate >= @ShiftDateStart
                                AND d.WorkingDate <= @ShiftDateEnd
                                
                            )
                            SELECT NoReg, WorkPlace, WorkingDate
                            FROM LatestVersion
                            WHERE lv = 1
                                AND WorkPlace!='ABS'
                                AND NoReg IN ({employeeNoParams})
                                {shiftTypeParams}
                            ORDER BY WorkingDate ASC";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (request.LastUpdate.HasValue)
                        {
                            command.Parameters.Add(new SqlParameter("@LastUpdate", request.LastUpdate));
                        }
                        command.Parameters.Add(new SqlParameter("@ShiftDateStart", request.ShiftDateStart));
                        command.Parameters.Add(new SqlParameter("@ShiftDateEnd", request.ShiftDateEnd));

                        for (int i = 0; i < employeeNos.Count; i++)
                        {
                            command.Parameters.Add(new SqlParameter($"@EmployeeNo{i}", employeeNos[i]));
                        }

                        if (request.ShiftType != null && request.ShiftType.Any())
                        {
                            for (int i = 0; i < request.ShiftType.Count; i++)
                            {
                                command.Parameters.Add(new SqlParameter($"@ShiftType{i}", request.ShiftType[i]));
                            }
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                responseData.Add(new WorkPlanData
                                {
                                    EmployeeNo = reader["NoReg"].ToString(),
                                    Shift = reader["WorkPlace"].ToString(),
                                    ShiftDate = Convert.ToDateTime(reader["WorkingDate"])
                                });
                            }
                        }
                    }
                }

                var response = new WorkPlanResponse
                {
                    Status = true,
                    Message = null,
                    Data = responseData
                };

                return Ok(response);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "A database error occurred.");
                return StatusCode(500, new { Message = "A database error occurred.", Details = sqlEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An internal server error occurred.");
                return StatusCode(500, new { Message = "An internal server error occurred.", Details = ex.Message });
            }
        }
    }
}
