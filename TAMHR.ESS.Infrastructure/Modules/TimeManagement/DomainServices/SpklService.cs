using Agit.Common;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// SPKL Service Class
    /// </summary>
    public class SpklService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Document approval repository
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository { get { return UnitOfWork.GetRepository<DocumentApproval>(); } }

        /// <summary>
        /// Spkl master repository
        /// </summary>
        protected IRepository<TimeManagementSpkl> SpklMasterRepository { get { return UnitOfWork.GetRepository<TimeManagementSpkl>(); } }

        /// <summary>
        /// Spkl request repository
        /// </summary>
        protected IRepository<SpklRequest> SpklRequestRepository { get { return UnitOfWork.GetRepository<SpklRequest>(); } }

        /// <summary>
        /// Spkl request readonly repository
        /// </summary>
        protected IReadonlyRepository<SpklRequestDetailView> SpklRequestReadonlyRepository { get { return UnitOfWork.GetRepository<SpklRequestDetailView>(); } }

        protected IReadonlyRepository<DocumentRequestDetail> documentRequestRepository { get { return UnitOfWork.GetRepository<DocumentRequestDetail>(); } }
        protected IReadonlyRepository<ActualOrganizationStructure> actualOrganization { get { return UnitOfWork.GetRepository<ActualOrganizationStructure>(); } }
        protected IReadonlyRepository<AnnualLeavePlanningDetail> LeavePlanningDetail { get { return UnitOfWork.GetRepository<AnnualLeavePlanningDetail>(); } }
        protected IReadonlyRepository<AnnualLeavePlanning> AnnualLeavePlannings { get { return UnitOfWork.GetRepository<AnnualLeavePlanning>(); } }
        #endregion

        #region Constructor
        public SpklService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        /// <summary>
        /// Check whether spkl request with temp id has valid owner or not
        /// </summary>
        /// <param name="noreg">Owner NoReg/SPKL Document Creator</param>
        /// <param name="tempId">Spkl Request Temporary Id</param>
        /// <param name="checkDocumentApproval">Check with Document Approval</param>
        /// <returns>True if owner is valid, false otherwise</returns>
        public bool IsValidDocument(string noreg, Guid tempId, bool checkDocumentApproval = false)
        {
            var hasSpkl = SpklRequestRepository.Fetch()
                .AsNoTracking()
                .Any(x => x.TempId == tempId && x.CreatedBy == noreg);

            var all = SpklRequestRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.TempId == tempId)
                .All(x => x.CreatedBy == noreg && (!checkDocumentApproval || x.DocumentApprovalId == null));

            return hasSpkl && all;
        }

        /// <summary>
        /// Get spkl request details by temp id and noreg
        /// </summary>
        /// <param name="tempId">Temporary Id</param>
        /// <param name="noreg">NoReg</param>
        /// <returns>Spkl Request Query</returns>
        public IQueryable<SpklRequestDetailView> GetSpklRequestDetails(Guid tempId, string noreg)
        {
            return SpklRequestReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.TempId == tempId && x.NoReg == noreg);
        }

        public IEnumerable<SpklRequestDetailStoredEntity> GetSpklRequestDetailsByUser(string noreg, string username, string orgCode)
        {
            return UnitOfWork.UdfQuery<SpklRequestDetailStoredEntity>(new { noreg, username, orgCode });
        }

        /// <summary>
        /// Get list of spkl request details by parent id and key date
        /// </summary>
        /// <param name="tempId">Temp Id</param>
        /// <param name="keyDate">Key Date</param>
        /// <returns>List of Spkl Request Details</returns>
        public IEnumerable<SpklRequestDetailView> GetSpklRequestDetails(Guid parentId, DateTime keyDate)
        {
            return SpklRequestReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.ParentId == parentId && x.OvertimeDate.Date == keyDate.Date)
                .OrderBy(x => x.Name);
        }

        /// <summary>
        /// Get list of spkl request details by document approval parent id and key date
        /// </summary>
        /// <param name="parentId">Document Approval Parent Id</param>
        /// <param name="keyDate">Key Date</param>
        /// <returns>List of Spkl Request Details</returns>
        public IEnumerable<SpklRequestDetailView> GetCompletedSpklRequestDetails(Guid parentId, DateTime keyDate)
        {
            return SpklRequestReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.ParentId == parentId && x.OvertimeDate.Date == keyDate.Date && x.DocumentStatusCode == DocumentStatus.Completed)
                .OrderBy(x => x.Name);
        }

        /// <summary>
        /// Get list of completed spkl request details by document approval parent id
        /// </summary>
        /// <param name="parentId">Document Approval Parent Id</param>
        /// <returns>List of Spkl Request Dates</returns>
        public IEnumerable<string> GetCompletedSpklRequestDates(Guid parentId)
        {
            return SpklRequestReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.ParentId == parentId && x.DocumentStatusCode == DocumentStatus.Completed)
                .OrderBy(x => x.OvertimeDate)
                .Select(x => x.OvertimeDate.ToString("dd/MM/yyyy"))
                .Distinct();
        }

        /// <summary>
        /// Get list of completed spkl details by organization code and class range
        /// </summary>
        /// <param name="orgCode">Organization Code</param>
        /// <param name="keyDate">Key Date</param>
        /// <param name="min">Min Class</param>
        /// <param name="max">Max Class</param>
        /// <returns>List of Spkl Master Data</returns>
        public IEnumerable<SpklRequestOrganizationStoredEntity> GetSpklRequestsByOrganization(string orgCode, DateTime keyDate, int min, int max)
        {
            return UnitOfWork.UdfQuery<SpklRequestOrganizationStoredEntity>(new { orgCode, keyDate, min, max });
        }

        /// <summary>
        /// Get list of spkl plan request details by document approval parent id
        /// </summary>
        /// <param name="parentId">Document Approval Parent Id</param>
        /// <returns>List of Spkl Request Dates</returns>
        public IEnumerable<string> GetSpklRequestDates(Guid parentId)
        {
            return SpklRequestReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.ParentId == parentId)
                .OrderBy(x => x.OvertimeDate)
                .Select(x => x.OvertimeDate.ToString("dd/MM/yyyy"))
                .Distinct();
        }

        /// <summary>
        /// Get spkl request detail by document approval id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>Spkl Request Detail</returns>
        public SpklRequestDetailView GetSpklRequestDetailById(Guid documentApprovalId)
        {
            return SpklRequestReadonlyRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.RowStatus && x.DocumentApprovalId == documentApprovalId);
        }

        /// <summary>
        /// Get spkl request detail by noreg and username
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="username">Username</param>
        /// <returns>List of Document Numbers</returns>
        public IEnumerable<SpklRequestDetailDocumentNumberStoredEntity> GetSpklRequestDetailDocumentNumbers(string noreg, string username, int month, int year)
        {
            return UnitOfWork.UdfQuery<SpklRequestDetailDocumentNumberStoredEntity>(new { noreg, username, month, year });
        }

        /// <summary>
        /// Update spkl by document approval id and temporary id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <param name="tempId">Temporary Id</param>
        public void UpdateSpkl(Guid documentApprovalId, Guid tempId)
        {
            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_CREATE_SPKL", new { documentApprovalId, tempId }, trans);
            });
        }

        public void Approve(string noreg, DocumentApproval documentApproval)
        {
            
        }
        /// <summary>
        /// Update spkl filter
        /// </summary>
        /// <param name="viewModel">Spkl Filter View Model</param>
        public void UpdateSpkl(SpklOvertimeUpdateViewModel viewModel)
        {
            var spklRequest = SpklRequestRepository.Fetch().FirstOrDefault(x => x.Id == viewModel.Id);

            spklRequest.OvertimeInAdjust = viewModel.OvertimeInAdjust;
            spklRequest.OvertimeOutAdjust = viewModel.OvertimeOutAdjust;
            spklRequest.OvertimeBreakAdjust = viewModel.OvertimeBreakAdjust;
            spklRequest.DurationAdjust = ServiceHelper.CalculateProxyDuration(viewModel.OvertimeIn, viewModel.OvertimeOut, viewModel.OvertimeInAdjust, viewModel.OvertimeOutAdjust, viewModel.NormalTimeIn, viewModel.NormalTimeOut, viewModel.OvertimeBreakAdjust);

            UnitOfWork.SaveChanges();
        }

        public void UpdateSpklDetail(SpklOvertimeUpdateViewModel viewModel)
        {
            var spklRequest = SpklRequestRepository.Fetch().FirstOrDefault(x => x.Id == viewModel.Id);

            spklRequest.OvertimeIn = viewModel.OvertimeIn;
            spklRequest.OvertimeOut = viewModel.OvertimeOut;
            spklRequest.OvertimeInAdjust = viewModel.OvertimeIn;
            spklRequest.OvertimeOutAdjust = viewModel.OvertimeOut;
            spklRequest.OvertimeBreak = viewModel.OvertimeBreak;
            spklRequest.OvertimeBreakAdjust = viewModel.OvertimeBreak;
            spklRequest.Duration = viewModel.DurationAdjust;
            spklRequest.DurationAdjust = viewModel.DurationAdjust;
            spklRequest.OvertimeCategoryCode = viewModel.OvertimeCategoryCode;
            spklRequest.OvertimeReason = viewModel.OvertimeReason;
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Update multiple spkl filter
        /// </summary>
        /// <param name="viewModel">List of Spkl Filter View Model</param>
        public void UpdateSpkl(SpklOvertimeUpdateViewModel[] viewModels)
        {
            Assert.ThrowIf(viewModels == null || viewModels.Length == 0, "Data cannot be empty");

            foreach (var viewModel in viewModels)
            {
                var spklRequest = SpklRequestRepository.Fetch().FirstOrDefault(x => x.Id == viewModel.Id);

                spklRequest.OvertimeInAdjust = viewModel.OvertimeInAdjust;
                spklRequest.OvertimeOutAdjust = viewModel.OvertimeOutAdjust;
                spklRequest.OvertimeBreakAdjust = viewModel.OvertimeBreakAdjust;
                spklRequest.DurationAdjust = ServiceHelper.CalculateProxyDuration(viewModel.OvertimeIn, viewModel.OvertimeOut, viewModel.OvertimeInAdjust, viewModel.OvertimeOutAdjust, viewModel.NormalTimeIn, viewModel.NormalTimeOut, viewModel.OvertimeBreakAdjust);
            }

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Function that triggered after submit SPKL document
        /// </summary>
        /// <param name="noreg">Noreg</param>
        /// <param name="documentApproval">Document Approval</param>
        public void Submit(string noreg, DocumentApproval documentApproval)
        {
            var documentNumber = documentApproval.DocumentNumber;

            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPDATE_SPKL_NOTIFICATION", new { actor = noreg, documentNumber }, trans);

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Complete document function
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="documentApproval">Spkl Master Document Approval</param>
        public void Complete(string noreg, DocumentApproval documentApproval)
        {
            //var coreService = new CoreService(UnitOfWork);
            var spklRequest = SpklRequestRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.DocumentApprovalId == documentApproval.Id);
            var spklRequestMaster = SpklMasterRepository.Fetch().FirstOrDefault(x => x.NoReg == spklRequest.NoReg && x.OvertimeDate.Date == spklRequest.OvertimeDate.Date && x.OvertimeInPlan == spklRequest.OvertimeIn);

            if (spklRequestMaster == null)
            {
                SpklMasterRepository.Add(TimeManagementSpkl.Create(spklRequest));
            }
            else
            {
                spklRequestMaster.OvertimeBreakPlan = spklRequest.OvertimeBreak;
                spklRequestMaster.OvertimeBreakAdjust = spklRequest.OvertimeBreakAdjust.Value;
                spklRequestMaster.OvertimeInPlan = spklRequest.OvertimeIn;
                spklRequestMaster.OvertimeInAdjust = spklRequest.OvertimeInAdjust.Value;
                spklRequestMaster.OvertimeOutPlan = spklRequest.OvertimeOut;
                spklRequestMaster.OvertimeOutAdjust = spklRequest.OvertimeOutAdjust.Value;
                spklRequestMaster.DurationPlan = spklRequest.Duration;
                spklRequestMaster.DurationAdjust = spklRequest.DurationAdjust.Value;
                spklRequestMaster.OvertimeCategoryCode = spklRequest.OvertimeCategoryCode;
                spklRequestMaster.OvertimeReason = spklRequest.OvertimeReason;
            }

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Update spkl details by spkl filter
        /// </summary>
        /// <param name="actor">NoReg</param>
        /// <param name="documentRequestDetail">Document Spkl Filter</param>
        public void UpdateSpkl(string actor, DocumentRequestDetailViewModel<SpklOvertimeViewModel> documentRequestDetail)
        {
            var documentParentId = documentRequestDetail.DocumentApprovalId;
            var viewModel = documentRequestDetail.Object;
            var tempId = viewModel.TempId;
            var noregs = "(" + string.Join("),(", viewModel.NoRegs) + ")";
            var noregss = viewModel.NoRegs.ToList();

            var overtimeDateIn = viewModel.OvertimeDate;
            var overtimeDateOut = viewModel.OvertimeDateOut;
            var overtimeIn = viewModel.OvertimeHourIn.Value.ToString("hh\\:mm\\:ss");
            var overtimeOut = viewModel.OvertimeHourOut.Value.ToString("hh\\:mm\\:ss");
            var overtimeBreak = viewModel.OvertimeTimeBreak;
            var overtimeCategoryCode = viewModel.Category;
            var overtimeReason = viewModel.Reason;

            var overtimeDateIndate = viewModel.OvertimeDate.Value.Date;
            var overtimeDateOutdate = viewModel.OvertimeDateOut.Value.Date;

            int overtimeDateIndateInt = int.Parse(overtimeDateIndate.ToString("yyyyMMdd"));
            int overtimeDateOutdateInt = int.Parse(overtimeDateOutdate.ToString("yyyyMMdd"));

            // Convert ke DateTime untuk hindari ToString()
            var startDate = new DateTime(overtimeDateOutdateInt / 10000,
                                        (overtimeDateOutdateInt % 10000) / 100,
                                        overtimeDateOutdateInt % 100);
            var endDate = new DateTime(overtimeDateIndateInt / 10000,
                                      (overtimeDateIndateInt % 10000) / 100,
                                      overtimeDateIndateInt % 100);

            // Fetch and filter records
            //var documentRequestDetails = documentRequestRepository.Fetch()
            //                                      .Where(ab => noregss.Contains(ab.CreatedBy))
            //                                      .ToList();
            var documentRequestDetails = new List<DocumentRequestDetail>();

            // Pakai for loop biasa, jangan pakai contains
            foreach (var noreg in noregss)
            {
                var hasilPerNoreg = documentRequestRepository.Fetch()
                                          .Where(ab => ab.CreatedBy == noreg) // PAKAI ==, BUKAN CONTAINS
                                          .ToList();

                documentRequestDetails.AddRange(hasilPerNoreg);
            }

            // Hapus duplicate jika perlu
            documentRequestDetails = documentRequestDetails.Distinct().ToList();

            var validReasonTypes = new List<string> { "cuti", "cutipanjang" };

            var reasonTypeMapping = new Dictionary<string, string>
                                        {
                                            { "Cuti Tahunan", "Annual Leave" },
                                            { "Cuti Panjang / Besar", "Long Leave" }
                                        };

            //var checkAbsent = (from ab in documentRequestDetails
            //                   let objJson = ab.ObjectValue
            //                   let obj = !string.IsNullOrEmpty(objJson) && !objJson.StartsWith("[")
            //                       ? JsonConvert.DeserializeObject<AbsenceViewModel>(objJson)
            //                       : null
            //                   join strt in actualOrganization.Fetch() on ab.CreatedBy equals strt.NoReg into joined
            //                   from strt in joined.DefaultIfEmpty() // Left Join
            //                   where obj != null &&
            //                         obj.StartDate.HasValue &&
            //                         obj.EndDate.HasValue &&
            //                         Convert.ToInt32(obj.StartDate.Value.ToString("yyyyMMdd")) <= overtimeDateOutdateInt &&
            //                         Convert.ToInt32(obj.EndDate.Value.ToString("yyyyMMdd")) >= overtimeDateIndateInt &&
            //                         validReasonTypes.Contains(obj.ReasonType) &&
            //                         noregss.Contains(ab.CreatedBy)
            //                   select new
            //                   {
            //                       ab.CreatedBy,
            //                       StartDate = obj.StartDate,
            //                       EndDate = obj.EndDate,
            //                       Name = strt.Name,
            //                       Reason = obj.Reason,
            //                       AbsentsID = ab.DocumentApprovalId
            //                   }).ToList();

            var checkAbsent = documentRequestDetails
                                .AsEnumerable()
                                .Select(ab =>
                                {
                                    var obj = !string.IsNullOrEmpty(ab.ObjectValue) && !ab.ObjectValue.StartsWith("[")
                                        ? JsonConvert.DeserializeObject<AbsenceViewModel>(ab.ObjectValue)
                                        : null;
                                    return new { ab, obj };
                                })
                                .Where(x => x.obj != null &&
                                           x.obj.StartDate.HasValue &&
                                           x.obj.EndDate.HasValue &&
                                           x.obj.StartDate.Value <= endDate &&  // ← FIX: DateTime comparison
                                           x.obj.EndDate.Value >= startDate &&   // ← FIX: DateTime comparison
                                           validReasonTypes.Contains(x.obj.ReasonType) &&
                                           noregss.Contains(x.ab.CreatedBy))
                                .Select(x =>
                                {
                                    var strt = actualOrganization.Fetch()
                                        .AsEnumerable()
                                        .FirstOrDefault(s => s.NoReg == x.ab.CreatedBy);
                                
                                    return new
                                    {
                                        x.ab.CreatedBy,
                                        StartDate = x.obj.StartDate,
                                        EndDate = x.obj.EndDate,
                                        Name = strt?.Name,
                                        Reason = x.obj.Reason,
                                        AbsentsID = x.ab.DocumentApprovalId
                                    };
                                })
                                .ToList();

            bool checkAbsents = checkAbsent.Any();

            List<DocumentApproval> checkdocument = new List<DocumentApproval>();

            if (checkAbsents)
            {
                var absentsIds = checkAbsent.Select(x => x.AbsentsID).ToList();
                checkdocument = DocumentApprovalRepository.Fetch()
                    .AsNoTracking()
                    .Where(x => absentsIds.Contains(x.Id) && new List<string> { "completed", "inprogress" }.Contains(x.DocumentStatusCode))
                    .ToList();
            }

            string topReason = checkAbsent
                .Select(x => reasonTypeMapping.ContainsKey(x.Reason) ? reasonTypeMapping[x.Reason] : x.Reason)
                .FirstOrDefault();

            bool checkdocuments = checkdocument.Any();

            if (checkdocuments)
            {
                string names = string.Join(", ", checkAbsent
                                          .Where(x => Convert.ToInt32(x.StartDate.Value.ToString("yyyyMMdd")) <= overtimeDateOutdateInt &&
                                                      Convert.ToInt32(x.EndDate.Value.ToString("yyyyMMdd")) >= overtimeDateIndateInt)
                                          .Select(x => x.Name)
                                          .Distinct()
                                          .ToList());

                throw new InvalidOperationException($"Sorry, please be advised that {names} have already planned to take {topReason} on this day.");
            }


            List<Guid> absentIds = new List<Guid>
            {
               Guid.Parse("91FDCA7E-8DAE-430D-BBED-D999F0EB46D4"),  //annualleave
               Guid.Parse("A039EA35-C3BA-43DA-A767-A9C3DF67486B")  // long leave
            };

            //var checkLeavePlanning = (from lp in LeavePlanningDetail.Fetch().AsNoTracking()
            //                          join al in AnnualLeavePlannings.Fetch().AsNoTracking()
            //                          on lp.AnnualLeavePlanningId equals al.Id into alGroup
            //                          from al in alGroup.DefaultIfEmpty() 
            //                          where noregss.Any(nr => lp.CreatedBy.Contains(nr)) &&
            //                                Convert.ToInt32(lp.StartDate.ToString("yyyyMMdd")) <= overtimeDateOutdateInt &&
            //                                Convert.ToInt32(lp.EndDate.ToString("yyyyMMdd")) >= overtimeDateIndateInt &&
            //                                absentIds.Contains(lp.AbsentId)
            //                          select new { al.DocumentApprovalId })
            //                          .ToList();
            var allLeavePlanning = LeavePlanningDetail.Fetch()
                                      .AsEnumerable()  // ← PINDAH KE MEMORY
                                      .Where(lp => noregss.Contains(lp.CreatedBy) &&
                                                  lp.StartDate <= endDate &&  // ← FIX: DateTime comparison
                                                  lp.EndDate >= startDate &&   // ← FIX: DateTime comparison
                                                  absentIds.Contains(lp.AbsentId))
                                      .ToList();

            var checkLeavePlanning = (from lp in allLeavePlanning
                                      join al in AnnualLeavePlannings.Fetch().AsEnumerable()
                                      on lp.AnnualLeavePlanningId equals al.Id into alGroup
                                      from al in alGroup.DefaultIfEmpty()
                                      select new { al?.DocumentApprovalId })
                                     .ToList();

            bool isPlanAbsent = checkLeavePlanning.Any();

            List<DocumentApproval> checkdocumentplan = new List<DocumentApproval>(); 

            if (isPlanAbsent)
            {
                var planId = checkLeavePlanning.Select(x => x.DocumentApprovalId).ToList();

                checkdocumentplan = DocumentApprovalRepository.Fetch()
                  .AsNoTracking()
                  .Where(x => planId.Contains(x.Id) && new List<string> { "completed", "draft", "inprogress" }.Contains(x.DocumentStatusCode))
                  .ToList();

            }

            bool checkPlandocument = checkdocumentplan.Any();

            if (checkPlandocument)
            {
                var names = string.Join(", ",
                    (from lp in LeavePlanningDetail.Fetch().AsNoTracking()
                     join ao in actualOrganization.Fetch().AsNoTracking()
                     on lp.CreatedBy equals ao.NoReg
                     where noregss.Any(nr => lp.CreatedBy.Contains(nr)) &&
                         Convert.ToInt32(lp.StartDate.ToString("yyyyMMdd")) <= overtimeDateOutdateInt &&
                         Convert.ToInt32(lp.EndDate.ToString("yyyyMMdd")) >= overtimeDateIndateInt &&
                            absentIds.Contains(lp.AbsentId)
                     select ao.Name).ToList());

                throw new InvalidOperationException($"Sorry, please be advised that {names} have already planned to take Annual Leave Planning on this day.");
            }


            UnitOfWork.Transact(trans =>
            {
                var parameters = new {
                    actor,
                    documentParentId,
                    tempId,
                    overtimeDateIn,
                    overtimeDateOut,
                    overtimeBreak,
                    overtimeIn,
                    overtimeOut,
                    overtimeCategoryCode,
                    overtimeReason,
                    noregs
                };

                UnitOfWork.UspQuery("SP_UPDATE_SPKL", parameters, trans);

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Delete spkl request from database
        /// </summary>
        /// <param name="id">Spkl Request Id</param>
        public void Delete(Guid id)
        {
            UnitOfWork.Transact(trans =>
            {
                SpklRequestRepository.DeleteById(id);

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Upload spkl data
        /// </summary>
        /// <param name="actor">NoReg</param>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <param name="tempId">Temporary Id</param>
        /// <param name="dt">Data Input</param>
        public void UploadSpkl(string actor, string postCode, Guid documentApprovalId, Guid tempId, DataTable dt)
        {
            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPLOAD_SPKL", new { actor, postCode, documentApprovalId, tempId, data = dt.AsTableValuedParameter("TVP_SPKL_REQUEST") }, trans);

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Upload spkl data
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="username">Username</param>
        /// <param name="dt">Data Input</param>
        public void UploadSpklReport(string noreg, string username, DataTable dt)
        {
            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPLOAD_SPKL_REPORT", new { noreg, username, data = dt.AsTableValuedParameter("TVP_SPKL_REPORT") }, trans);

                UnitOfWork.SaveChanges();
            });
        }

        public DataSourceResult GetSpklReportDetails(DataSourceRequest request, string noreg, string username, string orgCode)
        {
            var data = UnitOfWork.UdfQuery<SpklRequestDetailReportStoredEntity>(new { noreg, username, orgCode });
            return data.ToDataSourceResult(request);
        }
    }
}
