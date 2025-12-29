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
    public class AbnormalityOverTimeService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Document approval repository
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository { get { return UnitOfWork.GetRepository<DocumentApproval>(); } }

        /// <summary>
        /// Spkl request readonly repository
        /// </summary>
        protected IReadonlyRepository<AbnormalityOverTimeView> AbnormalityOverTimeViewRepository { get { return UnitOfWork.GetRepository<AbnormalityOverTimeView>(); } }

        /// <summary>
        /// AbnormalityOverTime readonly repository
        /// </summary>
        protected IRepository<AbnormalityOverTime> AbnormalityOverTimeRepository => UnitOfWork.GetRepository<AbnormalityOverTime>();

        /// <summary>
        /// Document request detail repository
        /// </summary>
        protected IRepository<DocumentRequestMemoOverTimeView> DocumentRequestMemoOverTimeViewRepository => UnitOfWork.GetRepository<DocumentRequestMemoOverTimeView>();

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

        protected IRepository<TimeManagement> TimeManagementRepository => UnitOfWork.GetRepository<TimeManagement>();

        protected IReadonlyRepository<SpklRequestDetailView> SpklRequestReadonlyRepository { get { return UnitOfWork.GetRepository<SpklRequestDetailView>(); } }

        #endregion

        #region Constructor
        private IStringLocalizer<IUnitOfWork> _localizer;
        public AbnormalityOverTimeService(IUnitOfWork unitOfWork, IStringLocalizer<IUnitOfWork> localizer)
            : base(unitOfWork)
        {
            _localizer = localizer;
        }
        #endregion

        private readonly string[] _properties = new[] {
            "OvertimeDate",
            "ProxyIn",
            "ProxyOut",
            "OvertimeIn",
            "OvertimeOut",
            "DocumentApprovalId",
            "Duration",
            "OvertimeBreak",
            "OvertimeReason",
            "OvertimeCategoryCode",
            "RowStatus"
        };

        /// <summary>
        /// Get Abnormallity OverTime query
        /// </summary>
        /// <returns>Abnormallity OverTime Query</returns>
        public IQueryable<AbnormalityOverTimeView> GetQuery()
        {
            return AbnormalityOverTimeViewRepository.Fetch()
                .AsNoTracking();
        }


        /// <summary>
        /// Data Abnormality Overtime By User
        /// </summary>
        /// <param name="noreg"></param>
        /// <returns></returns>
        public IQueryable<AbnormalityOverTimeView> GetsByCurrentUser(string noreg)
        {
            // Create config service object.
            var configService = new ConfigService(UnitOfWork);

            // Get start and end Date.
            var configStart = configService.GetConfig("Abnormality.StartDate");
            var configEnd = configService.GetConfig("Abnormality.EndDate");
            //var configMonth = configService.GetConfig("Abnormality.GetData.Month");

            var obj = DocumentRequestMemoOverTimeViewRepository.Fetch().Where(x => x.SubmitBy == noreg).Select(x => x.ObjectValue).ToList();
            List<AbnormalityOverTimeView> list = new List<AbnormalityOverTimeView>();
            foreach (var item in obj)
            {
                var result = JsonConvert.DeserializeObject<AbnormalityOverTimeViewModel>(item);
                list.AddRange(result.Details);
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

            return AbnormalityOverTimeViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && 
                x.OvertimeDate >= Convert.ToDateTime(configStart.ConfigValue) 
                && x.OvertimeDate <= Convert.ToDateTime(configEnd.ConfigValue) 
                && ((x.Status != "Completed" && x.Status != "Progress") || x.Status == null)
                ).OrderBy(x => x.OvertimeDate);
        }

        public Config GetDataConfig(string Catergory)
        {
            var configService = new ConfigService(UnitOfWork);
            return configService.GetConfig(Catergory);
        }

        // <summary>
        /// Data Abnormality Overtime By User
        /// </summary>
        /// <param name="noreg"></param>
        /// <returns></returns>
        public IQueryable<AbnormalityFileView> GetsAbnormalityFile(AbnormalityFileView entity)
        {
            return AbnormalityFileViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.TransactionId == entity.TransactionId);
        }

        /// <summary>
        /// Get Abnormallity OverTime by id
        /// </summary>
        /// <param name="id">Abnormallity OverTime Id</param>
        /// <returns>Abnormallity OverTime Object</returns>
        public AbnormalityOverTimeView Get(Guid id)
        {
            var result = GetQuery()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
            return result;
        }

        /// <summary>
        /// Update or insert time management
        /// </summary>
        /// <param name="timeManagement">Time Management Object</param>
        public void Upsert(string actor, AbnormalityOverTime overTime)
        {
            UnitOfWork.Transact((trans) =>
            {
                AbnormalityOverTimeRepository.Upsert<Guid>(overTime, _properties);
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
        /// Delete Abnormallity OverTime by id and its dependencies if any
        /// </summary>
        /// <param name="id">Abnormallity File Id</param>
        public void DeleteFile(Guid id)
        {
            var dataFile =AbnormalityFileRepository.FindById(id);
            UnitOfWork.Transact((trans) =>
            {
                AbnormalityFileRepository.DeleteById(id);
                CommonFileRepository.DeleteById(dataFile.CommonFileId);
                UnitOfWork.SaveChanges();
            });
        }

        public AbnormalityOverTime InsertAbnormalityOvertime(AbnormalityOverTimeView entity, string status)
        {
            var rs = new AbnormalityOverTime();
            //rs.Id = entity.AbnormalityOverTimeId.HasValue ? entity.AbnormalityOverTimeId.Value : Guid.Empty;
            var cekOverDatetime = entity.OvertimeDate.Date < entity.OvertimeIn.Date ? entity.OvertimeIn.Date : entity.OvertimeDate.Date; // handler if enable defaullt value 1/1/0001
            entity.OvertimeDate = cekOverDatetime;
            rs.OvertimeDate = entity.OvertimeDate;

            var wk = entity.OvertimeDate.ToShortDateString();

            var clkIn = entity.OvertimeIn.ToString("hh:mm tt");
            DateTime wrkIn = DateTime.Parse(wk + " " + clkIn);
            rs.OvertimeIn = wrkIn;

            var clkout = entity.OvertimeOut.ToString("hh:mm tt");
            DateTime wrkOut = DateTime.Parse(wk + " " + clkout);
            rs.OvertimeOut = wrkOut;

            var proxyIn = entity.ProxyIn.ToString("hh:mm tt");
            DateTime prxIn = DateTime.Parse(wk + " " + proxyIn);
            rs.ProxyIn = prxIn;

            var proxyOut = entity.OvertimeOut.ToString("hh:mm tt");
            DateTime prxOut = DateTime.Parse(wk + " " + proxyOut);
            rs.ProxyOut = prxOut;

            rs.DocumentApprovalId = entity.DocumentApprovalId;
            rs.TimeManagementSpklRequestId = entity.AbnormalityOverTimeId.HasValue ? Guid.Empty : entity.Id;
            rs.Status = status;
            rs.NoReg = entity.NoReg;
            rs.OvertimeInAdjust = entity.OvertimeInAdjust;
            rs.OvertimeOutAdjust = entity.OvertimeOutAdjust;
            rs.OvertimeBreak = entity.OvertimeBreak;
            rs.OvertimeBreakAdjust = entity.OvertimeBreakAdjust;
            rs.Duration = entity.Duration;
            rs.DurationAdjust = entity.DurationAdjust;
            rs.OvertimeCategoryCode = entity.OvertimeCategoryCode;
            rs.OvertimeReason = entity.OvertimeReason;
            rs.RowStatus = entity.RowStatus;
            rs.CreatedBy = entity.CreatedBy;
            rs.CreatedOn = entity.CreatedOn;

            AbnormalityOverTimeRepository.Add(rs);
            UnitOfWork.SaveChanges();

            return rs;
        }

        /// <summary>
        /// Complete Action Approval in Abnormality OverTime
        /// </summary>
        /// <param name="noregCurentApprover"></param>
        /// <param name="documentApproval"></param>
        public void CompleteApprove(string noregCurentApprover, DocumentApproval documentApproval)
        {
            // var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var result = JsonConvert.DeserializeObject<AbnormalityOverTimeViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                AbnormalityOverTime abnormality;

                //if (!item.AbnormalityOverTimeId.HasValue)
                //{
                //    InsertAbnormalityOvertime(item, "Completed");
                //} else
                //{
                    abnormality = AbnormalityOverTimeRepository.FindById(item.AbnormalityOverTimeId);
                    abnormality.Status = "Completed";
                    abnormality.Duration = item.Duration;
                    AbnormalityOverTimeRepository.Upsert<Guid>(abnormality, _properties);
                    UnitOfWork.SaveChanges();
                //}
            }
        }

        public void Submit(string noreg, DocumentApproval documentApproval)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var result = JsonConvert.DeserializeObject<AbnormalityOverTimeViewModel>(documentRequestDetail.ObjectValue);

            bool insertNew = false;
            foreach (var item in result.Details)
            {
                AbnormalityOverTime abnormality;

                if (!item.AbnormalityOverTimeId.HasValue)
                {
                    item.DocumentApprovalId = documentApproval.Id;
                    abnormality = InsertAbnormalityOvertime(item, "Progress");
                    //var abnormalityOverTime = AbnormalityOverTimeRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id && x.OvertimeDate == item.OvertimeDate).FirstOrDefault();
                    item.AbnormalityOverTimeId = abnormality.Id;
                    insertNew = true;
                }
                else
                {
                    abnormality = AbnormalityOverTimeRepository.FindById(item.AbnormalityOverTimeId);
                    abnormality.Status = "Progress";
                    abnormality.DocumentApprovalId = documentApproval.Id;
                    AbnormalityOverTimeRepository.Upsert<Guid>(abnormality, _properties);
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
            List<AbnormalityOverTimeView> list = new List<AbnormalityOverTimeView>();

            var result = JsonConvert.DeserializeObject<AbnormalityOverTimeViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                var abnormality = AbnormalityOverTimeRepository.FindById(item.AbnormalityOverTimeId);
                abnormality.Status = "Rejected";
                AbnormalityOverTimeRepository.Upsert<Guid>(abnormality, _properties);
                UnitOfWork.SaveChanges();
            }
        }

        public void Revise(string noreg, DocumentApproval documentApproval)
        {
            var documentId = documentApproval.Id;
            var documentNumber = documentApproval.DocumentNumber;

            // var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            List<AbnormalityOverTimeView> list = new List<AbnormalityOverTimeView>();

            var result = JsonConvert.DeserializeObject<AbnormalityOverTimeViewModel>(documentRequestDetail.ObjectValue);
            foreach (var item in result.Details)
            {
                var abnormality = AbnormalityOverTimeRepository.FindById(item.AbnormalityOverTimeId);
                abnormality.Status = "Revised";
                AbnormalityOverTimeRepository.Upsert<Guid>(abnormality, _properties);
                UnitOfWork.SaveChanges();
            }
        }

        public void Approve(string noreg, DocumentApproval documentApproval)
        {
            var documentId = documentApproval.Id;
            var documentNumber = documentApproval.DocumentNumber;

            var abnormalityList = AbnormalityOverTimeRepository.Find(x => x.DocumentApprovalId == documentId && x.Status != "Locked");

            var result = JsonConvert.DeserializeObject<AbnormalityOverTimeViewModel>(documentApproval.ObjDocumentRequestDetail.ObjectValue);

            JsonConvert.DeserializeObject(documentApproval.ObjDocumentRequestDetail.ObjectValue);

            foreach (var item in abnormalityList)
            {
                if (result.Details.Where(x => x.AbnormalityOverTimeId == item.Id).ToList().Count() == 0)
                {
                    item.Status = "Rejected";
                    AbnormalityOverTimeRepository.Upsert<Guid>(item, _properties);
                }
            }
            UnitOfWork.SaveChanges();
        }



        /// <summary>
        /// Soft delete Abnormallity OverTime by id and its dependencies if any
        /// </summary>
        /// <param name="id">Abnormallity OverTime Id</param>
        public void SoftDelete(Guid id)
        {
            var abnormality = AbnormalityOverTimeRepository.FindById(id);
            abnormality.RowStatus = false;
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete Abnormallity OverTime by id and its dependencies if any
        /// </summary>
        /// <param name="id">Abnormallity OverTime Id</param>
        public void Delete(Guid id)
        {
            AbnormalityOverTimeRepository.DeleteById(id);
        }

        public void DeleteByDocumentApprovalId(DocumentApproval documentApproval)
        {
            AbnormalityOverTimeRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).Delete();

            UnitOfWork.SaveChanges();
        }

        public TimeManagement GetNormalWorkSchedule(string noReg, DateTime workingDate)
        {
            return TimeManagementRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.NoReg == noReg && x.WorkingDate == workingDate);
        }

        public IEnumerable<EmployeeHRHierarchiesStoredEntity> GetHRHierarchies(DateTime KeyDate)
        {
            return UnitOfWork.UdfQuery<EmployeeHRHierarchiesStoredEntity>(new { keyDate = KeyDate });
        }

        public SpklRequestDetailView GetSpklRequestDetailById(Guid documentApprovalId)
        {
            //var spklOvertimeViewModel = new SpklOvertimeViewModel
            //{
            //    OvertimeDate = abnormalityOT.OvertimeDate
            //};

            return null;
        }
        public string GetDivisionByNoReg(string NoReg)
        {
            return UnitOfWork.GetConnection().Query<string>(@"
            select Divisi from vw_personal_data_information WHERE NoReg=@NoReg
            ", new { NoReg }).FirstOrDefault();
        }

        public string GetSectionByNoReg(string NoReg)
        {
            return UnitOfWork.GetConnection().Query<string>(@"
            select Seksi from vw_personal_data_information WHERE NoReg=@NoReg
            ", new { NoReg }).FirstOrDefault();
        }

        public string GetDepartmentByNoReg(string NoReg)
        {
            return UnitOfWork.GetConnection().Query<string>(@"
            select Departemen from vw_personal_data_information WHERE NoReg=@NoReg
            ", new { NoReg }).FirstOrDefault();
        }
    }
}
