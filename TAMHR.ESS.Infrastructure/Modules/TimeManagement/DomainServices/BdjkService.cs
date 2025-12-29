using Agit.Common;
using Agit.Common.Extensions;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class BdjkService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Document approval repository
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository { get { return UnitOfWork.GetRepository<DocumentApproval>(); } }

        /// <summary>
        /// BDJK request repository
        /// </summary>
        protected IRepository<BdjkRequest> BdjkRequestRepository { get { return UnitOfWork.GetRepository<BdjkRequest>(); } }

        /// <summary>
        /// Time management readonly repository
        /// </summary>
        protected IReadonlyRepository<TimeManagement> TimeManagementReadonlyRepository { get { return UnitOfWork.GetRepository<TimeManagement>(); } }

        /// <summary>
        /// BDJK master repository
        /// </summary>
        protected IRepository<BDJK> BdjkRepository { get { return UnitOfWork.GetRepository<BDJK>(); } }

        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();

        #endregion

        #region Constructor
        public BdjkService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public IQueryable<BdjkRequest> GetBdjkRequests()
        {
            return BdjkRequestRepository.Fetch()
                .AsNoTracking();
        }
        public IQueryable<BdjkRequest> GetBdjkRequests(Guid documentApprovalId)
        {
            return GetBdjkRequests().Where(x => x.DocumentApprovalId == documentApprovalId);
        }

        public BdjkRequest GetBdjkRequest(Guid id)
        {
            return GetBdjkRequests().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public IEnumerable<BdjkRequestStoredEntity> GetBdjkList(int year, int month, string noreg, Guid documentApprovalId)
        {
            return UnitOfWork.UspQuery<BdjkRequestStoredEntity>(new { year, month, noreg, documentApprovalId });
        }

        public DataSourceResult GetBdjkReportDetails(DataSourceRequest request, string noreg, string username, string orgCode, int orgLevel)
        {
            var data = UnitOfWork.UdfQuery<BdjkRequestDetailReportStoredEntity>(new { noreg, username, orgCode, orgLevel });
            return data.ToDataSourceResult(request);
        }

        /// <summary>
        /// Update bdjk by document approval id and temporary id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        public void UpsertBdjkRequest(Guid documentApprovalId, string noreg, DateTime period, IEnumerable<BdjkRequestViewModel> data)
        {
            Assert.ThrowIf(data.Any(x => x.WorkingDate.Month != period.Month || x.WorkingDate.Year != period.Year), string.Format("Cannot process, there is a date that not in period {0:MMMM yyyy}", period));
            Assert.ThrowIf(data.GroupBy(x => x.WorkingDate).Any(x => x.Count() > 1), "Cannot process, there is multiple date in data request");

            var dt = data.Select(x => new
            {
                NoReg = noreg,
                x.WorkingDate,
                x.BdjkCode,
                x.UangMakanDinas,
                x.Taxi,
                x.ActivityCode,
                x.BdjkReason
            });

            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPSERT_BDJK_REQUEST", new { documentApprovalId, data = dt.ConvertToDataTable().AsTableValuedParameter("TVP_BDJK_REQUEST") }, trans);
            });
        }

        /// <summary>
        /// Complete document function
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="documentApproval">Document Approval</param>
        public void Complete(string noreg, DocumentApproval documentApproval)
        {
            var bdjkRequest = BdjkRequestRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.DocumentApprovalId == documentApproval.Id);
            var bdjkRequestMaster = BdjkRepository.Fetch().FirstOrDefault(x => x.NoReg == bdjkRequest.NoReg && x.BDJKDate.Date == bdjkRequest.WorkingDate.Date);
            var timeManagement = TimeManagementReadonlyRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.NoReg == bdjkRequest.NoReg && x.WorkingDate.Date == bdjkRequest.WorkingDate.Date);

            var list = new List<string>();

            if (bdjkRequest.UangMakanDinas)
            {
                list.Add("D");
            }

            if (bdjkRequest.Taxi)
            {
                list.Add("T");
            }

            var additionalBdjkCode = string.Join(",", list);

            if (bdjkRequestMaster == null)
            {
                BdjkRepository.Add(BDJK.CreateFrom(additionalBdjkCode, bdjkRequest, timeManagement));
            }
            else
            {
                bdjkRequestMaster.BDJKCode = bdjkRequest.BdjkCode;
                bdjkRequestMaster.BDJKInActual = timeManagement?.WorkingTimeIn;
                bdjkRequestMaster.BDJKOutActual = timeManagement?.WorkingTimeOut;
                bdjkRequestMaster.UangMakanDinas = bdjkRequest.UangMakanDinas;
                bdjkRequestMaster.Taxi = bdjkRequest.Taxi;
                bdjkRequestMaster.BDJKCodeAdditional = additionalBdjkCode;
                bdjkRequestMaster.Activity = bdjkRequest.ActivityCode;
                bdjkRequestMaster.Description = bdjkRequest.BdjkReason;
                bdjkRequestMaster.BDJKDuration = bdjkRequest.BdjkDuration;
            }

            UnitOfWork.SaveChanges();
        }

        public void UpdateBdjkDetail(BdjkRequestUpdateViewModel viewModel)
        {
            var bdjkRequest = BdjkRequestRepository.Fetch().FirstOrDefault(x => x.Id == viewModel.Id);

            bdjkRequest.WorkingDate = viewModel.WorkingDate;
            bdjkRequest.ActivityCode = viewModel.ActivityCode;
            bdjkRequest.BdjkReason = viewModel.BdjkReason;
            bdjkRequest.BdjkCode = viewModel.BdjkCode;
            bdjkRequest.Taxi = viewModel.Taxi;
            bdjkRequest.UangMakanDinas = viewModel.UangMakanDinas;

            var documentRequestDetail = DocumentRequestDetailRepository.Find(x => x.DocumentApproval.Equals(viewModel.ParentId)).FirstOrDefault();
            BdjkPlanningViewModel bdjkPlan = JsonConvert.DeserializeObject<BdjkPlanningViewModel>(documentRequestDetail.ObjectValue);

            BdjkRequestViewModel req = bdjkPlan.Details.Where(x => x.WorkingDate == viewModel.WorkingDate).FirstOrDefault();

            req.ActivityCode = viewModel.ActivityCode;
            req.BdjkReason = viewModel.BdjkReason;
            req.BdjkCode = viewModel.BdjkCode;
            req.Taxi = viewModel.Taxi;
            req.UangMakanDinas = viewModel.UangMakanDinas;

            documentRequestDetail.ObjectValue = JsonConvert.SerializeObject(bdjkPlan);

            UnitOfWork.SaveChanges();
        }

        public void Approve(string noreg, DocumentApproval documentApproval)
        {

        }
    }
}
