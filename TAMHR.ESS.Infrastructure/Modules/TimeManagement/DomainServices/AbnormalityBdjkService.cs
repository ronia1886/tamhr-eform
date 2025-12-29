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
    /// SPKL Service Class
    /// </summary>
    public class AbnormalityBdjkService : GenericDomainServiceBase<AbnormalityBdjk>
    {
        protected override string[] Properties => new[] { "NoReg", "WorkingDate" };

        #region Repositories
        /// <summary>
        /// Document approval repository
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository { get { return UnitOfWork.GetRepository<DocumentApproval>(); } }

        /// <summary>
        /// Spkl request readonly repository
        /// </summary>
        protected IReadonlyRepository<AbnormalityBdjkView> AbnormalityBdjkViewRepository { get { return UnitOfWork.GetRepository<AbnormalityBdjkView>(); } }

        protected IReadonlyRepository<MasterDataBdjkView> MasterDataBdjkViewRepository { get { return UnitOfWork.GetRepository<MasterDataBdjkView>(); } }

        /// <summary>
        /// Daily work schedule readonly repository object.
        /// </summary>
        protected IReadonlyRepository<DailyWorkSchedule> DailyWorkScheduleReadonlyRepository => UnitOfWork.GetRepository<DailyWorkSchedule>();

        /// <summary>
        /// AbnormalityBdjk readonly repository
        /// </summary>
        protected IRepository<AbnormalityBdjk> AbnormalityBdjkRepository => UnitOfWork.GetRepository<AbnormalityBdjk>();

        /// <summary>
        /// Document request detail repository
        /// </summary>
        protected IRepository<DocumentRequestMemoBdjkView> DocumentRequestMemoBdjkViewRepository => UnitOfWork.GetRepository<DocumentRequestMemoBdjkView>();

        /// <summary>
        /// DocumentRequestDetail repository object.
        /// </summary>
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();


        /// <summary>
        /// AbnormalityFileRepository repository object.
        /// </summary>
        protected IRepository<AbnormalityFile> AbnormalityFileRepository => UnitOfWork.GetRepository<AbnormalityFile>();

        /// <summary>
        /// AbnormalityFileViewRepository repository object.
        /// </summary>
        protected IRepository<AbnormalityFileView> AbnormalityFileViewRepository => UnitOfWork.GetRepository<AbnormalityFileView>();

        /// <summary>
        /// CommonFileRepository repository object.
        /// </summary>
        protected IRepository<CommonFile> CommonFileRepository => UnitOfWork.GetRepository<CommonFile>();


        protected IReadonlyRepository<EventsCalendar> EventsCalendarRepository => UnitOfWork.GetRepository<EventsCalendar>();
        protected IRepository<TimeManagement> TimeManagementRepository => UnitOfWork.GetRepository<TimeManagement>();
        protected IRepository<TrackingApproval> TrackingApprovalRepository => UnitOfWork.GetRepository<TrackingApproval>();
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();
        protected IRepository<UserRole> UserRoleRepository => UnitOfWork.GetRepository<UserRole>();
        #endregion

        #region Constructor
        private IStringLocalizer<IUnitOfWork> _localizer;
        public AbnormalityBdjkService(IUnitOfWork unitOfWork, IStringLocalizer<IUnitOfWork> localizer)
            : base(unitOfWork)
        {
            _localizer = localizer;
        }
        #endregion

        private readonly string[] _properties = new[] {
            "WorkingDate",
            "WorkingTimeIn",
            "WorkingTimeOut",
            "BDJKCode",
            "Taxi",
            "UangMakanDinas",
            "ActivityCode",
            "TimeManagementBdjkId",
            "DocumentApprovalId",
            "NoReg",
            "Status",
            "BDJKReason"
         };

        public DateTime WorkingDate { get; set; }
        public DateTime WorkingTimeIn { get; set; }
        public DateTime WorkingTimeOut { get; set; }
        public string BdjkCode { get; set; }
        public bool Taxi { get; set; }
        public bool UangMakanDinas { get; set; }
        /// <summary>
        /// Get Abnormallity Absence query
        /// </summary>
        /// <returns>Abnormallity Absence Query</returns>
        public IQueryable<AbnormalityBdjk> GetQuery()
        {
            return AbnormalityBdjkRepository.Fetch()
                .AsNoTracking();
        }

        public IQueryable<AbnormalityBdjkView> GetQueryView()
        {
            return AbnormalityBdjkViewRepository.Fetch()
                .AsNoTracking();
        }

        public IEnumerable<AbnormalityBdjkView> GetBdjkRequestDetailsByUser(string noreg)
        {
            // return ServiceProxy.GetTableValuedDataSourceResult<BdjkRequestDetailStoredEntity>(new { noreg, username, orgCode, orgLevel });
            var configService = new ConfigService(UnitOfWork);

            // Get start and end Date.
            var configStart = configService.GetConfig("Abnormality.StartDate");
            var configEnd = configService.GetConfig("Abnormality.EndDate");
            //var configMonth = configService.GetConfig("Abnormality.GetData.Month");

            var obj = DocumentRequestMemoBdjkViewRepository.Fetch().Where(x => x.SubmitBy == noreg).Select(x => x.ObjectValue).ToList();
            List<AbnormalityBdjkView> list = new List<AbnormalityBdjkView>();
            foreach (var item in obj)
            {
                var result = JsonConvert.DeserializeObject<AbnormalityBdjkViewModel>(item);
                list.AddRange((IEnumerable<AbnormalityBdjkView>)result.Details.ToList());
            }

            //int year = DateTime.Now.Year;
            //int month = DateTime.Now.Month - Convert.ToInt32(configMonth.ConfigValue);
            //if (month < 1)
            //{
            //    month += 12;
            //    year -= 1;
            //}
            //
            //DateTime firstMonthDate = new DateTime(year, month, 1);

            return AbnormalityBdjkViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && 
                x.WorkingDate >= Convert.ToDateTime(configStart.ConfigValue) 
                && x.WorkingDate <= Convert.ToDateTime(configEnd.ConfigValue) 
                && x.WorkingTimeOut.HasValue 
                && ((x.Status != "Completed" && x.Status != "Progress") || x.Status == null)
                ).OrderBy(x => x.WorkingDate);
            //return UnitOfWork.UdfQuery<AbnormalityBdjkView>(new { noreg, username, orgCode, orgLevel });
        }

        // <summary>
        /// Data Abnormality Bdjk By User
        /// </summary>
        /// <param name="noreg"></param>
        /// <returns></returns>
        public IQueryable<AbnormalityFileView> GetsAbnormalityFile(AbnormalityFileView entity)
        {
            return AbnormalityFileViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.TransactionId == entity.TransactionId);
        }

        // <summary>
        /// Master Data BDJK
        /// </summary>
        /// <param name="noreg"></param>
        /// <returns></returns>
        public IQueryable<MasterDataBdjkView> GetsMasterDataBdjkView()
        {
            return MasterDataBdjkViewRepository.Fetch().Where(x => x.DocumentStatusCode == "Completed")
                .AsNoTracking();
        }

        /// <summary>
        /// Get Abnormallity Absence by id
        /// </summary>
        /// <param name="id">Abnormallity Absence Id</param>
        /// <returns>Abnormallity Absence Object</returns>
        public AbnormalityBdjk Get(Guid id)
        {
            var result = GetQuery()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
            return result;
        }

        public DailyWorkSchedule GetBySfitCode(string shiftCode)
        {
            return DailyWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking().Where(x => x.ShiftCode == shiftCode).FirstOrDefault();
        }
        
        /// <summary>
        /// Update or insert time management
        /// </summary>
        /// <param name="timeManagement">Time Management Object</param>
        public void Upsert(string actor, AbnormalityBdjk Bdjk)
        {
            UnitOfWork.Transact((trans) =>
            {
                AbnormalityBdjkRepository.Upsert<Guid>(Bdjk, _properties);
                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Update or insert time management
        /// </summary>
        /// <param name="timeManagement">Time Management Object</param>
        public void AddFile(string actor, AbnormalityFile dataFile)
        {
            UnitOfWork.Transact((trans) =>
            {
                AbnormalityFileRepository.Add(dataFile);
                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Delete Abnormallity Absence by id and its dependencies if any
        /// </summary>
        /// <param name="id">Abnormallity File Id</param>
        public void DeleteFile(Guid id)
        {
            var dataFile = AbnormalityFileRepository.FindById(id);
            UnitOfWork.Transact((trans) =>
            {
                AbnormalityFileRepository.DeleteById(id);
                CommonFileRepository.DeleteById(dataFile.CommonFileId);
                UnitOfWork.SaveChanges();
            });
        }

        public AbnormalityBdjk InsertAbnormalityBdjk(AbnormalityBdjkView entity, string status)
        {
            var rs = new AbnormalityBdjk();
            rs.WorkingDate = entity.WorkingDate;
            var wk = entity.WorkingDate.ToShortDateString();
            var clkIn = entity.WorkingTimeIn.Value.ToString("hh:mm tt");
            DateTime wrkIn = DateTime.Parse(wk + " " + clkIn);
            rs.WorkingTimeIn = wrkIn;
            var clkout = entity.WorkingTimeOut.Value.ToString("hh:mm tt");
            DateTime wrkOut = DateTime.Parse(wk + " " + clkout);
            rs.WorkingTimeOut = wrkOut;
            rs.DocumentApprovalId = entity.DocumentApprovalId;
            rs.TimeManagementBdjkId = entity.AbnormalityBdjkId.HasValue ? Guid.Empty : entity.Id;
            rs.Status = status;
            rs.NoReg = entity.NoReg;

            rs.ActivityCode = entity.ActivityCode;
            rs.BDJKCode = entity.BdjkCode;
            rs.BDJKDuration = entity.Duration;
            rs.BDJKReason = entity.BdjkReason;

            rs.Taxi = entity.Taxi;
            rs.UangMakanDinas = entity.UangMakanDinas;

            AbnormalityBdjkRepository.Add(rs);
            UnitOfWork.SaveChanges();

            return rs;
        }

        public void CompleteApprove(string noregCurentApprover, DocumentApproval documentApproval)
        {
            // var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var result = JsonConvert.DeserializeObject<AbnormalityBdjkViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                AbnormalityBdjk abnormality;

                abnormality = AbnormalityBdjkRepository.FindById(item.AbnormalityBdjkId);
                abnormality.Status = "Completed";
                AbnormalityBdjkRepository.Upsert<Guid>(abnormality, _properties);
                UnitOfWork.SaveChanges();
            }
        }

        public void Submit(string noreg, DocumentApproval documentApproval)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var result = JsonConvert.DeserializeObject<AbnormalityBdjkViewModel>(documentRequestDetail.ObjectValue);

            bool insertNew = false;
            foreach (var item in result.Details)
            {
                AbnormalityBdjk abnormality;

                if (!item.AbnormalityBdjkId.HasValue)
                {
                    item.DocumentApprovalId = documentApproval.Id;
                    abnormality = InsertAbnormalityBdjk(item, "Progress");
                    //var abnormalityBdjk = AbnormalityBdjkRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id && x.BdjkDate == item.BdjkDate).FirstOrDefault();
                    item.AbnormalityBdjkId = abnormality.Id;
                    insertNew = true;
                }
                else
                {
                    abnormality = AbnormalityBdjkRepository.FindById(item.AbnormalityBdjkId);
                    abnormality.Status = "Progress";
                    AbnormalityBdjkRepository.Upsert<Guid>(abnormality, _properties);
                    UnitOfWork.SaveChanges();
                }
            }

            if (insertNew)
            {
                DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id)
                    .Update(x => new DocumentRequestDetail { ObjectValue = JsonConvert.SerializeObject(result) });
                UnitOfWork.SaveChanges();
            }
        }
        public void Reject(string noreg, DocumentApproval documentApproval)
        {
            var documentId = documentApproval.Id;
            var documentNumber = documentApproval.DocumentNumber;

            // var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            List<AbnormalityBdjkViewModel> list = new List<AbnormalityBdjkViewModel>();

            var result = JsonConvert.DeserializeObject<AbnormalityBdjkViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                var abnormality = AbnormalityBdjkRepository.FindById(item.AbnormalityBdjkId);
                abnormality.Status = "Rejected";
                AbnormalityBdjkRepository.Upsert<Guid>(abnormality, _properties);
                UnitOfWork.SaveChanges();
            }
        }

        public void Approve(string noreg, DocumentApproval documentApproval)
        {
            var documentId = documentApproval.Id;
            var documentNumber = documentApproval.DocumentNumber;

            var abnormalityList = AbnormalityBdjkRepository.Find(x => x.DocumentApprovalId == documentId && x.Status != "Locked");

            var result = JsonConvert.DeserializeObject<AbnormalityBdjkViewModel>(documentApproval.ObjDocumentRequestDetail.ObjectValue);

            JsonConvert.DeserializeObject(documentApproval.ObjDocumentRequestDetail.ObjectValue);

            foreach (var item in abnormalityList)
            {
                if (result.Details.Where(x => x.AbnormalityBdjkId == item.Id).ToList().Count() == 0)
                {
                    item.Status = "Rejected";
                    AbnormalityBdjkRepository.Upsert<Guid>(item, _properties);
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
            List<AbnormalityBdjkViewModel> list = new List<AbnormalityBdjkViewModel>();

            var result = JsonConvert.DeserializeObject<AbnormalityBdjkViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                var abnormality = AbnormalityBdjkRepository.FindById(item.AbnormalityBdjkId);
                abnormality.Status = "Revised";
                AbnormalityBdjkRepository.Upsert<Guid>(abnormality, _properties);
                UnitOfWork.SaveChanges();
            }
        }

        /// <summary>
        /// Soft delete Abnormallity Absence by id and its dependencies if any
        /// </summary>
        /// <param name="id">Abnormallity Absence Id</param>
        public void SoftDelete(Guid id)
        {
            var abnormality = AbnormalityBdjkRepository.FindById(id);
            abnormality.RowStatus = false;
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete Abnormallity Absence by id and its dependencies if any
        /// </summary>
        /// <param name="id">Abnormallity Absence Id</param>
        public void Delete(Guid id)
        {
            AbnormalityBdjkRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }

        public bool isHoliday(DateTime date)
        {
            return EventsCalendarRepository.Fetch().Where(x => x.StartDate <= date && x.EndDate >= date).AsNoTracking().Count() > 0;
        }
        public void DeleteByDocumentApprovalId(DocumentApproval documentApproval)
        {
            AbnormalityBdjkRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).Delete();

            UnitOfWork.SaveChanges();
        }

        public AbnormalityBdjkView GetAbnormalityBdjkById(Guid Id)
        {
            return AbnormalityBdjkViewRepository.Fetch().Where(x => x.Id == Id).FirstOrDefault();
        }
        public IEnumerable<EmployeeHRHierarchiesStoredEntity> GetHRHierarchies(DateTime KeyDate)
        {
            return UnitOfWork.UdfQuery<EmployeeHRHierarchiesStoredEntity>(new { keyDate = KeyDate });
        }
        public TimeManagement GetNormalWorkSchedule(string noReg, DateTime workingDate)
        {
            return TimeManagementRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.NoReg == noReg && x.WorkingDate == workingDate);
        }
        public IEnumerable<TrackingApproval> GetTrackingApprovals(Guid documentApprovalId) => GetTrackingApprovalsQuery(documentApprovalId).ToList();

        private IQueryable<TrackingApproval> GetTrackingApprovalsQuery(Guid documentApprovalId)
        {
            return TrackingApprovalRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.DocumentApprovalId == documentApprovalId && x.RowStatus)
                .OrderBy(x => x.ApprovalLevel);
        }
        public IEnumerable<User> GetUsersByRole(string roleKey)
        {
            // Get list of users by role key.
            var users = (
                from u in UserRepository.Fetch().AsNoTracking()
                join r in UserRoleRepository.Fetch().AsNoTracking()
                on u.Id equals r.UserId
                where r.Role.RoleKey.Equals(roleKey)
                select r.User
            ).ToList();

            // Return the list.
            return users;
        }

        public IEnumerable<BdjkRequestStoredEntity> GetAbnormalityBdjkByDocApprovalId(Guid DocumentApprovalId)
        {
            List<BdjkRequestStoredEntity> result = new List<BdjkRequestStoredEntity>();

            List<AbnormalityBdjkView> bdjkList = AbnormalityBdjkViewRepository.Fetch().AsNoTracking().Where(x => x.DocumentApprovalId == DocumentApprovalId).OrderBy(x => x.WorkingDate).ToList();

            IEnumerable<TrackingApproval> apprv = GetTrackingApprovals(DocumentApprovalId);

            List<string> userList = GetUsersByRole("HR_PROXY_ABNORMALITY").Select(x => x.NoReg).ToList();

            //TrackingApproval lastApproval = apprv.Where(x => !userList.Contains(x.NoReg)).OrderByDescending(x => x.ModifiedOn).FirstOrDefault();

            TrackingApproval lastApproval = apprv.Where(x => x.ApprovalActionCode == "approve").OrderBy(x => x.ApprovalLevel).FirstOrDefault();

            foreach (AbnormalityBdjkView bdjk in bdjkList)
            {
                BdjkRequestStoredEntity temp = new BdjkRequestStoredEntity();
                temp.ActivityCode = bdjk.ActivityCode;
                temp.BdjkCode = bdjk.BdjkCode;
                temp.Taxi = bdjk.Taxi;
                temp.UangMakanDinas = bdjk.UangMakanDinas;
                temp.WorkingDate = bdjk.WorkingDate;
                temp.ProxyIn = bdjk.WorkingTimeIn;
                temp.ProxyOut = bdjk.WorkingTimeOut;
                temp.Overtime = (int)bdjk.Duration;
                temp.ActivityName = bdjk.ActivityName;
                temp.DocumentStatusCode = bdjk.DocumentStatusCode;

                temp.LastApproverName = lastApproval.Name;

                result.Add(temp);
            }
            
            return result;
        }
    }
}
