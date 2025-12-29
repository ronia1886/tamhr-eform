using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Agit.Common.Extensions;
using Newtonsoft.Json;
using Agit.Domain.Extensions;
using TAMHR.ESS.Infrastructure.ViewModels;
using Dapper;
using Microsoft.Extensions.Localization;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle proxy time master data.
    /// </summary>
    public class AbnormalityAbsenceService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// User repository object.
        /// </summary>
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();

        /// <summary>
        /// Abnormallity Absence shift repository object.
        /// </summary>
        protected IRepository<EmpWorkSchSubtitute> EmpWorkSchSubtituteRepository => UnitOfWork.GetRepository<EmpWorkSchSubtitute>();

        /// <summary>
        /// DocumentApproval repository object.
        /// </summary>
        protected IRepository<AbnormalityAbsence> AbnormalityAbsenceRepository => UnitOfWork.GetRepository<AbnormalityAbsence>();

        /// <summary>
        /// DocumentApproval repository object.
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();


        /// <summary>
        /// DocumentRequestDetail repository object.
        /// </summary>
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();

        /// <summary>
        /// Abnormallity Absence repository object.
        /// </summary>
        protected IRepository<AbnormalityAbsenceView> AbnormalityAbsenceViewRepository => UnitOfWork.GetRepository<AbnormalityAbsenceView>();

        /// <summary>
        /// Time management repository object.
        /// </summary>
        protected IRepository<TimeManagement> TimeManagementRepository => UnitOfWork.GetRepository<TimeManagement>();


        /// <summary>
        /// Abnormallity Absence history repository object.
        /// </summary>
        protected IRepository<TimeManagementHistory> TimeManagementHistoryRepository => UnitOfWork.GetRepository<TimeManagementHistory>();

        /// <summary>
        /// Abnormallity Absence readonly repository object.
        /// </summary>
        protected IReadonlyRepository<AbnormalityAbsenceView> TimeManagementReadonlyRepository => UnitOfWork.GetRepository<AbnormalityAbsenceView>();

        /// <summary>
        /// Daily work schedule readonly repository object.
        /// </summary>
        protected IReadonlyRepository<DailyWorkSchedule> DailyWorkScheduleReadonlyRepository => UnitOfWork.GetRepository<DailyWorkSchedule>();

        /// <summary>
        /// Document request detail repository
        /// </summary>
        protected IRepository<DocumentRequestMemoAbsenceView> DocumentRequestMemoAbsenceViewRepository => UnitOfWork.GetRepository<DocumentRequestMemoAbsenceView>();

        /// <summary>
        /// Common file repository
        /// </summary>
        protected IRepository<CommonFile> CommonFileRepository => UnitOfWork.GetRepository<CommonFile>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated.
        /// </summary>
        private readonly string[] _properties = new[] {
            "WorkingTimeIn",
            "WorkingTimeOut",
            "AbsentStatus",
            "Description",
            "ShiftCode",
            "NormalTimeIn",
            "NormalTimeOut",
            "AbnormalityStatus",
            "Reason",
            "CommonFileId",
            "DocumentApprovalId"
        };

        private readonly string[] _propertiesTimeManaGement = new[] {
            "WorkingTimeIn",
            "WorkingTimeOut",
            "AbsentStatus",
            "Description",
            "ShiftCode",
            "NormalTimeIn",
            "NormalTimeOut",
           
        };
        #endregion


        private IStringLocalizer<IUnitOfWork> _localizer;

        #region Constructor
        public AbnormalityAbsenceService(IUnitOfWork unitOfWork, IStringLocalizer<IUnitOfWork> localizer)
            : base(unitOfWork)
        {
            _localizer = localizer;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get Abnormallity Absence query
        /// </summary>
        /// <returns>Abnormallity Absence Query</returns>
        public IQueryable<AbnormalityAbsenceView> GetQuery()
        {
            return TimeManagementReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Get Abnormallity Absence query
        /// </summary>
        /// <returns>Abnormallity Absence Query</returns>
        public IQueryable<AbnormalityAbsenceView> GetQuery(string noreg, string name, DateTime? startDate, DateTime? endDate)
        {
            return GetQuery()
                .Where(x => (x.Name.Contains(name) || x.NoReg.Contains(noreg)) && (!startDate.HasValue || x.WorkingDate >= startDate.Value) && (!endDate.HasValue || x.WorkingDate <= endDate.Value))
                .OrderBy(x => x.Name)
                .ThenBy(x => x.WorkingDate);
        }

        public IQueryable<AbnormalityAbsenceView>GetsByCurrentUser(string noreg)
        {
            // Create config service object.
            var configService = new ConfigService(UnitOfWork);

            // Get start and end Date.
            var configStart = configService.GetConfig("Abnormality.StartDate");
            var configEnd = configService.GetConfig("Abnormality.EndDate");
            //var configMonth = configService.GetConfig("Abnormality.GetData.Month");

            var obj = DocumentRequestMemoAbsenceViewRepository.Fetch().Where(x => x.SubmitBy == noreg).Select(x => x.ObjectValue).ToList();
            List<AbnormalityAbsenceView> list = new List<AbnormalityAbsenceView>();
            foreach (var item in obj)
            {
                var result = JsonConvert.DeserializeObject<AbnormalityAbsenceViewModel>(item);
                list.AddRange(result.Details);
            }

            //int year = DateTime.Now.Year;
            //int month = DateTime.Now.Month - Convert.ToInt32(configMonth.ConfigValue);
            //if(month < 1)
            //{
            //    month += 12;
            //    year -= 1;
            //}

            //DateTime firstMonthDate = new DateTime(year, month, 1);

            return AbnormalityAbsenceViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.WorkingDate >= Convert.ToDateTime(configStart.ConfigValue) && x.WorkingDate <= Convert.ToDateTime(configEnd.ConfigValue) &&
                    ((
                    //x.AbnormalityStatus != "Progress" && 
                    x.AbnormalityStatus != "Completed") || x.AbnormalityStatus == null)
                ).OrderBy(x => x.WorkingDate);
        }

        /// <summary>
        /// Get Abnormallity Absence history query by date
        /// </summary>
        /// <param name="date">Working Date</param>
        /// <returns>Abnormallity Absence History Query</returns>
        public IQueryable<TimeManagementHistory> GetHistoriesQuery(string noreg, DateTime date)
        {
            var clearDate = date.Date;

            return TimeManagementHistoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.WorkingDate == date);
        }

        public CommonFile GetCommonFIle(Guid commonFileId)
        {
            return CommonFileRepository.FindById(commonFileId);
        }

        /// <summary>
        /// Get list of Abnormallity Absence
        /// </summary>
        /// <returns>List of Abnormallity Absence</returns>
        public IEnumerable<AbnormalityAbsenceView> Gets()
        {
            return GetQuery().ToList();
        }

        /// <summary>
        /// Get Abnormallity Absence by id
        /// </summary>
        /// <param name="id">Abnormallity Absence Id</param>
        /// <returns>Abnormallity Absence Object</returns>
        public AbnormalityAbsenceView Get(Guid id)
        {
            var result = GetQuery()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
            return result;
        }

        /// <summary>
        /// Complete Action Approval in Abnormality Absence
        /// </summary>
        /// <param name="noregCurentApprover"></param>
        /// <param name="documentApproval"></param>
        public void CompleteApprove(string noregCurentApprover, DocumentApproval documentApproval)
        {
           // var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            List<AbnormalityAbsenceView> list = new List<AbnormalityAbsenceView>();
         
            var result = JsonConvert.DeserializeObject<AbnormalityAbsenceViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                var abnormality = AbnormalityAbsenceRepository.FindById(item.AbnormalityAbsenceId);
                abnormality.AbnormalityStatus = "Completed";

                UnitOfWork.SaveChanges();
            }

            
            foreach (var item in result.Details)
            {
                var obj = new TimeManagement();
                obj.Id = item.Id;
                obj.WorkingTimeIn = item.AbnormaityProxyIn;
                obj.WorkingTimeOut = item.AbnormalityProxyOut;
                obj.ShiftCode = item.ShiftCode;
                obj.AbsentStatus = item.AbnormalityAbsenStatus;
                obj.Description = item.Reason;
                obj.NoReg = item.NoReg;

                UpsertTimeManagement(obj.CreatedBy, obj);

            }

        }

        /// <summary>
        /// Complete Action Approval in Abnormality Absence
        /// </summary>
        /// <param name="noregCurentApprover"></param>
        /// <param name="documentApproval"></param>
        public void Update(string noregCurentApprover, DocumentApproval documentApproval)
        {
            // var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            
            documentRequestDetail.ObjectValue = documentApproval.ObjDocumentRequestDetail.ObjectValue;
            UnitOfWork.SaveChanges();
        }

        public void Submit(string noreg, DocumentApproval documentApproval)
        {
            var documentId = documentApproval.Id;
            var documentNumber = documentApproval.DocumentNumber;

            // var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            List<AbnormalityAbsenceView> list = new List<AbnormalityAbsenceView>();

            var result = JsonConvert.DeserializeObject<AbnormalityAbsenceViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                var abnormality = AbnormalityAbsenceRepository.FindById(item.AbnormalityAbsenceId);
                abnormality.DocumentApprovalId = documentId;
                abnormality.AbnormalityStatus = "Progress";
                AbnormalityAbsenceRepository.Upsert<Guid>(abnormality, _properties);
                UnitOfWork.SaveChanges();
            }
        }

        public void Reject(string noreg, DocumentApproval documentApproval)
        {
            var documentId = documentApproval.Id;
            var documentNumber = documentApproval.DocumentNumber;

            // var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            List<AbnormalityAbsenceView> list = new List<AbnormalityAbsenceView>();

            var result = JsonConvert.DeserializeObject<AbnormalityAbsenceViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                var abnormality = AbnormalityAbsenceRepository.FindById(item.AbnormalityAbsenceId);
                abnormality.AbnormalityStatus = "Rejected";
                AbnormalityAbsenceRepository.Upsert<Guid>(abnormality, _properties);
                UnitOfWork.SaveChanges();
            }
        }

        public void Approve(string noreg, DocumentApproval documentApproval)
        {
            var documentId = documentApproval.Id;
            var documentNumber = documentApproval.DocumentNumber;

            var abnormalityList = AbnormalityAbsenceRepository.Find(x => x.DocumentApprovalId == documentId && x.AbnormalityStatus != "Locked");

            var result = JsonConvert.DeserializeObject<AbnormalityAbsenceViewModel>(documentApproval.ObjDocumentRequestDetail.ObjectValue);

            JsonConvert.DeserializeObject(documentApproval.ObjDocumentRequestDetail.ObjectValue);

            foreach (var item in abnormalityList)
            {
                if (result.Details.Where(x => x.AbnormalityAbsenceId == item.Id).ToList().Count() == 0)
                {
                    item.AbnormalityStatus = "Rejected";
                    AbnormalityAbsenceRepository.Upsert<Guid>(item, _properties);
                }
            }
            UnitOfWork.SaveChanges();
        }

        public void Revise(string noreg, DocumentApproval documentApproval)
        {
            var documentId = documentApproval.Id;
            var documentNumber = documentApproval.DocumentNumber;

            // var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            List<AbnormalityAbsenceView> list = new List<AbnormalityAbsenceView>();

            var result = JsonConvert.DeserializeObject<AbnormalityAbsenceViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                var abnormality = AbnormalityAbsenceRepository.FindById(item.AbnormalityAbsenceId);
                abnormality.AbnormalityStatus = "Revised";
                AbnormalityAbsenceRepository.Upsert<Guid>(abnormality, _properties);
                UnitOfWork.SaveChanges();
            }
        }

        /// <summary>
        /// Update or insert time management
        /// </summary>
        /// <param name="timeManagement">Time Management Object</param>
        public void Upsert(string actor, AbnormalityAbsence timeManagement)
        {
            var tm = TimeManagementRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == timeManagement.TimeManagementId);

            var now = DateTime.Now;

            var shift = DailyWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.ShiftCode == timeManagement.ShiftCode);

            timeManagement.NormalTimeIn = tm.NormalTimeIn;
            timeManagement.NormalTimeOut = tm.NormalTimeOut;

            if (shift != null)
            {
                var date = tm.WorkingDate.Date;
                DateTime? normalTimeIn = null;
                DateTime? normalTimeOut = null;

                if (shift.NormalTimeIN.HasValue && shift.NormalTimeOut.HasValue)
                {
                    normalTimeIn = date.Add(shift.NormalTimeIN.Value);
                    normalTimeOut = date.Add(shift.NormalTimeOut.Value);

                    if (shift.NormalTimeOut < shift.NormalTimeIN)
                    {
                        normalTimeOut = normalTimeOut.Value.AddDays(1);
                    }
                }

                timeManagement.NormalTimeIn = normalTimeIn;
                timeManagement.NormalTimeOut = normalTimeOut;
            }

            UnitOfWork.Transact((trans) =>
            {
                AbnormalityAbsenceRepository.Upsert<Guid>(timeManagement, _properties);
                UnitOfWork.SaveChanges();
            });
        }


        /// <summary>
        /// Update or insert time management
        /// </summary>
        /// <param name="timeManagement">Time Management Object</param>
        public void UpsertTimeManagement(string actor, TimeManagement timeManagement)
        {
            var tm = TimeManagementRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == timeManagement.Id);

            var now = DateTime.Now;

            var shift = DailyWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.ShiftCode == timeManagement.ShiftCode);

            timeManagement.NormalTimeIn = tm.NormalTimeIn;
            timeManagement.NormalTimeOut = tm.NormalTimeOut;

            if (shift != null)
            {
                var date = tm.WorkingDate.Date;
                DateTime? normalTimeIn = null;
                DateTime? normalTimeOut = null;

                if (shift.NormalTimeIN.HasValue && shift.NormalTimeOut.HasValue)
                {
                    normalTimeIn = date.Add(shift.NormalTimeIN.Value);
                    normalTimeOut = date.Add(shift.NormalTimeOut.Value);

                    if (shift.NormalTimeOut < shift.NormalTimeIN)
                    {
                        normalTimeOut = normalTimeOut.Value.AddDays(1);
                    }
                }

                timeManagement.NormalTimeIn = normalTimeIn;
                timeManagement.NormalTimeOut = normalTimeOut;
            }

            UnitOfWork.Transact((trans) =>
            {
                TimeManagementRepository.Upsert<Guid>(timeManagement, _propertiesTimeManaGement);

                TimeManagementHistoryRepository.Add(TimeManagementHistory.CreateFrom(tm));

                TimeManagementHistoryRepository.Add(TimeManagementHistory.CreateFrom(timeManagement));

                var empWorkSubtitute = EmpWorkSchSubtituteRepository.Fetch().FirstOrDefault(x => x.NoReg == timeManagement.NoReg && x.Date == timeManagement.WorkingDate.Date);

                if (tm.ShiftCode != timeManagement.ShiftCode)
                {
                    if (empWorkSubtitute != null)
                    {
                        empWorkSubtitute.ShiftCodeUpdate = timeManagement.ShiftCode;
                        empWorkSubtitute.CreatedBy = actor;
                        empWorkSubtitute.CreatedOn = now;
                    }
                    else
                    {
                        EmpWorkSchSubtituteRepository.Add(EmpWorkSchSubtitute.CreateFrom(actor, now, tm.ShiftCode, timeManagement));
                    }
                }

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Soft delete Abnormallity Absence by id and its dependencies if any
        /// </summary>
        /// <param name="id">Abnormallity Absence Id</param>
        public void SoftDelete(Guid id)
        {
            var TimeManagement = AbnormalityAbsenceRepository.FindById(id);

            TimeManagement.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete Abnormallity Absence by id and its dependencies if any
        /// </summary>
        /// <param name="id">Abnormallity Absence Id</param>
        public void Delete(Guid id)
        {
            AbnormalityAbsenceRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete Abnormallity Absence history by id and its dependencies if any
        /// </summary>
        /// <param name="id">Abnormallity Absence History Id</param>
        public void DeleteHistory(Guid id)
        {
            TimeManagementHistoryRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        public void DeleteByDocumentApprovalId(DocumentApproval documentApproval)
        {
            AbnormalityAbsenceRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).Delete();

            UnitOfWork.SaveChanges();
        }
        public IEnumerable<EmployeeHRHierarchiesStoredEntity> GetHRHierarchies(DateTime KeyDate)
        {
            return UnitOfWork.UdfQuery<EmployeeHRHierarchiesStoredEntity>(new { keyDate = KeyDate });
        }
        #endregion
    }
}
