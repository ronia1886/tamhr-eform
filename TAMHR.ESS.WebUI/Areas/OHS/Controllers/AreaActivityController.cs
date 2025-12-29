using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Domain;
using Kendo.Mvc.UI;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using TAMHR.ESS.Infrastructure;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.WebUI.Areas.OHS.Controllers
{
    #region API Controller
    /// <summary>
    /// API Area Activity OHS
    /// </summary>
    [Route("api/ohs/area-activity")]
    public class AreaActivityController : ApiControllerBase
    {
        #region SERVICES
        public TotalEmployeeService TotalEmployeeService => ServiceProxy.GetService<TotalEmployeeService>();
        public SafetyIncidentService SafetyIncidentService => ServiceProxy.GetService<SafetyIncidentService>();
        public SafetyFacilityService SafetyFacilityService => ServiceProxy.GetService<SafetyFacilityService>();
        public FireProtectionService FireProtectionService => ServiceProxy.GetService<FireProtectionService>();
        public APARRefillService APARRefillService => ServiceProxy.GetService<APARRefillService>();
        public TrainingRecordService TrainingRecordService => ServiceProxy.GetService<TrainingRecordService>();
        public ProjectActivityService ProjectActivityService => ServiceProxy.GetService<ProjectActivityService>();
        public AreaService AreaService => ServiceProxy.GetService<AreaService>();
        public EquipmentService EquipmentService => ServiceProxy.GetService<EquipmentService>();
        public DashboardAreaActivityService DashboardAreaActivityService => ServiceProxy.GetService<DashboardAreaActivityService>();
        public ConfigService GeneralCategory => ServiceProxy.GetService<ConfigService>();
        #endregion

        #region DASHBOARD
        
        [HttpPost("dashboard-list/gets")]
        public async Task<DataSourceResult> GetDashboardList([DataSourceRequest] DataSourceRequest request, [FromForm] string? periode = null, [FromForm] string? divisionCode = null, [FromForm] string? areaId = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await DashboardAreaActivityService.Gets(Noreg, periode, divisionCode, areaId).ToDataSourceResultAsync(request);
        }

        [HttpPost("getChartDashboard")]
        public async Task<IActionResult> GetChartDashboard([FromBody] JObject request)
        {
            //Console.WriteLine("categoryChart>> " + request["categoryChart"]?.ToString());
            //Console.WriteLine("DivisionCode>> " + request["divisionCode"]?.ToString());
            //Console.WriteLine("AreaId>> " + request["areaId"]?.ToString());
            //Console.WriteLine("Periode>> " + request["periode"]?.ToString());
            var categoryChart = request["categoryChart"]?.ToString();
            var periode = request["periode"]?.ToString();
            var divisionCode = request["divisionCode"]?.ToString();
            var areaId = request["areaId"]?.ToString();
            object result;
            var Noreg = ServiceProxy.UserClaim.NoReg;
            if (categoryChart != "Project_Activity" && categoryChart != "FRSRChart")
            {
                if (periode != "" || divisionCode != "" || areaId != "")
                {
                    Console.WriteLine("divisionCode1>> " + divisionCode);
                    result = DashboardAreaActivityService.GetsBarChartByFilter(categoryChart, Noreg, periode, divisionCode, areaId);
                }
                else {
                    Console.WriteLine("divisionCode2>> " + divisionCode);
                    result = DashboardAreaActivityService.GetsBarChart(categoryChart, Noreg, periode, divisionCode, areaId);
                }
            }
            else if (categoryChart == "FRSRChart") {
                if (periode != "" || divisionCode != "" || areaId != "")
                {
                    result = DashboardAreaActivityService.GetsMultipleChartByFilter(categoryChart, Noreg, periode, divisionCode, areaId);
                }
                else
                {
                    result = DashboardAreaActivityService.GetsMultipleChart(categoryChart, Noreg, periode, divisionCode, areaId);
                    Console.WriteLine("result121>> " + result);
                }
            }
            else
            {
                if (periode != "" || divisionCode != "" || areaId != "")
                {
                    result = DashboardAreaActivityService.GetsChartByFilter(categoryChart, Noreg, periode, divisionCode, areaId);
                }
                else
                {
                    result = DashboardAreaActivityService.GetsChart(categoryChart, Noreg, periode, divisionCode, areaId);
                }

            }

            return Ok(result);
        }

        #endregion

        #region TOTAL WORKER
        [HttpPost("totalEmployee/gets")]
        public async Task<DataSourceResult> GetFromTotalEmployee([DataSourceRequest] DataSourceRequest request, [FromForm] string? periode = null, [FromForm] string? divisionCode = null, [FromForm] string? areaId = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await TotalEmployeeService.Gets(divisionCode, areaId, periode, Noreg).ToDataSourceResultAsync(request);
        }

        [HttpPost("insertTotalEmployee")]
        public IActionResult CreateTotalEmployee([FromBody] JObject request)
        {
            try
            {
                // Ambil data dari request body dengan validasi tambahan
                if (!request.ContainsKey("TotalEmployee") || !int.TryParse(request["TotalEmployee"]?.ToString(), out int totalEmployee) || totalEmployee < 0)
                {
                    return BadRequest(new { Exception = true, Message = "Total Employee harus berupa angka positif." });
                }

                if (!request.ContainsKey("TotalEmployeeOutsourcing") || !int.TryParse(request["TotalEmployeeOutsourcing"]?.ToString(), out int totalEmployeeOutsourcing) || totalEmployeeOutsourcing < 0)
                {
                    return BadRequest(new { Exception = true, Message = "Total Employee Outsourcing harus berupa angka positif." });
                }

                string divisionCode = request["DivisionCode"]?.ToString();
                if (string.IsNullOrWhiteSpace(divisionCode))
                {
                    return BadRequest(new { Exception = true, Message = "Division Code tidak boleh kosong." });
                }

                string divisionCodeInput = request["DivisionCode_input"]?.ToString();
                string areaIdInput = request["AreaId_input"]?.ToString();

                // Validasi AreaId harus berupa GUID yang valid
                if (!request.ContainsKey("AreaId") || !Guid.TryParse(request["AreaId"]?.ToString(), out Guid areaId))
                {
                    return BadRequest(new { Exception = true, Message = "Area ID tidak valid atau kosong." });
                }

                // Ambil data dari request body dengan validasi tambahan
                if (!request.ContainsKey("TotalWorkDay") || !int.TryParse(request["TotalWorkDay"]?.ToString(), out int totalWorkDay) || totalWorkDay < 0)
                {
                    return BadRequest(new { Exception = true, Message = "Total Work Day harus berupa angka positif." });
                }

                int totalOvertime = 0; // Default 0
                if (request.ContainsKey("TotalOvertime") && int.TryParse(request["TotalOvertime"]?.ToString(), out int overtimeValue))
                {
                    if (overtimeValue < 0)
                    {
                        return BadRequest(new { Exception = true, Message = "Total Overtime harus berupa angka positif." });
                    }
                    totalOvertime = overtimeValue;
                }

                Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                    ? parsedId
                    : (Guid?)null;

                // Simpan data ke model
                var model = new TotalEmployeeModel
                {
                    Id = id ?? Guid.NewGuid(), // Jika ID null, buat ID baru
                    TotalEmployee = totalEmployee,
                    TotalEmployeeOutsourcing = totalEmployeeOutsourcing,
                    TotalWorkDay = totalWorkDay,
                    TotalOvertime = totalOvertime,
                    DivisionCode = divisionCode,
                    DivisionName = divisionCodeInput, // Nama divisi dari input
                    AreaId = areaId,
                    AreaName = areaIdInput, // Nama area dari input
                    CreatedBy = "Admin", // Sesuaikan dengan user login
                    CreatedOn = DateTime.UtcNow,
                    RowStatus = true
                };

                // Simpan data ke database
                TotalEmployeeService.Upsert(model);

                return CreatedAtAction("Get", new { id = model.Id }, new { message = "Data berhasil disimpan!", data = model });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan internal.", error = ex.Message });
            }
        }

        [HttpPut("updateTotalEmployee")]
        public IActionResult UpdateTotalEmployee([FromBody] JObject request)
        {
            try
            {
                // Validasi ID harus ada
                if (!request.ContainsKey("Id") || !Guid.TryParse(request["Id"]?.ToString(), out Guid id))
                {
                    return BadRequest(new { Exception = true, Message = "ID tidak valid atau kosong." });
                }

                // Cek apakah data dengan ID ini ada di database
                var existingData = TotalEmployeeService.GetById(id);
                if (existingData == null)
                {
                    return NotFound(new { Exception = true, Message = "Data tidak ditemukan." });
                }

                // Validasi Total Employee
                if (!request.ContainsKey("TotalEmployee") || !int.TryParse(request["TotalEmployee"]?.ToString(), out int totalEmployee) || totalEmployee < 0)
                {
                    return BadRequest(new { Exception = true, Message = "Total Employee harus berupa angka positif." });
                }

                if (!request.ContainsKey("TotalEmployeeOutsourcing") || !int.TryParse(request["TotalEmployeeOutsourcing"]?.ToString(), out int totalEmployeeOutsourcing) || totalEmployeeOutsourcing < 0)
                {
                    return BadRequest(new { Exception = true, Message = "Total Employee Outsourcing harus berupa angka positif." });
                }

                // Validasi Division Code
                string divisionCode = request["DivisionCode"]?.ToString();
                if (string.IsNullOrWhiteSpace(divisionCode))
                {
                    return BadRequest(new { Exception = true, Message = "Division Code tidak boleh kosong." });
                }

                string divisionCodeInput = request["DivisionCode_input"]?.ToString();
                string areaIdInput = request["AreaId_input"]?.ToString();

                // Validasi AreaId harus berupa GUID yang valid
                if (!request.ContainsKey("AreaId") || !Guid.TryParse(request["AreaId"]?.ToString(), out Guid areaId))
                {
                    return BadRequest(new { Exception = true, Message = "Area ID tidak valid atau kosong." });
                }

                // Ambil data dari request body dengan validasi tambahan
                if (!request.ContainsKey("TotalWorkDay") || !int.TryParse(request["TotalWorkDay"]?.ToString(), out int totalWorkDay) || totalWorkDay < 0)
                {
                    return BadRequest(new { Exception = true, Message = "Total Work Day harus berupa angka positif." });
                }

                int totalOvertime = 0; // Default 0
                if (request.ContainsKey("TotalOvertime") && int.TryParse(request["TotalOvertime"]?.ToString(), out int overtimeValue))
                {
                    if (overtimeValue < 0)
                    {
                        return BadRequest(new { Exception = true, Message = "Total Overtime harus berupa angka positif." });
                    }
                    totalOvertime = overtimeValue;
                }

                var model = new TotalEmployeeModel
                {
                    Id = id,
                    TotalEmployee = totalEmployee,
                    TotalEmployeeOutsourcing = totalEmployeeOutsourcing,
                    TotalWorkDay = totalWorkDay,
                    TotalOvertime = totalOvertime,
                    DivisionCode = divisionCode,
                    DivisionName = divisionCodeInput, // Nama divisi dari input
                    AreaId = areaId, // Default ke Guid.Empty jika null
                    AreaName = areaIdInput, // Nama area dari input
                    ModifiedBy = "Admin", // Sesuaikan dengan user login
                    ModifiedOn = DateTime.UtcNow,
                    RowStatus = true
                };

                TotalEmployeeService.Upsert(model);

                return Ok(new { Exception = true, Message = "Data berhasil diperbarui!", data = existingData });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan internal.", error = ex.Message });
            }
        }

        [HttpDelete("deleteTotalEmployee")]
        public IActionResult DeleteTotalEmployee([FromForm] Guid id)
        {
            TotalEmployeeService.Delete(id);

            return NoContent();
        }
        #endregion

        #region SAFETY INCIDENT
        [HttpPost("safety-incident/gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request, [FromForm] string? periode = null, [FromForm] string? divisionCode = null, [FromForm] string? areaId = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await SafetyIncidentService.Gets(divisionCode, areaId, periode, Noreg).ToDataSourceResultAsync(request);
        }

        [HttpGet("safety-incident/{id}")]
        public IActionResult GetById(Guid id)
        {
            var result = SafetyIncidentService.GetById(id); 
            if (result == null) { 
                return NotFound(new ApiResponse<SafetyIncidentViewModel> { 
                    Status = "Error", 
                    Message = "Data not found", 
                    Data = null 
                }); 
            
            }
            return Ok(new ApiResponse<SafetyIncidentViewModel> { 
                Status = "Success", 
                Message = "Data retrieved successfully", 
                Data = result 
            });
        }

        [HttpPost("insertSafetyIncident")]
        public IActionResult CreateSafetyIncident([FromBody] JObject request)
        {
            try
            {
                // Validasi IncidentDescription
                if (!request.ContainsKey("IncidentDescription") || string.IsNullOrWhiteSpace(request["IncidentDescription"]?.ToString()))
                {
                    return BadRequest(new { Exception = true, Message = "IncidentDescription is required." });
                }

                // Validasi IncidentTypeCode
                if (!request.ContainsKey("IncidentTypeCode") || string.IsNullOrWhiteSpace(request["IncidentTypeCode"]?.ToString()))
                {
                    return BadRequest(new { Exception = true, Message = "IncidentTypeCode is required." });
                }

                // Validasi DivisionCode (harus ada dan tidak kosong)
                if (!request.ContainsKey("DivisionCode") || string.IsNullOrWhiteSpace(request["DivisionCode"]?.ToString()))
                {
                    return BadRequest(new { Exception = true, Message = "DivisionCode is required." });
                }

                // Validasi AreaId harus berupa GUID yang valid
                if (!request.ContainsKey("AreaId") || !Guid.TryParse(request["AreaId"]?.ToString(), out Guid areaId))
                {
                    return BadRequest(new { Exception = true, Message = "AreaId is required and must be a valid GUID." });
                }

                // Validasi IncidentDate
                if (!request.ContainsKey("IncidentDate") || !DateTime.TryParse(request["IncidentDate"]?.ToString(), out DateTime incidentDate))
                {
                    return BadRequest(new { Exception = true, Message = "IncidentDate is required and must be a valid date." });
                }

                // Validasi IncidentTime (jika ada)
                TimeSpan? incidentTime = null;
                if (request.ContainsKey("IncidentTime") && !string.IsNullOrWhiteSpace(request["IncidentTime"]?.ToString()))
                {
                    if (!TimeSpan.TryParse(request["IncidentTime"].ToString(), out TimeSpan parsedIncidentTime))
                    {
                        return BadRequest(new { Exception = true, Message = "IncidentTime must be a valid time format (HH:mm:ss)." });
                    }
                    incidentTime = parsedIncidentTime;
                }

                // Validasi IncidentTypeCode
                if (request["IncidentTypeCode"]?.ToString() == "near_miss")
                {
                    // Validasi IncidentTypeCode
                    if (!request.ContainsKey("Subject") || string.IsNullOrWhiteSpace(request["Subject"]?.ToString()))
                    {
                        return BadRequest(new { Exception = true, Message = "Subject is required." });
                    }
                }

                // Validasi TotalVictim (jika ada)
                int? totalVictim = null;
                if (request.ContainsKey("TotalVictim") && !string.IsNullOrWhiteSpace(request["TotalVictim"]?.ToString()))
                {
                    if (!int.TryParse(request["TotalVictim"].ToString(), out int parsedVictim) || parsedVictim < 0)
                    {
                        return BadRequest(new { Exception = true, Message = "TotalVictim must be a positive integer." });
                    }
                    totalVictim = parsedVictim;
                }

                // Validasi TotalLoss (jika ada)
                string totalLoss = request["TotalLoss"]?.ToString();
                if (!string.IsNullOrWhiteSpace(totalLoss) && !decimal.TryParse(totalLoss, out decimal parsedTotalLoss))
                {
                    return BadRequest(new { Exception = true, Message = "TotalLoss must be a valid numeric value." });
                }

                // Mapping data ke model
                var model = new SafetyIncidentModel
                {
                    Id = Guid.NewGuid(),
                    IncidentDescription = request["IncidentDescription"].ToString(),
                    IncidentTypeCode = request["IncidentTypeCode"].ToString(),
                    IncidentDate = incidentDate,
                    Remark = request["Remark"]?.ToString(),
                    DivisionCode = request["DivisionCode"]?.ToString(),
                    DivisionName = request["DivisionCode_input"]?.ToString(),
                    AreaId = areaId,
                    AreaName = request["AreaId_input"]?.ToString(),
                    TotalLoss = totalLoss,
                    PropertyType = request["PropertyType"]?.ToString(),
                    CreatedBy = "Admin", // Sesuaikan dengan user login
                    CreatedOn = DateTime.UtcNow,
                    RowStatus = true
                };

                // Jika IncidentTime diberikan, gabungkan dengan IncidentDate
                if (incidentTime != null)
                {
                    model.IncidentDate = incidentDate.Date.Add(incidentTime.Value);
                }

                // Jika ada nilai null yang boleh diabaikan, set ke nilai default atau kosong
                model.Subject = request["Subject"]?.ToString() ?? string.Empty;
                model.AccidentType = request["AccidentType"]?.ToString() ?? string.Empty;
                model.TotalVictim = totalVictim;
                model.LossTime = request["LossTime"]?.ToString() ?? string.Empty;
                model.Attachment = request["Attachment"]?.ToString() ?? string.Empty;

                Console.WriteLine($"Upsert called with model: {JsonConvert.SerializeObject(model)}");

                var Noreg = ServiceProxy.UserClaim.NoReg;
                var UserName = ServiceProxy.UserClaim.Username;
                // Panggil service untuk menyimpan data
                SafetyIncidentService.InsertSafetyIncident(model, Noreg, UserName);

                return Ok(new { Success = true, Message = "Incident created successfully.", Data = model });
            }
            catch (Exception ex)
            {
                // Tangani error
                return StatusCode(500, new { Exception = true, Message = "Internal server error.", Error = ex.Message });
            }
        }


        [HttpPut("updateSafetyIncident")]
        public IActionResult UpdateSafetyIncident([FromBody] JObject request)
        {
            try
            {
                // Validasi ID (harus ada dan valid GUID)
                if (!request.ContainsKey("Id") || !Guid.TryParse(request["Id"]?.ToString(), out Guid id))
                {
                    return BadRequest(new { Exception = true, Message = "Id is required and must be a valid GUID." });
                }

                // Validasi IncidentDescription
                if (!request.ContainsKey("IncidentDescription") || string.IsNullOrWhiteSpace(request["IncidentDescription"]?.ToString()))
                {
                    return BadRequest(new { Exception = true, Message = "IncidentDescription is required." });
                }

                // Validasi IncidentTypeCode
                if (!request.ContainsKey("IncidentTypeCode") || string.IsNullOrWhiteSpace(request["IncidentTypeCode"]?.ToString()))
                {
                    return BadRequest(new { Exception = true, Message = "IncidentTypeCode is required." });
                }


                // Validasi DivisionCode (harus ada dan tidak kosong)
                if (!request.ContainsKey("DivisionCode") || string.IsNullOrWhiteSpace(request["DivisionCode"]?.ToString()))
                {
                    return BadRequest(new { Exception = true, Message = "DivisionCode is required." });
                }

                // Validasi AreaId harus berupa GUID yang valid
                if (!request.ContainsKey("AreaId") || !Guid.TryParse(request["AreaId"]?.ToString(), out Guid areaId))
                {
                    return BadRequest(new { Exception = true, Message = "AreaId is required and must be a valid GUID." });
                }

                // Validasi IncidentDate
                if (!request.ContainsKey("IncidentDate") || !DateTime.TryParse(request["IncidentDate"]?.ToString(), out DateTime incidentDate))
                {
                    return BadRequest(new { Exception = true, Message = "IncidentDate is required and must be a valid date." });
                }

                // Validasi IncidentTime (jika ada)
                TimeSpan? incidentTime = null;
                if (request.ContainsKey("IncidentTime") && !string.IsNullOrWhiteSpace(request["IncidentTime"]?.ToString()))
                {
                    if (!TimeSpan.TryParse(request["IncidentTime"].ToString(), out TimeSpan parsedIncidentTime))
                    {
                        return BadRequest(new { Exception = true, Message = "IncidentTime must be a valid time format (HH:mm:ss)." });
                    }
                    incidentTime = parsedIncidentTime;
                }

                // Validasi TotalVictim (jika ada)
                int? totalVictim = null;
                if (request.ContainsKey("TotalVictim") && !string.IsNullOrWhiteSpace(request["TotalVictim"]?.ToString()))
                {
                    if (!int.TryParse(request["TotalVictim"].ToString(), out int parsedVictim) || parsedVictim < 0)
                    {
                        return BadRequest(new { Exception = true, Message = "TotalVictim must be a positive integer." });
                    }
                    totalVictim = parsedVictim;
                }

                // Validasi IncidentTypeCode
                if (request["IncidentTypeCode"]?.ToString() == "near_miss")
                {
                    // Validasi IncidentTypeCode
                    if (!request.ContainsKey("Subject") || string.IsNullOrWhiteSpace(request["Subject"]?.ToString()))
                    {
                        return BadRequest(new { Exception = true, Message = "Subject is required." });
                    }
                }

                // Validasi TotalLoss (jika ada)
                string totalLoss = request["TotalLoss"]?.ToString();
                if (!string.IsNullOrWhiteSpace(totalLoss) && !decimal.TryParse(totalLoss, out decimal parsedTotalLoss))
                {
                    return BadRequest(new { Exception = true, Message = "TotalLoss must be a valid numeric value." });
                }

                // Mapping data ke model
                var model = new SafetyIncidentModel
                {
                    Id = id,
                    IncidentDescription = request["IncidentDescription"].ToString(),
                    IncidentTypeCode = request["IncidentTypeCode"].ToString(),
                    IncidentDate = incidentDate,
                    Remark = request["Remark"]?.ToString(),
                    DivisionCode = request["DivisionCode"]?.ToString(),
                    DivisionName = request["DivisionCode_input"]?.ToString(),
                    AreaId = areaId,
                    AreaName = request["AreaId_input"]?.ToString(),
                    TotalLoss = totalLoss,
                    PropertyType = request["PropertyType"]?.ToString(),
                    ModifiedBy = "Admin", // Sesuaikan dengan user login
                    ModifiedOn = DateTime.UtcNow,
                    RowStatus = true
                };

                // Jika IncidentTime diberikan, gabungkan dengan IncidentDate
                if (incidentTime != null)
                {
                    model.IncidentDate = incidentDate.Date.Add(incidentTime.Value);
                }

                // Jika ada nilai null yang boleh diabaikan, set ke nilai default atau kosong
                model.Subject = request["Subject"]?.ToString() ?? null;
                model.AccidentType = request["AccidentType"]?.ToString() ?? null;
                model.TotalVictim = totalVictim;
                model.LossTime = request["LossTime"]?.ToString() ?? null;
                model.Attachment = request["Attachment"]?.ToString() ?? null;

                // Panggil service untuk menyimpan data
                SafetyIncidentService.UpdateSafetyIncident(model);

                return Ok(new { Success = true, Message = "Incident updated successfully.", Data = model });
            }
            catch (Exception ex)
            {
                // Tangani error
                return StatusCode(500, new { Exception = true, Message = "Internal server error.", Error = ex.Message });
            }
        }


        [HttpDelete("deleteSafetyIncident")]
        public IActionResult DeleteSafetyIncident([FromForm] Guid id)
        {
            SafetyIncidentService.Delete(id);

            return NoContent();
        }
        #endregion

        #region SAFETY FACILITY
        [HttpPost("safetyFacility/gets")]
        public async Task<DataSourceResult> GetFromSafetyFacility([DataSourceRequest] DataSourceRequest request, [FromForm] string? periode = null, [FromForm] string? divisionCode = null, [FromForm] string? areaId = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await SafetyFacilityService.Gets(divisionCode, areaId, periode, Noreg).ToDataSourceResultAsync(request);
        }
        [HttpPost("insertSafetyFacility")]
        public IActionResult CreateSafetyFacility([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string equipmentIdInput = request["EquipmentId_input"]?.ToString();
            Guid? equipmentId = request["EquipmentId"] != null && Guid.TryParse(request["EquipmentId"].ToString(), out Guid parsedEquipmentId)
                ? parsedEquipmentId
                : (Guid?)null;

            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();

            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;

            string totalActual = request["TotalActual"]?.ToString();
            string remark = request["Remark"]?.ToString();

            // Validasi data yang diperlukan
            if (string.IsNullOrEmpty(totalActual) || !int.TryParse(totalActual, out int parsedTotalActual))
            {
                return BadRequest(new { Exception = true, Message = "TotalActual harus berupa angka yang valid." });
            }

            if (equipmentId == null || string.IsNullOrEmpty(equipmentIdInput))
            {
                return BadRequest(new { Exception = true, Message = "EquipmentId dan EquipmentId_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }

            // Membuat model SafetyFacility
            var model = new SafetyFacilityModel
            {
                Id = id ?? Guid.NewGuid(), // Gunakan Guid baru jika Id tidak diberikan
                EquipmentId = equipmentId.Value,
                EquipmentName = equipmentIdInput,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                AreaId = areaId.Value,
                AreaName = areaIdInput,
                TotalActual = parsedTotalActual,
                Remark = remark,
                CreatedBy = "Admin", // Sesuaikan dengan user login
                CreatedOn = DateTime.UtcNow,
                RowStatus = true
            };

            // Simpan data menggunakan service
            SafetyFacilityService.Upsert(model);

            return CreatedAtAction("Get", new { id = model.Id }, model);
        }


        [HttpPut("updateSafetyFacility")]
        public IActionResult UpdateSafetyFacility([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string equipmentIdInput = request["EquipmentId_input"]?.ToString();
            Guid? equipmentId = request["EquipmentId"] != null && Guid.TryParse(request["EquipmentId"].ToString(), out Guid parsedEquipmentId)
                ? parsedEquipmentId
                : (Guid?)null;

            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();

            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;

            string totalActual = request["TotalActual"]?.ToString();
            string remark = request["Remark"]?.ToString();

             // Validasi data yang diperlukan
            if (string.IsNullOrEmpty(totalActual) || !int.TryParse(totalActual, out int parsedTotalActual))
            {
                return BadRequest(new { Exception = true, Message = "TotalActual harus berupa angka yang valid." });
            }

            if (equipmentId == null || string.IsNullOrEmpty(equipmentIdInput))
            {
                return BadRequest(new { Exception = true, Message = "EquipmentId dan EquipmentId_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }

            // Membuat model SafetyFacility
            var model = new SafetyFacilityModel
            {
                Id = id ?? Guid.NewGuid(), // Gunakan Guid baru jika Id tidak diberikan
                EquipmentId = equipmentId.Value,
                EquipmentName = equipmentIdInput,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                AreaId = areaId.Value,
                AreaName = areaIdInput,
                TotalActual = parsedTotalActual,
                Remark = remark,
                ModifiedBy = "Admin", // Sesuaikan dengan user login
                ModifiedOn = DateTime.UtcNow,
                RowStatus = true
            };

            // Simpan data menggunakan service
            SafetyFacilityService.Upsert(model);

            return CreatedAtAction("Get", new { id = model.Id }, model);

            return NoContent();
        }
        [HttpDelete("deleteSafetyFacility")]
        public IActionResult DeleteSafetyFacility([FromForm] Guid id)
        {
            SafetyFacilityService.Delete(id);

            return NoContent();
        }
        #endregion

        #region FIRE PROTECTION
        [HttpPost("fireProtection/gets")]
        public async Task<DataSourceResult> GetFromFireProtection([DataSourceRequest] DataSourceRequest request, [FromForm] string? periode = null, [FromForm] string? divisionCode = null, [FromForm] string? areaId = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await FireProtectionService.Gets(divisionCode, areaId, periode, Noreg).ToDataSourceResultAsync(request);
        }

        [HttpPost("insertFireProtection")]
        public IActionResult CreateFireProtection([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string categoryInput = request["Category_input"]?.ToString();
            string category = request["Category"]?.ToString();

            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();

            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;

            //string installedStr = request["Installed"]?.ToString();
            string readyStr = request["Ready"]?.ToString();

            // Validasi data yang diperlukan
            //if (string.IsNullOrEmpty(installedStr) || !int.TryParse(installedStr, out int installed))
            //{
            //    return BadRequest(new { Exception = true, Message = "Installed harus berupa angka yang valid." });
            //}

            if (string.IsNullOrEmpty(readyStr) || !int.TryParse(readyStr, out int ready))
            {
                return BadRequest(new { Exception = true, Message = "Ready harus berupa angka yang valid." });
            }

            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(categoryInput))
            {
                return BadRequest(new { Exception = true, Message = "Category dan Category_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }


            // Membuat model FireProtectionModel
            var model = new FireProtectionModel
            {
                Id = id ?? Guid.NewGuid(), // Gunakan Guid baru jika Id tidak diberikan
                //Installed = installed,
                Ready = ready,
                Category = category,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                AreaId = areaId,
                AreaName = areaIdInput,
                CreatedBy = "Admin", // Sesuaikan dengan user login
                CreatedOn = DateTime.UtcNow,
                RowStatus = true
            };

            // Simpan data menggunakan service
            FireProtectionService.Upsert(model);

            return CreatedAtAction("Get", new { id = model.Id }, model);
        }


        [HttpPut("updateFireProtection")]
        public IActionResult UpdateFireProtection([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string categoryInput = request["Category_input"]?.ToString();
            string category = request["Category"]?.ToString();

            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();

            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;

            //string installedStr = request["Installed"]?.ToString();
            string readyStr = request["Ready"]?.ToString();

            // Validasi data yang diperlukan
            if (id == null)
            {
                return BadRequest(new { Exception = true, Message = "Id tidak boleh kosong." });
            }

            // Validasi data yang diperlukan
            //if (string.IsNullOrEmpty(installedStr) || !int.TryParse(installedStr, out int installed))
            //{
            //    return BadRequest(new { Exception = true, Message = "Installed harus berupa angka yang valid." });
            //}

            if (string.IsNullOrEmpty(readyStr) || !int.TryParse(readyStr, out int ready))
            {
                return BadRequest(new { Exception = true, Message = "Ready harus berupa angka yang valid." });
            }

            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(categoryInput))
            {
                return BadRequest(new { Exception = true, Message = "Category dan Category_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }


            // Membuat model FireProtectionModel
            var model = new FireProtectionModel
            {
                Id = id.Value, // Gunakan Id yang diberikan
                //Installed = installed,
                Ready = ready,
                Category = category,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                AreaId = areaId,
                AreaName = areaIdInput,
                ModifiedBy = "Admin", // Sesuaikan dengan user login
                ModifiedOn = DateTime.UtcNow,
                RowStatus = true
            };

            // Perbarui data menggunakan service
            var existingModel = FireProtectionService.GetById(id.Value);
            if (existingModel == null)
            {
                return NotFound("Data tidak ditemukan.");
            }

            FireProtectionService.Upsert(model);

            return NoContent();
        }


        [HttpDelete("deleteFireProtection")]
        public IActionResult DeleteFireProtection([FromForm] Guid id)
        {
            FireProtectionService.Delete(id);

            return NoContent();
        }

        #endregion

        #region APAR REFILL
        [HttpPost("apar-refill/gets")]
        public async Task<DataSourceResult> GetFromAparRefill([DataSourceRequest] DataSourceRequest request, [FromForm] string? periode = null, [FromForm] string? divisionCode = null, [FromForm] string? areaId = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await APARRefillService.Gets(divisionCode, areaId, periode, Noreg).ToDataSourceResultAsync(request);
        }

        [HttpPost("insertAparRefill")]
        public IActionResult CreateAparRefill([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string type = request["Type"]?.ToString();
            string merk = request["Merk"]?.ToString();

            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();

            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;

            string qtyStr = request["Qty"]?.ToString();
            string remark = request["Remark"]?.ToString();
            string useAparFor = request["UseAparFor"]?.ToString();
            // Validasi data yang diperlukan
            if (string.IsNullOrEmpty(type))
            {
                return BadRequest(new { Exception = true, Message = "Type tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(merk))
            {
                return BadRequest(new { Exception = true, Message = "Merk tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(qtyStr) || !int.TryParse(qtyStr, out int qty))
            {
                return BadRequest(new { Exception = true, Message = "Qty harus berupa angka yang valid." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(useAparFor))
            {
                return BadRequest(new { Exception = true, Message = "Keterangan Penggunaan APAR tidak boleh kosong." });
            }

            // Validasi tambahan: Jika useAparFor adalah "Lain", maka remark tidak boleh kosong
            if (useAparFor == "Lain" && string.IsNullOrEmpty(remark))
            {
                return BadRequest(new { Exception = true, Message = "Remark tidak boleh kosong." });
            }

            // Membuat model AparRefillModel
            var model = new APARRefillModel
            {
                Id = id ?? Guid.NewGuid(), // Gunakan Guid baru jika Id tidak diberikan
                Type = type,
                Merk = merk,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                AreaId = areaId,
                AreaName = areaIdInput,
                Qty = qty,
                Remark = remark,
                UseAparFor = useAparFor,
                CreatedBy = "Admin", // Sesuaikan dengan user login
                CreatedOn = DateTime.UtcNow,
                RowStatus = true
            };

            // Simpan data menggunakan service
            APARRefillService.Upsert(model);

            return CreatedAtAction("Get", new { id = model.Id }, model);
        }



        [HttpPut("updateAparRefill")]
        public IActionResult UpdateAparRefill([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string type = request["Type"]?.ToString();
            string merk = request["Merk"]?.ToString();

            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();

            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;

            string qtyStr = request["Qty"]?.ToString();
            string remark = request["Remark"]?.ToString();
            string useAparFor = request["UseAparFor"]?.ToString();
            // Validasi data yang diperlukan
            if (id == null)
            {
                return BadRequest(new { Exception = true, Message = "Id tidak boleh kosong." });
            }
            
            if (string.IsNullOrEmpty(type))
            {
                return BadRequest(new { Exception = true, Message = "Type tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(merk))
            {
                return BadRequest(new { Exception = true, Message = "Merk tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(qtyStr) || !int.TryParse(qtyStr, out int qty))
            {
                return BadRequest(new { Exception = true, Message = "Qty harus berupa angka yang valid." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(useAparFor))
            {
                return BadRequest(new { Exception = true, Message = "Keterangan Penggunaan APAR tidak boleh kosong." });
            }

            // Validasi tambahan: Jika useAparFor adalah "Lain", maka remark tidak boleh kosong
            if (useAparFor == "Lain" && string.IsNullOrEmpty(remark))
            {
                return BadRequest(new { Exception = true, Message = "Remark tidak boleh kosong" });
            }

            // Perbarui model AparRefillModel
            var existingModel = APARRefillService.GetById(id.Value);
            if (existingModel == null)
            {
                return NotFound("Data tidak ditemukan.");
            }

            var model = new APARRefillModel
            {
                Id = id.Value,
                Type = type,
                Merk = merk,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                AreaId = areaId,
                AreaName = areaIdInput,
                Qty = qty,
                Remark = remark,
                ModifiedBy = "Admin", // Sesuaikan dengan user login
                ModifiedOn = DateTime.UtcNow,
                RowStatus = true,
                UseAparFor = useAparFor
            };

            APARRefillService.Upsert(model);

            return NoContent();
        }



        [HttpDelete("deleteAparRefill")]
        public IActionResult DeleteAparRefill([FromForm] Guid id)
        {
            APARRefillService.Delete(id);

            return NoContent();
        }
        #endregion

        #region TRAINING RECORD
        [HttpPost("trainingRecord/gets")]
        public async Task<DataSourceResult> GetFromTrainingRecord([DataSourceRequest] DataSourceRequest request, [FromForm] string? periode = null, [FromForm] string? divisionCode = null, [FromForm] string? areaId = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await TrainingRecordService.Gets(divisionCode, areaId, periode, Noreg).ToDataSourceResultAsync(request);
        }

        [HttpPost("insertTrainingRecord")]
        public IActionResult CreateTrainingRecord([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string description = request["Description"]?.ToString();
            string institution = request["Institution"]?.ToString();
            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();
            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;
            string participantInput = request["Participant_input"]?.ToString();
            string participant = request["Participant"]?.ToString();
            string remark = request["Remark"]?.ToString();
            string dateStartStr = request["DateStart"]?.ToString();
            string dateEndStr = request["DateEnd"]?.ToString();

            string totalPersonStr = request["TotalPerson"]?.ToString();

            if (string.IsNullOrEmpty(totalPersonStr) || !int.TryParse(totalPersonStr, out int totalPerson))
            {
                return BadRequest(new { Exception = true, Message = "TotalPerson harus berupa angka yang valid." });
            }

            // Validasi data yang diperlukan
            if (string.IsNullOrEmpty(description))
            {
                return BadRequest(new { Exception = true, Message = "Description tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(dateStartStr) || !DateTime.TryParse(dateStartStr, out DateTime dateStart))
            {
                return BadRequest(new { Exception = true, Message = "Training Start Date harus berupa tanggal yang valid." });
            }

            if (string.IsNullOrEmpty(dateEndStr) || !DateTime.TryParse(dateEndStr, out DateTime dateEnd))
            {
                return BadRequest(new { Exception = true, Message = "Training End Date harus berupa tanggal yang valid." });
            }

            if (string.IsNullOrEmpty(institution))
            {
                return BadRequest(new { Exception = true, Message = "Institution tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(participant) || string.IsNullOrEmpty(participantInput))
            {
                return BadRequest(new { Exception = true, Message = "Participant dan Participant_input tidak boleh kosong." });
            }


            // Membuat model TrainingRecordModel
            var model = new TrainingRecordModel
            {
                Id = id ?? Guid.NewGuid(), // Gunakan Guid baru jika Id tidak diberikan
                Description = description,
                Institution = institution,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                Participant = participant,
                AreaId = areaId,
                DateStart = dateStart,
                DateEnd = dateEnd,
                AreaName = areaIdInput,
                TotalPerson = totalPerson,
                Remark = remark,
                CreatedBy = "Admin", // Sesuaikan dengan user login
                CreatedOn = DateTime.UtcNow,
                RowStatus = true
            };

            // Simpan data menggunakan service
            TrainingRecordService.Upsert(model);

            return CreatedAtAction("GetTrainingRecord", new { id = model.Id }, model);
        }

        [HttpPut("updateTrainingRecord")]
        public IActionResult UpdateTrainingRecord([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string description = request["Description"]?.ToString();
            string institution = request["Institution"]?.ToString();
            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();
            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;
            string participantInput = request["Participant_input"]?.ToString();
            string participant = request["Participant"]?.ToString();
            string remark = request["Remark"]?.ToString();
            string dateStartStr = request["DateStart"]?.ToString();
            string dateEndStr = request["DateEnd"]?.ToString();
            string totalPersonStr = request["TotalPerson"]?.ToString();

            // Validasi data yang diperlukan
            if (id == null)
            {
                return BadRequest(new { Exception = true, Message = "Id tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(totalPersonStr) || !int.TryParse(totalPersonStr, out int totalPerson))
            {
                return BadRequest(new { Exception = true, Message = "TotalPerson harus berupa angka yang valid." });
            }

            // Validasi data yang diperlukan
            if (string.IsNullOrEmpty(description))
            {
                return BadRequest(new { Exception = true, Message = "Description tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(dateStartStr) || !DateTime.TryParse(dateStartStr, out DateTime dateStart))
            {
                return BadRequest(new { Exception = true, Message = "Training Start Date harus berupa tanggal yang valid." });
            }

            if (string.IsNullOrEmpty(dateEndStr) || !DateTime.TryParse(dateEndStr, out DateTime dateEnd))
            {
                return BadRequest(new { Exception = true, Message = "Training End Date harus berupa tanggal yang valid." });
            }

            if (string.IsNullOrEmpty(institution))
            {
                return BadRequest(new { Exception = true, Message = "Institution tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(participant) || string.IsNullOrEmpty(participantInput))
            {
                return BadRequest(new { Exception = true, Message = "Participant dan Participant_input tidak boleh kosong." });
            }



            // Perbarui model TrainingRecordModel
            var existingModel = TrainingRecordService.GetById(id.Value);
            if (existingModel == null)
            {
                return NotFound("Data tidak ditemukan.");
            }
            // Membuat model TrainingRecordModel
            var model = new TrainingRecordModel
            {
                Id = id.Value,
                Description = description,
                Institution = institution,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                Participant = participant,
                AreaId = areaId,
                DateStart = dateStart,
                DateEnd = dateEnd,
                AreaName = areaIdInput,
                TotalPerson = totalPerson,
                Remark = remark,
                ModifiedBy = "Admin", // Sesuaikan dengan user login
                ModifiedOn = DateTime.UtcNow,
                RowStatus = true
            };

            // Simpan data menggunakan service
            TrainingRecordService.Upsert(model);

            return NoContent();
        }



        [HttpDelete("deleteTrainingRecord")]
        public IActionResult DeleteTrainingRecord([FromForm] Guid id)
        {
            TrainingRecordService.Delete(id);

            return NoContent();
        }
        #endregion

        #region PROJECT ACTIVITY
        [HttpPost("ProjectActivity/gets")]
        public async Task<DataSourceResult> GetFromProjectActivity([DataSourceRequest] DataSourceRequest request ,[FromForm] string? periode = null, [FromForm] string? divisionCode = null, [FromForm] string? areaId = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await ProjectActivityService.Gets(divisionCode, areaId, periode, Noreg).ToDataSourceResultAsync(request);
        }

        [HttpPost("insertProjectActivity")]
        public IActionResult InsertProjectActivity([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string contractor = request["Contractor"]?.ToString();
            string projectName = request["ProjectName"]?.ToString();
            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();
            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;
            string riskCategory = request["RiskCategory"]?.ToString();
            string startDateStr = request["StartDate"]?.ToString();
            string endDateStr = request["EndDate"]?.ToString();
            string totalWorkerStr = request["TotalWorker"]?.ToString();

            // Validasi data yang diperlukan
            if (string.IsNullOrEmpty(contractor))
            {
                return BadRequest(new { Exception = true, Message = "Contractor tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(projectName))
            {
                return BadRequest(new { Exception = true, Message = "ProjectName tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(riskCategory))
            {
                return BadRequest(new { Exception = true, Message = "RiskCategory tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(startDateStr) || !DateTime.TryParse(startDateStr, out DateTime startDate))
            {
                return BadRequest(new { Exception = true, Message = "StartDate harus berupa tanggal yang valid." });
            }

            if (string.IsNullOrEmpty(endDateStr) || !DateTime.TryParse(endDateStr, out DateTime endDate))
            {
                return BadRequest(new { Exception = true, Message = "EndDate harus berupa tanggal yang valid." });
            }

            if (string.IsNullOrEmpty(totalWorkerStr) || !int.TryParse(totalWorkerStr, out int totalWorker))
            {
                return BadRequest(new { Exception = true, Message = "TotalWorker harus berupa angka yang valid." });
            }


            // Membuat model ProjectActivityModel
            var model = new ProjectActivityModel
            {
                Id = id ?? Guid.NewGuid(), // Gunakan Guid baru jika Id tidak diberikan
                Contractor = contractor,
                ProjectName = projectName,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                AreaId = areaId,
                AreaName = areaIdInput,
                RiskCategory = riskCategory,
                StartDate = startDate,
                EndDate = endDate,
                TotalWorker = totalWorker,
                CreatedBy = "Admin", // Sesuaikan dengan user login
                CreatedOn = DateTime.UtcNow,
                RowStatus = true
            };

            // Simpan data menggunakan service
            ProjectActivityService.Upsert(model);

            return CreatedAtAction("GetProjectActivity", new { id = model.Id }, model);
        }


        [HttpPut("updateProjectActivity")]
        public IActionResult UpdateProjectActivity([FromBody] JObject request)
        {
            // Ambil data dari request body dengan validasi tambahan
            Guid? id = request["Id"] != null && Guid.TryParse(request["Id"].ToString(), out Guid parsedId)
                ? parsedId
                : (Guid?)null;

            string contractor = request["Contractor"]?.ToString();
            string projectName = request["ProjectName"]?.ToString();
            string divisionCodeInput = request["DivisionCode_input"]?.ToString();
            string divisionCode = request["DivisionCode"]?.ToString();
            string areaIdInput = request["AreaId_input"]?.ToString();
            Guid? areaId = request["AreaId"] != null && Guid.TryParse(request["AreaId"].ToString(), out Guid parsedAreaId)
                ? parsedAreaId
                : (Guid?)null;
            string riskCategory = request["RiskCategory"]?.ToString();
            string startDateStr = request["StartDate"]?.ToString();
            string endDateStr = request["EndDate"]?.ToString();
            string totalWorkerStr = request["TotalWorker"]?.ToString();

            // Validasi data yang diperlukan
            if (id == null)
            {
                return BadRequest(new { Exception = true, Message = "Id tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(contractor))
            {
                return BadRequest(new { Exception = true, Message = "Contractor tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(projectName))
            {
                return BadRequest(new { Exception = true, Message = "ProjectName tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(divisionCode) || string.IsNullOrEmpty(divisionCodeInput))
            {
                return BadRequest(new { Exception = true, Message = "DivisionCode dan DivisionCode_input tidak boleh kosong." });
            }

            if (areaId == null || string.IsNullOrEmpty(areaIdInput))
            {
                return BadRequest(new { Exception = true, Message = "AreaId dan AreaId_input tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(riskCategory))
            {
                return BadRequest(new { Exception = true, Message = "RiskCategory tidak boleh kosong." });
            }

            if (string.IsNullOrEmpty(startDateStr) || !DateTime.TryParse(startDateStr, out DateTime startDate))
            {
                return BadRequest(new { Exception = true, Message = "StartDate harus berupa tanggal yang valid." });
            }

            if (string.IsNullOrEmpty(endDateStr) || !DateTime.TryParse(endDateStr, out DateTime endDate))
            {
                return BadRequest(new { Exception = true, Message = "EndDate harus berupa tanggal yang valid." });
            }

            if (string.IsNullOrEmpty(totalWorkerStr) || !int.TryParse(totalWorkerStr, out int totalWorker))
            {
                return BadRequest(new { Exception = true, Message = "TotalWorker harus berupa angka yang valid." });
            }


            // Membuat model ProjectActivityModel
            var model = new ProjectActivityModel
            {
                Id = id.Value, // Gunakan Guid baru jika Id tidak diberikan
                Contractor = contractor,
                ProjectName = projectName,
                DivisionCode = divisionCode,
                DivisionName = divisionCodeInput,
                AreaId = areaId,
                AreaName = areaIdInput,
                RiskCategory = riskCategory,
                StartDate = startDate,
                EndDate = endDate,
                TotalWorker = totalWorker,
                ModifiedBy = "Admin", // Sesuaikan dengan user login
                ModifiedOn = DateTime.UtcNow,
                RowStatus = true
            };

            // Simpan data menggunakan service
            ProjectActivityService.Upsert(model);

            return CreatedAtAction("GetProjectActivity", new { id = model.Id }, model);
        }

        [HttpDelete("deleteProjectActivity")]
        public IActionResult deleteProjectActivity([FromForm] Guid id)
        {
            ProjectActivityService.Delete(id);

            return NoContent();
        }
        #endregion

        #region MASTER DATA
        [HttpPost("gets-Divisi")]
        public async Task<DataSourceResult> GetByDivisi()
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await AreaService.GetDivisionsAreaActivity(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("gets-AreaDropDown")]
        public async Task<DataSourceResult> GetByArea([DataSourceRequest] DataSourceRequest request, [FromForm] string? divisionCode = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await AreaService.GetAreaDropDownAreaActivity(divisionCode, Noreg).ToDataSourceResultAsync(request);
        }

        [HttpPost("gets-filterAreaDropDown")]
        public async Task<DataSourceResult> GetByfilterAreaDropDown([DataSourceRequest] DataSourceRequest request, [FromForm] string? divisionCode = null)
        {
            var Noreg = ServiceProxy.UserClaim.NoReg;
            return await AreaService.GetFilterAreaDropDown(divisionCode, Noreg).ToDataSourceResultAsync(request);
        }

        [HttpPost("gets-EquipmentDropDown")]
        public async Task<DataSourceResult> GetByEquipment([DataSourceRequest] DataSourceRequest request,[FromForm] string? divisionCode = null,[FromForm] string? areaId = null)
        {
            var result = await EquipmentService.GetPopUpEquipment(divisionCode, areaId)
                         .ToDataSourceResultAsync(request);
            return result;
        }

        [HttpPost("getGeneralCategory")]
        public async Task<DataSourceResult> GetGeneralCategory(string category)
        {
            //category = "OHSRiskCategory";
            return await GeneralCategory.GetGeneralCategoriesQuerySortedSequence(category).ToDataSourceResultAsync(new DataSourceRequest());
        }
        #endregion
    }
}
#endregion

#region MVC Controller
/// <summary>
/// OHS Area Activity Controller
/// </summary>
[Area("OHS")]
[Permission(PermissionKey.AreaActivityView)]
public class AreaActivityController : MvcControllerBase
{
    #region Domain Services
    public TotalEmployeeService TotalEmployeeService => ServiceProxy.GetService<TotalEmployeeService>();
    public SafetyIncidentService SafetyIncidentService => ServiceProxy.GetService<SafetyIncidentService>();
    public SafetyFacilityService SafetyFacilityService => ServiceProxy.GetService<SafetyFacilityService>();
    public FireProtectionService FireProtectionService => ServiceProxy.GetService<FireProtectionService>();
    public APARRefillService APARRefillService => ServiceProxy.GetService<APARRefillService>();
    public TrainingRecordService TrainingRecordService => ServiceProxy.GetService<TrainingRecordService>();
    public ProjectActivityService ProjectActivityService => ServiceProxy.GetService<ProjectActivityService>();
    #endregion

    private readonly IUnitOfWork _unitOfWork;

    public AreaActivityController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var NoReg = ServiceProxy.UserClaim.NoReg;
        ViewBag.divisionData = GetDivisionTree(NoReg);
        ViewBag.areaData = GetAreaTree(NoReg);
        return View();
    }

    public IActionResult ListAreaActivity(string Periode, string DivisionCode, string DivisionName, string AreaId, string AreaName)
    {
        var NoReg = ServiceProxy.UserClaim.NoReg;
        ViewBag.divisionData = GetDivisionTree(NoReg);
        ViewBag.areaData = GetAreaTree(NoReg);

        return View("ListAreaActivity");
    }

    public List<DropDownTreeItemModel> GetAreaTree(string NoReg = null)
    {
        var areaService = ServiceProxy.GetService<AreaService>();
        var areas = areaService.GetAreas().AsQueryable(); // Pastikan bisa digunakan dalam LINQ

        if (areas == null || !areas.Any())
        {
            return new List<DropDownTreeItemModel>();
        }

        // Jika NoReg diberikan, filter berdasarkan akses pengguna
        if (!string.IsNullOrEmpty(NoReg))
        {
            var roleRepo = _unitOfWork.GetRepository<RoleAreaActivitytModel>(); // Ambil repository role akses

            var allowedAreaIds = roleRepo.Fetch()
                .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                .Select(a => a.AreaId) // Ambil daftar AreaId yang diperbolehkan
                .Distinct()
                .ToList(); // Eksekusi untuk mendapatkan daftar ID area

            if (allowedAreaIds.Any())
            {
                //areas = areas.Where(x => allowedAreaIds.Contains(x.Id)); // Filter area berdasarkan akses
                var allowedAreaQueryable = allowedAreaIds.AsQueryable();

                areas = areas.Join(
                    allowedAreaQueryable,
                    a => a.Id,
                    id => id,
                    (a, id) => a
                );
            }   
        }

        // Map data area ke DropDownTreeItemModel
        var listDropDownTreeItem = areas
            .Select(area => new DropDownTreeItemModel
            {
                Value = area.Id.ToString(),
                Text = area.NamaArea,
                Expanded = false // Default: tidak terbuka
            })
            .OrderBy(item => item.Text) // Urutkan secara alfabetis berdasarkan NamaArea
            .ToList();

        return listDropDownTreeItem;
    }
    public List<DropDownTreeItemModel> GetDivisionTree(string NoReg = null)
    {

        var payrollService = ServiceProxy.GetService<PayrollReportService>();
        var divisions = payrollService.GetDivisions().AsQueryable(); // Pastikan bisa di-query

        // Jika NoReg diberikan, filter berdasarkan akses pengguna
        if (!string.IsNullOrEmpty(NoReg))
        {
            var roleRepo = _unitOfWork.GetRepository<RoleAreaActivitytModel>();

            var allowedDivisionCodes = roleRepo.Fetch()
                .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                .Select(a => a.DivisionCode)
                .Distinct()
                .ToList(); // Eksekusi untuk mendapatkan daftar kode divisi

            if (allowedDivisionCodes.Any())
            {
                //divisions = divisions.Where(d => allowedDivisionCodes.Contains(d.OrgCode)); // Filter divisi berdasarkan akses
                var allowedDivisionQueryable = allowedDivisionCodes.AsQueryable();

                divisions = divisions.Join(
                    allowedDivisionQueryable,
                    d => d.OrgCode,
                    code => code,
                    (d, code) => d
                );
            }
        }

        // Map data divisi ke DropDownTreeItemModel
        var listDropDownTreeItem = divisions
            .Select(division => new DropDownTreeItemModel
            {
                Value = $"{division.OrgCode}#{division.ObjectText}", // Format: OrgCode#Nama
                Text = division.ObjectText,
                Expanded = false // Default: tidak terbuka
            })
            .OrderBy(x => x.Text) // Urutkan secara alfabetis
            .ToList();

        return listDropDownTreeItem;
    }


    [HttpPost]
    public virtual IActionResult LoadNearMiss(Guid id)
    {
        Console.WriteLine($"Data ID>>{JsonConvert.SerializeObject(id)}");
        var commonData = SafetyIncidentService.GetById(id);
        Console.WriteLine($"Data Result>>{JsonConvert.SerializeObject(commonData)}");
        return PartialView("_NearMissForm", commonData);
    }

    [HttpPost]
    public virtual IActionResult LoadPropertyDamage(Guid id)
    {
        Console.WriteLine($"Data ID>>{JsonConvert.SerializeObject(id)}");
        var commonData = SafetyIncidentService.GetById(id);
        Console.WriteLine($"Data Result>>{JsonConvert.SerializeObject(commonData)}");
        return PartialView("_PropertyDamageForm", commonData);
    }

    [HttpPost]
    public virtual IActionResult LoadWorkingAccident(Guid id)
    {
        Console.WriteLine($"Data ID>>{JsonConvert.SerializeObject(id)}");
        var commonData = SafetyIncidentService.GetById(id);
        Console.WriteLine($"Data Result>>{JsonConvert.SerializeObject(commonData)}");
        return PartialView("_WorkingAccidentForm", commonData);
    }

    [HttpPost]
    public virtual IActionResult LoadTrafficAccident(Guid id)
    {
        Console.WriteLine($"Data ID>>{JsonConvert.SerializeObject(id)}");
        var commonData = SafetyIncidentService.GetById(id);
        Console.WriteLine($"Data Result>>{JsonConvert.SerializeObject(commonData)}");
        return PartialView("_TrafficAccidentForm", commonData);
    }

    [HttpPost]
    public virtual IActionResult LoadTotalWorker(Guid id)
    {
        // Ambil data dari service
        var commonData = TotalEmployeeService.GetById(id);
        Console.WriteLine($"commonData>>>{JsonConvert.SerializeObject(commonData)}");

        return PartialView("_TotalWorkerForm", commonData);
    }

    [HttpPost]
    public virtual IActionResult LoadSafetyFacility(Guid id)
    {
        Console.WriteLine($"Data ID>>{JsonConvert.SerializeObject(id)}");
        var commonData = SafetyFacilityService.GetById(id);
        Console.WriteLine($"Data Result>>{JsonConvert.SerializeObject(commonData)}");
        return PartialView("_SafetyFacilityForm", commonData);
    }

    [HttpPost]
    public virtual IActionResult LoadFireProtection(Guid id)
    {
        Console.WriteLine($"Data ID>>{JsonConvert.SerializeObject(id)}");
        var commonData = FireProtectionService.GetById(id);
        Console.WriteLine($"Data Result>>{JsonConvert.SerializeObject(commonData)}");
        return PartialView("_FireProtectionForm", commonData);
    }

    [HttpPost]
    public virtual IActionResult LoadAparRefill(Guid id)
    {
        Console.WriteLine($"Data ID>>{JsonConvert.SerializeObject(id)}");
        var commonData = APARRefillService.GetById(id);
        Console.WriteLine($"Data Result>>{JsonConvert.SerializeObject(commonData)}");
        return PartialView("_AparRefillForm", commonData);
    }

    [HttpPost]
    public virtual IActionResult LoadTrainingRecord(Guid id)
    {
        Console.WriteLine($"Data ID>>{JsonConvert.SerializeObject(id)}");
        var commonData = TrainingRecordService.GetById(id);
        Console.WriteLine($"Data Result>>{JsonConvert.SerializeObject(commonData)}");
        return PartialView("_TrainingRecordForm", commonData);
    }

    [HttpPost]
    public virtual IActionResult LoadProjectActivity(Guid id)
    {
        Console.WriteLine($"Data ID>>{JsonConvert.SerializeObject(id)}");
        var commonData = ProjectActivityService.GetById(id);
        Console.WriteLine($"Data Result>>{JsonConvert.SerializeObject(commonData)}");
        return PartialView("_ProjectActivityForm", commonData);
    }
}
#endregion