using Agit.Common;
using Agit.Common.Extensions;
using Agit.Common.Utility;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainEvents;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle approval
    /// </summary>
    public class ApprovalService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// User repository
        /// </summary>
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();

        /// <summary>
        /// User Role repository
        /// </summary>
        protected IRepository<UserRole> UserRoleRepository => UnitOfWork.GetRepository<UserRole>();


        /// <summary>
        /// Role repository
        /// </summary>
        protected IRepository<Role> RoleRepository => UnitOfWork.GetRepository<Role>();

        /// <summary>
        /// Document approval change tracking repository
        /// </summary>
        protected IRepository<DocumentApprovalChangeTracking> DocumentApprovalChangeTrackingRepository => UnitOfWork.GetRepository<DocumentApprovalChangeTracking>();

        /// <summary>
        /// Form sequence repository
        /// </summary>
        protected IRepository<FormSequence> FormSequenceRepository => UnitOfWork.GetRepository<FormSequence>();

        /// <summary>
        /// Common sequence repository
        /// </summary>
        protected IRepository<CommonSequence> CommonSequenceRepository => UnitOfWork.GetRepository<CommonSequence>();

        /// <summary>
        /// Document approval readonly repository
        /// </summary>
        protected IReadonlyRepository<DocumentApprovalView> DocumentApprovalReadonlyRepository => UnitOfWork.GetRepository<DocumentApprovalView>();

        protected IReadonlyRepository<HybridWorkPlanningView> HybridWorkPlanningRepository => UnitOfWork.GetRepository<HybridWorkPlanningView>();

        /// <summary>
        /// Notification repository
        /// </summary>
        protected IRepository<Notification> NotificationRepository => UnitOfWork.GetRepository<Notification>();

        /// <summary>
        /// Form repository
        /// </summary>
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();

        /// <summary>
        /// Document approval repository
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();

        /// <summary>
        /// Document approval participant repository
        /// </summary>
        protected IRepository<DocumentApprovalParticipant> DocumentApprovalParticipantRepository => UnitOfWork.GetRepository<DocumentApprovalParticipant>();

        /// <summary>
        /// Document approval convertation repository
        /// </summary>
        protected IRepository<DocumentApprovalConversation> DocumentApprovalConvertationRepository => UnitOfWork.GetRepository<DocumentApprovalConversation>();

        /// <summary>
        /// Document approval file repository
        /// </summary>
        protected IRepository<DocumentApprovalFile> DocumentApprovalFileRepository => UnitOfWork.GetRepository<DocumentApprovalFile>();

        /// <summary>
        /// Document Request Detail repository
        /// </summary>
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();

        /// <summary>
        /// Document approval history repository
        /// </summary>
        protected IRepository<DocumentApprovalHistory> DocumentApprovalHistoryRepository => UnitOfWork.GetRepository<DocumentApprovalHistory>();

        /// <summary>
        /// Tracking approval repository
        /// </summary>
        protected IRepository<TrackingApproval> TrackingApprovalRepository => UnitOfWork.GetRepository<TrackingApproval>();

        /// <summary>
        /// Common file repository
        /// </summary>
        protected IRepository<CommonFile> CommonFileRepository => UnitOfWork.GetRepository<CommonFile>();

        /// <summary>
        /// Config repository
        /// </summary>

        protected IRepository<Config> ConfigRepository => UnitOfWork.GetRepository<Config>();

        /// <summary>
        /// Approval matrix repository
        /// </summary>
        protected IRepository<ApprovalMatrix> ApprovalMatrixRepository => UnitOfWork.GetRepository<ApprovalMatrix>();

        /// <summary>
        /// Email template repository
        /// </summary>
        protected IRepository<EmailTemplate> EmailTemplateRepository => UnitOfWork.GetRepository<EmailTemplate>();

        /// <summary>
        ///  Actual reporting structure repository
        /// </summary>
        protected IReadonlyRepository<ActualReportingStructureView> ActualReportingStructureReadonlyRepository => UnitOfWork.GetRepository<ActualReportingStructureView>();

        #endregion

        private readonly IDictionary<string, Action<string, DocumentApproval>> _completeHandlers = new Dictionary<string, Action<string, DocumentApproval>>();
        private readonly IDictionary<string, Action<string, DocumentApproval>> _rejectHandlers = new Dictionary<string, Action<string, DocumentApproval>>();
        private readonly IDictionary<string, Action<string, DocumentApproval>> _reviseHandlers = new Dictionary<string, Action<string, DocumentApproval>>();
        private readonly IDictionary<string, Action<string, DocumentApproval>> _submitHandlers = new Dictionary<string, Action<string, DocumentApproval>>();
        private readonly IDictionary<string, Action<string, DocumentApproval>> _approveHandlers = new Dictionary<string, Action<string, DocumentApproval>>();
        private readonly IDictionary<string, Action<string, DocumentApproval>> _cancelHandlers = new Dictionary<string, Action<string, DocumentApproval>>();

        #region Domain Event Manager
        /// <summary>
        /// Domain event manager object
        /// </summary>
        protected readonly DomainEventManager DomainEventManager;
        #endregion

        private IStringLocalizer<IUnitOfWork> _localizer;

        private EventHandler<DocumentRequestDetailViewModel> documentCreated;
        public event EventHandler<DocumentRequestDetailViewModel> DocumentCreated
        {
            add { documentCreated += value; }
            remove { documentCreated -= value; }
        }

        private EventHandler<DocumentRequestDetailViewModel> documentUpdated;
        public event EventHandler<DocumentRequestDetailViewModel> DocumentUpdated
        {
            add { documentUpdated += value; }
            remove { documentUpdated -= value; }
        }

        #region Constructor
        /// <summary> 
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork">Unit of Work Object</param>
        public ApprovalService(IUnitOfWork unitOfWork, DomainEventManager domainEventManager, IStringLocalizer<IUnitOfWork> localizer)
            : base(unitOfWork)
        {
            if (localizer != null)
            {
                _localizer = localizer;
                DomainEventManager = domainEventManager;

                //personal data
                _completeHandlers.Add(ApplicationForm.MarriageStatus, (noreg, documentApproval) => new PersonalDataService(unitOfWork, localizer).CompleteMarriageStatusAsync(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.FamilyRegistration, (noreg, documentApproval) => new PersonalDataService(unitOfWork, localizer).CompleteBirthRegistrationAsync(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.Condolance, (noreg, documentApproval) => new PersonalDataService(unitOfWork, localizer).CompleteDismemberment(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.Divorce, (noreg, documentApproval) => new PersonalDataService(unitOfWork, localizer).CompleteDivorce(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.Address, (noreg, documentApproval) => new PersonalDataService(unitOfWork, localizer).CompleteAddress(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.BankAccount, (noreg, documentApproval) => new PersonalDataService(unitOfWork, localizer).CompleteBankAccount(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.TaxStatus, (noreg, documentApproval) => new PersonalDataService(unitOfWork, localizer).CompleteTaxStatus(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.Education, (noreg, documentApproval) => new PersonalDataService(unitOfWork, localizer).CompleteEducation(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.OthersPersonalData, (noreg, documentApproval) => new PersonalDataService(unitOfWork, localizer).CompleteOthersPersonalData(noreg, documentApproval));

                //claim benefit
                _completeHandlers.Add(ApplicationForm.EyeglassesAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteEyeglassesAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.MarriageAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteMarriageAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.CondolanceAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteMisseryAllowance(noreg, documentApproval));
                _approveHandlers.Add(ApplicationForm.VacationAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).ApprovalVacationAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.VacationAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteVacationAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.DistressedAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteDistressedAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.PtaAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompletePtaAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.ConceptIdeaAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteIdeBerkonsepAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.Reimbursement, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteRSAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.CopFuelAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteCopFuelAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.Cop, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteCOPAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.Scp, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteSCPAllowance(noreg, documentApproval));
                _reviseHandlers.Add(ApplicationForm.VacationAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).ResetClaimVacationAllowance(documentApproval));
                _cancelHandlers.Add(ApplicationForm.VacationAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).ResetClaimVacationAllowance(documentApproval));


                //pinjaman
                _completeHandlers.Add(ApplicationForm.CompanyLoan, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteLoanAllowance(noreg, documentApproval));
                _rejectHandlers.Add(ApplicationForm.CompanyLoan, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).RejectLoanAllowance(noreg, documentApproval));
                _reviseHandlers.Add(ApplicationForm.CompanyLoan, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).RejectLoanAllowance(noreg, documentApproval));

                _completeHandlers.Add("company-loan36", (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteLoanAllowance36(noreg, documentApproval));
                _rejectHandlers.Add("company-loan36", (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).RejectLoanAllowance36(noreg, documentApproval));
                _reviseHandlers.Add("company-loan36", (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).RejectLoanAllowance36(noreg, documentApproval));

                _completeHandlers.Add(ApplicationForm.DpaRegister, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteDpaRegister(noreg, documentApproval));

                // DPA change handlers.
                _completeHandlers.Add(ApplicationForm.ShiftMealAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteMealAllowanceShift(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.MealAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteMealAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.GetBpkbCop, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteBpkbRequest(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.ReturnBpkbCop, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteBpkbBorrow(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.AyoSekolah, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteAyoSekolah(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.KbAllowance, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteKBAllowance(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.LetterOfGuarantee, (noreg, documentApproval) => new ClaimBenefitService(unitOfWork, localizer).CompleteLetterGuarantee(noreg, documentApproval));

                // Time management handlers.
                _completeHandlers.Add(ApplicationForm.Absence, (noreg, documentApproval) => new TimeManagementService(unitOfWork, localizer).CompleteAbsence(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.MaternityLeave, (noreg, documentApproval) => new TimeManagementService(unitOfWork, localizer).CompleteMaternity(this, noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.ShiftPlanning, (noreg, documentApproval) => new TimeManagementService(unitOfWork, localizer).CompleteShiftPlanning(noreg, documentApproval));
                _approveHandlers.Add(ApplicationForm.BdjkPlanning, (noreg, documentApproval) => new BdjkService(unitOfWork).Approve(noreg, documentApproval));
                _approveHandlers.Add(ApplicationForm.BdjkReport, (noreg, documentApproval) => new BdjkService(unitOfWork).Approve(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.BdjkPlanning, (noreg, documentApproval) => new BdjkService(unitOfWork).Complete(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.BdjkReport, (noreg, documentApproval) => new BdjkService(unitOfWork).Complete(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.SpklOvertime, (noreg, documentApproval) => new SpklService(unitOfWork).Complete(noreg, documentApproval));

                // Others handlers.
                _completeHandlers.Add(ApplicationForm.HealthDeclaration, (noreg, documentApproval) => new OthersService(unitOfWork).CompleteHealthDeclaration(noreg, documentApproval));

                // Termination handlers.
                _approveHandlers.Add(ApplicationForm.Termination, (noreg, documentApproval) => new TerminationService(unitOfWork).Approve(noreg, documentApproval));
                _submitHandlers.Add(ApplicationForm.Termination, (noreg, documentApproval) => new TerminationService(unitOfWork).Submit(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.Termination, (noreg, documentApproval) => new TerminationService(unitOfWork).Complete(noreg, documentApproval));
                _rejectHandlers.Add(ApplicationForm.Termination, (noreg, documentApproval) => new TerminationService(unitOfWork).Reject(noreg, documentApproval));
                _reviseHandlers.Add(ApplicationForm.Termination, (noreg, documentApproval) => new TerminationService(unitOfWork).Revise(noreg, documentApproval));
                ///_completeHandlers.Add(ApplicationForm.Vaccine, (noreg, documentApproval) => new OthersService(unitOfWork).CompleteVaccine(noreg, documentApproval));

                // Submit handlers.
                _submitHandlers.Add(ApplicationForm.SpklOvertime, (noreg, documentApproval) => new SpklService(unitOfWork).Submit(noreg, documentApproval));
                _approveHandlers.Add(ApplicationForm.SpklOvertime, (noreg, documentApproval) => new SpklService(unitOfWork).Approve(noreg, documentApproval));
                _submitHandlers.Add(ApplicationForm.HealthDeclaration, (noreg, documentApproval) => new OthersService(unitOfWork).SubmitHealthDeclaration(noreg, documentApproval));

                //Abnormality Handlers
                _submitHandlers.Add(ApplicationForm.AbnormalityAbsence, (noreg, documentApproval) => new AbnormalityAbsenceService(unitOfWork, localizer).Submit(noreg, documentApproval));
                _rejectHandlers.Add(ApplicationForm.AbnormalityAbsence, (noreg, documentApproval) => new AbnormalityAbsenceService(unitOfWork, localizer).Reject(noreg, documentApproval));
                _reviseHandlers.Add(ApplicationForm.AbnormalityAbsence, (noreg, documentApproval) => new AbnormalityAbsenceService(unitOfWork, localizer).Revise(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.AbnormalityAbsence, (noreg, documentApproval) => new AbnormalityAbsenceService(unitOfWork, localizer).CompleteApprove(noreg, documentApproval));
                _cancelHandlers.Add(ApplicationForm.AbnormalityAbsence, (noreg, documentApproval) => new AbnormalityAbsenceService(unitOfWork, localizer).DeleteByDocumentApprovalId(documentApproval));
                _approveHandlers.Add(ApplicationForm.AbnormalityAbsence, (noreg, documentApproval) => new AbnormalityAbsenceService(unitOfWork, localizer).Approve(noreg, documentApproval));

                _submitHandlers.Add(ApplicationForm.AbnormalityOverTime, (noreg, documentApproval) => new AbnormalityOverTimeService(unitOfWork, localizer).Submit(noreg, documentApproval));
                _rejectHandlers.Add(ApplicationForm.AbnormalityOverTime, (noreg, documentApproval) => new AbnormalityOverTimeService(unitOfWork, localizer).Reject(noreg, documentApproval));
                _reviseHandlers.Add(ApplicationForm.AbnormalityOverTime, (noreg, documentApproval) => new AbnormalityOverTimeService(unitOfWork, localizer).Revise(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.AbnormalityOverTime, (noreg, documentApproval) => new AbnormalityOverTimeService(unitOfWork, localizer).CompleteApprove(noreg, documentApproval));
                _cancelHandlers.Add(ApplicationForm.AbnormalityOverTime, (noreg, documentApproval) => new AbnormalityOverTimeService(unitOfWork, localizer).DeleteByDocumentApprovalId(documentApproval));
                _approveHandlers.Add(ApplicationForm.AbnormalityOverTime, (noreg, documentApproval) => new AbnormalityOverTimeService(unitOfWork, localizer).Approve(noreg, documentApproval));

                _submitHandlers.Add(ApplicationForm.AbnormalityBdjk, (noreg, documentApproval) => new AbnormalityBdjkService(unitOfWork, localizer).Submit(noreg, documentApproval));
                _rejectHandlers.Add(ApplicationForm.AbnormalityBdjk, (noreg, documentApproval) => new AbnormalityBdjkService(unitOfWork, localizer).Reject(noreg, documentApproval));
                _reviseHandlers.Add(ApplicationForm.AbnormalityBdjk, (noreg, documentApproval) => new AbnormalityBdjkService(unitOfWork, localizer).Revise(noreg, documentApproval));
                _completeHandlers.Add(ApplicationForm.AbnormalityBdjk, (noreg, documentApproval) => new AbnormalityBdjkService(unitOfWork, localizer).CompleteApprove(noreg, documentApproval));
                _cancelHandlers.Add(ApplicationForm.AbnormalityBdjk, (noreg, documentApproval) => new AbnormalityBdjkService(unitOfWork, localizer).DeleteByDocumentApprovalId(documentApproval));
                _approveHandlers.Add(ApplicationForm.AbnormalityBdjk, (noreg, documentApproval) => new AbnormalityBdjkService(unitOfWork, localizer).Approve(noreg, documentApproval));

            }
        }
        #endregion

        #region Tracking Approval Area
        public IEnumerable<User> GetTrackingApprovalUsers(Guid documentApprovalId, string noreg)
        {
            var users = GetTrackingApprovalsQuery(documentApprovalId)
                .Join(UserRepository.Fetch(), approval => approval.NoReg, user => user.NoReg, (approval, user) => user)
                .Where(x => x.NoReg != noreg)
                .ToList();

            return users;
        }

        public IEnumerable<User> GetDistinctTrackingApprovalUsers(Guid documentApprovalId, string noreg)
        {
            return GetTrackingApprovalUsers(documentApprovalId, noreg).Distinct();
        }

        /// <summary>
        /// Get tracking approval query by id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>Tracking Approval Query</returns>
        private IQueryable<TrackingApproval> GetTrackingApprovalsQuery(Guid documentApprovalId)
        {
            return TrackingApprovalRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.DocumentApprovalId == documentApprovalId && x.RowStatus)
                .OrderBy(x => x.ApprovalLevel);
        }

        /// <summary>
        /// Get list of tracking approvals by document approval id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>List of Tracking Approvals</returns>
        public IEnumerable<TrackingApproval> GetTrackingApprovals(Guid documentApprovalId) => GetTrackingApprovalsQuery(documentApprovalId).ToList();

        /// <summary>
        /// Get list of tracking approvals by document approval id async
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>List of Tracking Approvals</returns>
        public Task<List<TrackingApproval>> GetTrackingApprovalsAsync(Guid documentApprovalId) => GetTrackingApprovalsQuery(documentApprovalId).OrderBy(x => x.ApprovalLevel).ToListAsync();

        /// <summary>
        /// Get list of tracking approvals by document approval id
        /// </summary>
        /// <param name="documentApprovalId">Documen Approval Id</param>
        /// <returns>List of Tracking Approvals</returns>
        public async Task<TrackingApprovalViewModel> GetTrackingApprovalViewModel(Guid documentApprovalId)
        {
            var documentApproval = await DocumentApprovalRepository.FindByIdAsync(documentApprovalId).ConfigureAwait(false);
            var trackingApprovals = await GetTrackingApprovalsAsync(documentApprovalId).ConfigureAwait(false);
            var progress = documentApproval.Progress;
            var documentStatusCode = documentApproval.DocumentStatusCode;

            var viewModel = new TrackingApprovalViewModel(progress, documentStatusCode, trackingApprovals);

            return viewModel;
        }
        #endregion

        #region Document Approval Area
        /// <summary>
        /// Check whether document approval has approval matrix or not
        /// </summary>
        /// <param name="id">Document Approval Id</param>
        /// <returns>True if document approval has approval matrix, false otherwise</returns>
        public bool HasApprovalMatrix(Guid id)
        {
            var form = DocumentApprovalRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => x.Form)
                .FirstOrDefault();

            if (form == null) return false;

            if (!form.NeedApproval) return true;

            return ApprovalMatrixRepository.Fetch().Any(x => x.FormId == form.Id);
        }

        /// <summary>
        /// Get list of child request documents by parent document approval
        /// </summary>
        /// <param name="document">Parent Document Approval Object</param>
        /// <returns>List of Child Request Documents</returns>
        public IEnumerable<DocumentApproval> GetChildRequestDocuments(DocumentApproval document)
        {
            if (document.ParentId == null)
            {
                return DocumentApprovalRepository.Fetch()
                    .AsNoTracking()
                    .Where(x => x.ParentId == document.Id && x.VisibleInHistory && x.RowStatus)
                    .Include(x => x.Form);

            }
            else
            {
                return DocumentApprovalRepository.Fetch()
                    .AsNoTracking()
                    .Where(x => x.Id == document.ParentId && x.RowStatus)
                    .Include(x => x.Form);
            }

        }

        /// <summary>
        /// Get list of chlid request documents by document parent id
        /// </summary>
        /// <param name="parentId">Document Approval Parent Id</param>
        /// <returns>List of Child Request Documents</returns>
        public IEnumerable<DocumentApproval> GetChildRequestDocuments(Guid parentId)
        {
            return DocumentApprovalRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.ParentId == parentId)
                .Include(x => x.Form);
        }

        /// <summary>
        /// Get list of complete request
        /// </summary>
        /// <param name="formkey">Form Key</param>
        /// <returns>List of Request</returns>
        public IEnumerable<DocumentApproval> GetCompleteRequest(string formkey, string noreg, DateTime startDate, DateTime endDate)
        {
            var data = from da in DocumentApprovalRepository.Fetch()
                       join dd in DocumentRequestDetailRepository.Fetch() on da.Id equals dd.DocumentApprovalId
                       join f in FormRepository.Fetch() on da.FormId equals f.Id
                       where da.DocumentStatusCode == DocumentStatus.Completed && f.FormKey == formkey && da.SubmitBy == noreg && da.SubmitOn >= startDate && da.SubmitOn <= endDate
                       select da;

            return data;
        }

        /// <summary>
        /// Get list of complete request details
        /// </summary>
        /// <param name="formkey">Form Key</param>
        /// <returns>List of Request Details</returns>
        public IEnumerable<DocumentRequestDetail> GetCompleteRequestDetails(string formkey)
        {
            var data = from da in DocumentApprovalRepository.Fetch()
                       join dd in DocumentRequestDetailRepository.Fetch() on da.Id equals dd.DocumentApprovalId
                       join f in FormRepository.Fetch() on da.FormId equals f.Id
                       where da.DocumentStatusCode == DocumentStatus.Completed && f.FormKey == formkey
                       select dd;

            return data;
        }

        /// <summary>
        /// Get list of complete request details
        /// </summary>
        /// <param name="formkey">Form Key</param>
        /// <returns>List of Request Details</returns>
        public IEnumerable<DocumentRequestDetail> GetInprogressRequestDetails(string formkey)
        {
            var data = from da in DocumentApprovalRepository.Fetch()
                       join dd in DocumentRequestDetailRepository.Fetch() on da.Id equals dd.DocumentApprovalId
                       join f in FormRepository.Fetch() on da.FormId equals f.Id
                       where da.DocumentStatusCode == DocumentStatus.InProgress && f.FormKey == formkey
                       select dd;

            return data;
        }

        /// <summary>
        /// Get list of complete request details
        /// </summary>
        /// <param name="formkey">Form Key</param>
        /// <returns>List of Request Details</returns>
        public IEnumerable<DocumentRequestDetail> GetInprogressDraftRequestDetails(string formkey)
        {
            var data = from da in DocumentApprovalRepository.Fetch()
                       join dd in DocumentRequestDetailRepository.Fetch() on da.Id equals dd.DocumentApprovalId
                       join f in FormRepository.Fetch() on da.FormId equals f.Id
                       where (da.DocumentStatusCode == DocumentStatus.InProgress) && f.FormKey == formkey
                       select dd;

            return data;
        }

        /// <summary>
        /// Create common sequence by key
        /// </summary>
        /// <param name="key">Common Key</param>
        /// <returns>Generated Sequence</returns>
        public int CreateCommonSequence(string key)
        {
            var generatedSequenceNumber = 1;

            UnitOfWork.Transact(trans =>
            {
                var commonSequence = CommonSequenceRepository.Fetch()
                    .Where(x => x.SequenceKey == key)
                    .OrderByDescending(x => x.SequenceNumber)
                    .FirstOrDefault();

                if (commonSequence != null)
                {
                    generatedSequenceNumber = ++commonSequence.SequenceNumber;
                }
                else
                {
                    CommonSequenceRepository.Add(CommonSequence.Create(key, 1));
                }

                UnitOfWork.SaveChanges();
            });

            return generatedSequenceNumber;
        }

        /// <summary>
        /// Get list of document approval histories by document approval id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>List of Document Approval Histories</returns>
        public IQueryable<DocumentApprovalHistory> GetDocumentApprovalHistoriesQuery(Guid documentApprovalId)
        {
            return DocumentApprovalHistoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.DocumentApprovalId == documentApprovalId && x.RowStatus);
        }

        /// <summary>
        /// Get list of document approval histories by document approval id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>List of Document Approval Histories</returns>
        public IEnumerable<DocumentApprovalHistory> GetDocumentApprovalHistories(Guid documentApprovalId)
        {
            return GetDocumentApprovalHistoriesQuery(documentApprovalId);
        }

        /// <summary>
        /// Get list of document approval histories by document approval id async
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>List of Document Approval Histories</returns>
        public Task<List<DocumentApprovalHistory>> GetDocumentApprovalHistoriesAsync(Guid documentApprovalId)
        {
            return GetDocumentApprovalHistoriesQuery(documentApprovalId).ToListAsync();
        }

        /// <summary>
        /// Create approval history detail
        /// </summary>
        /// <param name="approvalHistoryDetail">Approval History Detail Object</param>
        public void CreateApprovalHistoryDetail(DocumentApprovalHistory approvalHistoryDetail)
        {
            DocumentApprovalHistoryRepository.Add(approvalHistoryDetail);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Get document approvals by document approval id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>Document Approvals</returns>
        public DocumentApproval GetDocumentApprovalById(Guid documentApprovalId)
        {
            return DocumentApprovalRepository.Fetch()
                .Where(x => x.Id == documentApprovalId)
                .Include(x => x.Form)
                .FirstOrDefault();
        }
        public DocumentRequestDetail GetDocumentDetailApprovalById(Guid documentApprovalId)
        {
            return DocumentRequestDetailRepository.Fetch()
                .Where(x => x.DocumentApprovalId == documentApprovalId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Generate permission for all forms by noreg and document approval id
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>List of Permissions</returns>
        public IEnumerable<string> GeneratePermissions(string noreg, Guid documentApprovalId, bool canViewAllDocumentApprovals = false)
        {
            var list = new List<string>();
            var user = UserRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.NoReg == noreg);

            var documentApproval = DocumentApprovalRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == documentApprovalId);

            var isInTrackingApproval = TrackingApprovalRepository.Fetch()
                .AsNoTracking()
                .Any(x => x.DocumentApprovalId == documentApprovalId && x.NoReg == noreg);

            var isInApprovalHistory = DocumentApprovalHistoryRepository.Fetch()
                .AsNoTracking()
                .Any(x => x.DocumentApprovalId == documentApprovalId && x.NoReg == noreg);

            var form = FormRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == documentApproval.FormId);

            var statusCode = documentApproval.DocumentStatusCode;
            var isOwner = documentApproval.CreatedBy == noreg;
            var currentApprover = documentApproval.CurrentApprover ?? string.Empty;
            var isCurrentApprover = currentApprover.Contains($"({user.Username})");
            var isCompleted = new[] { DocumentStatus.Cancelled, DocumentStatus.Completed, DocumentStatus.Rejected, DocumentStatus.Expired }.Contains(statusCode);

            if (form.FormKey == "termination")
            {
                var documentDetail = DocumentRequestDetailRepository.Fetch()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.DocumentApprovalId == documentApprovalId);

                var json = JsonConvert.DeserializeObject<Termination>(documentDetail.ObjectValue);

                var parent = ActualReportingStructureReadonlyRepository.Fetch()
                                    .AsNoTracking()
                                    .Where(x => x.NoReg == json.NoReg && x.ParentNoReg == noreg).ToList();

                if (parent.Count() > 0)
                {
                    list.Add("Core.Approval.View");
                }
            }

            if (form.FormKey == "weekly-wfh-planning")
            {
                var aos = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch().FirstOrDefault(x => x.NoReg == documentApproval.CreatedBy && x.Staffing == 100);

                var directSup = ActualReportingStructureReadonlyRepository.Fetch().FirstOrDefault(x => x.NoReg == aos.NoReg && x.PostCode == aos.PostCode && x.HierarchyLevel == 1);

                if (directSup != null && noreg == directSup.ParentNoReg)
                {
                    list.Add("Core.Approval.View");
                }
            }

            if (isOwner)
            {
                list.Add("Core.Approval.View");

                if (!isCompleted)
                {
                    list.Add("Core.Approval.Cancel");
                    list.Add("Core.Approval.ViewAction");
                }
            }

            if (isInApprovalHistory || canViewAllDocumentApprovals)
            {
                if (canViewAllDocumentApprovals)
                {
                    list.Add("Core.Approval.View");
                }

                list.Add("Core.Approval.ViewApprovalHistories");
                list.Add("Core.Approval.ViewConversation");
            }

            if (isInTrackingApproval)
            {
                if (statusCode != DocumentStatus.Draft)
                {
                    list.Add("Core.Approval.ViewApprovalHistories");
                }

                list.Add("Core.Approval.View");
                list.Add("Core.Approval.CreateConversation");
                list.Add("Core.Approval.ViewConversation");
            }

            if ((statusCode == DocumentStatus.Draft || (statusCode == DocumentStatus.Revised && isCurrentApprover)) && isOwner)
            {
                list.Add("Core.Approval.ViewAction");
                list.Add("Core.Approval.Edit");
                list.Add("Core.Approval.Submit");
            }
            else if (isCurrentApprover)
            {
                list.Add("Core.Approval.ViewAction");
                list.Add("Core.Approval.Approve");
                list.Add("Core.Approval.Reject");
                list.Add("Core.Approval.Revise");
            }

            return list;
        }

        /// <summary>
        /// Create approval document and detail
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="viewModel">Document Request Detail Object</param>
        public void CreateApprovalDocument(string noreg, DocumentRequestDetailViewModel viewModel, Func<string, Dictionary<string, object>, string> callback)
        {
            var user = UserRepository.Find(x => x.NoReg == noreg).FirstOrDefaultIfEmpty();

            Assert.ThrowIf(viewModel.DocumentApprovalId != default(Guid), "Cannot create because document approval already exist");
            var now = DateTime.Now;

            var dicts = new Dictionary<string, object>();
            dicts.Add("noreg", user.NoReg);
            dicts.Add("name", user.Name);
            dicts.Add("day", now.Day);
            dicts.Add("month", now.Month);
            dicts.Add("year", now.Year);

            UnitOfWork.Transact(() =>
            {
                var formKey = viewModel.FormKey;
                var form = FormRepository.Find(x => x.FormKey == formKey).FirstOrDefault();
                var formSequence = FormSequenceRepository.Fetch().FirstOrDefault(x => x.FormId == form.Id && x.Period == now.Year);
                var sequence = (formSequence?.SequenceNumber ?? 0) + 1;

                if (formSequence != null)
                {
                    formSequence.SequenceNumber = sequence;
                }
                else
                {
                    FormSequenceRepository.Add(FormSequence.Create(form.Id, now.Year, sequence));
                }

                dicts.Add("sequenceNumber", sequence.ToString("D4"));

                var documentNumber = StringHelper.Format(form.DocumentNumberFormat, dicts);

                dicts.Add("documentNumber", documentNumber);

                var documentApproval = new DocumentApproval
                {
                    FormId = form.Id,
                    DocumentNumber = documentNumber,
                    Title = callback(form.TitleFormat, dicts),//StringHelper.Format(form.TitleFormat, dicts),
                    DocumentStatusCode = EnumHelper.eDocumentAction.draft.ToString(),
                    CreatedBy = noreg
                };

                var documentRequestDetail = new DocumentRequestDetail
                {
                    DocumentApprovalId = documentApproval.Id,
                    ReferenceId = viewModel.ReferenceId,
                    ReferenceTable = viewModel.ReferenceTable,
                    ObjectValue = viewModel.ObjectValue,
                    RequestTypeCode = viewModel.ReferenceId.HasValue ? "update" : "new",
                    CreatedBy = noreg
                };

                if (viewModel != null && viewModel.Attachments != null)
                {
                    foreach (var item in viewModel.Attachments)
                    {
                        item.CommonFile = GetCommonFileById(item.CommonFileId);
                        item.DocumentApprovalId = documentApproval.Id;
                        item.CreatedBy = noreg;
                    }

                    documentApproval.DocumentApprovalFiles = viewModel.Attachments;
                }

                documentApproval.ObjDocumentRequestDetail = documentRequestDetail;

                DocumentApprovalRepository.Add(documentApproval);

                UnitOfWork.SaveChanges();

                viewModel.Id = documentRequestDetail.Id;
                viewModel.DocumentApprovalId = documentApproval.Id;

                MoveCommonFile(documentApproval);

                documentCreated(this, viewModel);
            });
        }

        /// <summary>
        /// Create approval document and detail auto-completed (by pass approval)
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="viewModel">Document Request Detail Object</param>
        public void CreateApprovalDocumentByPass(string noreg, DocumentRequestDetailViewModel viewModel, string postCode, Func<string, Dictionary<string, object>, string> callback)
        {
            var user = UserRepository.Find(x => x.NoReg == noreg).FirstOrDefaultIfEmpty();

            Assert.ThrowIf(viewModel.DocumentApprovalId != default(Guid), "Cannot create because document approval already exist");
            var now = DateTime.Now;

            var dicts = new Dictionary<string, object>();
            dicts.Add("noreg", user.NoReg);
            dicts.Add("name", user.Name);
            dicts.Add("day", now.Day);
            dicts.Add("month", now.Month);
            dicts.Add("year", now.Year);

            UnitOfWork.Transact(() =>
            {
                var formKey = viewModel.FormKey;
                var form = FormRepository.Find(x => x.FormKey == formKey).FirstOrDefault();
                var formSequence = FormSequenceRepository.Fetch().FirstOrDefault(x => x.FormId == form.Id && x.Period == now.Year);
                var sequence = (formSequence?.SequenceNumber ?? 0) + 1;

                if (formSequence != null)
                {
                    formSequence.SequenceNumber = sequence;
                }
                else
                {
                    FormSequenceRepository.Add(FormSequence.Create(form.Id, now.Year, sequence));
                }

                dicts.Add("sequenceNumber", sequence.ToString("D4"));

                var obj = JsonConvert.DeserializeObject<WeeklyWFHPlanningViewModel>(viewModel.ObjectValue);
                dicts.Add("monthPlanning", obj.WeeklyWFHPlanning.StartDate.ToString("MM"));

                var documentNumber = StringHelper.Format(form.DocumentNumberFormat, dicts);

                dicts.Add("documentNumber", documentNumber);

                var documentApproval = new DocumentApproval
                {
                    FormId = form.Id,
                    DocumentNumber = documentNumber,
                    Title = callback(form.TitleFormat, dicts),//StringHelper.Format(form.TitleFormat, dicts),
                    DocumentStatusCode = EnumHelper.eDocumentAction.completed.ToString(),
                    Progress = 100,
                    CreatedBy = noreg,
                    SubmitBy = noreg,
                    SubmitOn = now
                };

                var documentRequestDetail = new DocumentRequestDetail
                {
                    DocumentApprovalId = documentApproval.Id,
                    ReferenceId = viewModel.ReferenceId,
                    ReferenceTable = viewModel.ReferenceTable,
                    ObjectValue = viewModel.ObjectValue,
                    RequestTypeCode = viewModel.ReferenceId.HasValue ? "update" : "new",
                    CreatedBy = noreg
                };

                if (viewModel != null && viewModel.Attachments != null)
                {
                    foreach (var item in viewModel.Attachments)
                    {
                        item.CommonFile = GetCommonFileById(item.CommonFileId);
                        item.DocumentApprovalId = documentApproval.Id;
                        item.CreatedBy = noreg;
                    }

                    documentApproval.DocumentApprovalFiles = viewModel.Attachments;
                }

                documentApproval.ObjDocumentRequestDetail = documentRequestDetail;

                DocumentApprovalRepository.Add(documentApproval);

                UnitOfWork.SaveChanges();

                viewModel.Id = documentRequestDetail.Id;
                viewModel.DocumentApprovalId = documentApproval.Id;

                MoveCommonFile(documentApproval);

                documentCreated(this, viewModel);

                var emailTemplate = EmailTemplateRepository.Fetch()
                    .AsNoTracking()
                    .Where(x => x.MailKey.Contains(MailTemplate.WeeklyWfhPlanning))
                    .ToDictionary(a => a.MailKey);

                var templateReminder = emailTemplate[MailTemplate.WeeklyWfhPlanning];
                var templateAcknowledge = EmailTemplateRepository.Fetch().AsNoTracking().Where(x => x.MailKey == MailTemplate.AcknowledgeSuperiorHybrid).FirstOrDefault();
                var templateUserSubmission = EmailTemplateRepository.Fetch().AsNoTracking().Where(x => x.MailKey == MailTemplate.HybridWorkPlanningUserSubmission).FirstOrDefault();

                var configService = new ConfigService(UnitOfWork);
                var generalCategory = configService.GetGeneralCategory(templateReminder.ModuleCode);

                var sup = ActualReportingStructureReadonlyRepository.Find(x => x.NoReg == noreg && x.PostCode == postCode && x.HierarchyLevel == 1).FirstOrDefault();

                var supUser = UserRepository.Find(x => x.NoReg == sup.ParentNoReg).FirstOrDefault();
                var baseUrlConfigValue = configService.GetConfig("Application.Url")?.ConfigValue;

                var dataAcknowledge = new
                {
                    superior_name = supUser.Name,
                    module = generalCategory.Name,
                    form_title = templateAcknowledge.Title,
                    month = obj.WeeklyWFHPlanning.StartDate.ToString("MMMM"),
                    submitter_name = user.Name,
                    url = baseUrlConfigValue + "/timemanagement/form/view?formKey=weekly-wfh-planning&docid=" + documentApproval.Id
                };

                var dataUserSubmission = new
                {
                    name = user.Name,
                    module = generalCategory.Name,
                    form_title = templateUserSubmission.Title,
                    month = obj.WeeklyWFHPlanning.StartDate.ToString("MMMM"),
                    document_no = documentNumber,
                    submitter_name = user.Name,
                    url = baseUrlConfigValue + "/timemanagement/form/view?formKey=weekly-wfh-planning&docid=" + documentApproval.Id
                };

                var emailService = new EmailService(UnitOfWork);

                //var isSentAcknowledge = emailService.SendEmail(MailTemplate.AcknowledgeSuperiorHybrid, supUser.Email, dataAcknowledge);
                //var isSentUserSubmission = emailService.SendEmail(MailTemplate.HybridWorkPlanningUserSubmission, user.Email, dataUserSubmission);
                var isSentAcknowledge = emailService.SendEmailAsync(MailTemplate.AcknowledgeSuperiorHybrid, supUser.Email, dataAcknowledge);
                var isSentUserSubmission = emailService.SendEmailAsync(MailTemplate.HybridWorkPlanningUserSubmission, user.Email, dataUserSubmission);

            });
        }

        /// <summary>
        /// Create auto request approval document asynchronously
        /// </summary>
        /// <param name="lastApprover">Last Approver NoReg</param>
        /// <param name="parentDocumentApproval">Parent Document Approval Object</param>
        /// <param name="actualOrganizationStructure">Last Approver Organization Structure Object</param>
        /// <param name="viewModel">Document Request Detail Object</param>
        public void CreateAutoRequestApprovalDocumentAsync(string lastApprover, DocumentApproval parentDocumentApproval, ActualOrganizationStructure actualOrganizationStructure, ActualOrganizationStructure approverActualOrganizationStructure, DocumentRequestDetailViewModel viewModel)
        {
            Assert.ThrowIf(viewModel.DocumentApprovalId != default(Guid), "Cannot create because document approval already exist");
            var now = DateTime.Now;
            var noreg = parentDocumentApproval.CreatedBy;

            var dicts = new Dictionary<string, object>();
            dicts.Add("noreg", noreg);
            dicts.Add("name", actualOrganizationStructure.Name);
            dicts.Add("day", now.Day);
            dicts.Add("month", now.Month);
            dicts.Add("year", now.Year);

            UnitOfWork.Transact((transaction) =>
            {
                var formKey = viewModel.FormKey;
                var form = FormRepository.Find(x => x.FormKey == formKey).FirstOrDefault();
                var formSequence = FormSequenceRepository.Fetch().FirstOrDefault(x => x.FormId == form.Id && x.Period == now.Year);
                var sequence = (formSequence?.SequenceNumber ?? 0) + 1;

                if (formSequence != null)
                {
                    formSequence.SequenceNumber = sequence;
                }
                else
                {
                    FormSequenceRepository.Add(FormSequence.Create(form.Id, now.Year, sequence));
                }

                dicts.Add("sequenceNumber", sequence.ToString("D4"));

                var documentNumber = StringHelper.Format(form.DocumentNumberFormat, dicts);

                dicts.Add("documentNumber", documentNumber);

                var documentApproval = new DocumentApproval
                {
                    ParentId = parentDocumentApproval.Id,
                    FormId = form.Id,
                    DocumentNumber = documentNumber,
                    Title = StringHelper.Format(form.TitleFormat, dicts),
                    DocumentStatusCode = EnumHelper.eDocumentAction.draft.ToString(),
                    CreatedBy = noreg
                };

                var documentRequestDetail = new DocumentRequestDetail
                {
                    DocumentApprovalId = documentApproval.Id,
                    ReferenceId = viewModel.ReferenceId,
                    ReferenceTable = viewModel.ReferenceTable,
                    ObjectValue = viewModel.ObjectValue,
                    RequestTypeCode = viewModel.ReferenceId.HasValue ? "update" : "new"
                };

                if (viewModel != null && viewModel.Attachments != null)
                {
                    foreach (var item in viewModel.Attachments)
                    {
                        item.DocumentApprovalId = documentApproval.Id;
                    }

                    documentApproval.DocumentApprovalFiles = viewModel.Attachments;
                }

                documentApproval.ObjDocumentRequestDetail = documentRequestDetail;

                DocumentApprovalRepository.Add(documentApproval);

                UnitOfWork.SaveChanges();

                //post async innitiate
                DocumentApprovalHistoryRepository.Add(DocumentApprovalHistory.Create(documentApproval.Id, "initiate", actualOrganizationStructure, ""));
                TrackingApprovalRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).Delete();

                documentApproval.DocumentStatusCode = DocumentStatus.InProgress;
                documentApproval.SubmitBy = noreg;
                documentApproval.SubmitOn = now;
                documentApproval.LastApprovedBy = noreg;
                documentApproval.LastApprovedOn = now;

                var trackingApprovals = UnitOfWork.UspQuery<TrackingApprovalStoredEntity>(new
                {
                    documentApprovalId = documentApproval.Id,
                    username = actualOrganizationStructure.Name,
                    noreg = actualOrganizationStructure.NoReg,
                    postCode = actualOrganizationStructure.PostCode
                }, transaction);

                var approvers = trackingApprovals.Where(x => x.ApprovalLevel > 1);

                if (approvers.IsEmpty()) throw new Exception("Failed to submit document, there is no approvers from given approval matrix data. Please contact administrator for more information.");

                var minApprovalLevel = approvers.Min(x => x.ApprovalLevel);
                var nextApprovers = trackingApprovals.Where(x => x.ApprovalLevel == minApprovalLevel);

                var progress = (int)Math.Round(100 / (decimal)trackingApprovals.Select(x => x.ApprovalLevel).Distinct().Count());

                documentApproval.Progress = progress;

                //var nextApproverUsernames = "(" + string.Join("),(", UserRepository.Find(x => nextApprovers.Any(y => y.NoReg == x.NoReg)).Select(x => x.Username)) + ")";
                var nextApproverNoRegs = nextApprovers?.Select(y => y.NoReg).ToList() ?? new List<string>();

                string nextApproverUsernames = "";

                if (nextApproverNoRegs.Any())
                {
                    // Step 1: Get all users first
                    var allUsers = UserRepository.Fetch().Where(x => x.RowStatus).ToList();

                    // Step 2: Filter in memory
                    var usernames = allUsers
                        .Where(x => nextApproverNoRegs.Contains(x.NoReg))
                        .Select(x => x.Username)
                        .ToList();

                    nextApproverUsernames = "(" + string.Join("),(", usernames) + ")";
                }
                documentApproval.CurrentApprover = nextApproverUsernames;
                UnitOfWork.SaveChanges();

                //if request tax status
                if (formKey == "tax-status" || formKey == "absence")
                {
                    //update tracking approval
                    var group = TrackingApprovalRepository
                        .Fetch()
                        .AsNoTracking()
                        .Where(x => x.DocumentApprovalId == documentApproval.Id && string.IsNullOrEmpty(x.ApprovalActionCode))
                        .ToList()
                        .GroupBy(x => x.ApprovalLevel)
                        .OrderBy(x => x.Key);
                    //.FirstOrDefault();

                    foreach (var item in group)
                    {
                        //create history
                        ActualOrganizationStructure actualOrganizationStructureApprover = new MdmService(UnitOfWork).GetActualOrganizationStructure(item.FirstOrDefault().NoReg);
                        DocumentApprovalHistoryRepository.Add(DocumentApprovalHistory.Create(documentApproval.Id, "approve", actualOrganizationStructureApprover, ""));

                        var trackingApproval = group.FirstOrDefault(x => x.FirstOrDefault().NoReg == item.FirstOrDefault().NoReg);

                        TrackingApprovalRepository.Fetch()
                            .Where(x => x.DocumentApprovalId == documentApproval.Id && x.ApprovalLevel == trackingApproval.FirstOrDefault().ApprovalLevel)
                            .Update(x => new TrackingApproval { ApprovalActionCode = "approve", Remarks = "" });

                        UnitOfWork.SaveChanges();
                    }

                    documentApproval.Progress = 100;
                    documentApproval.DocumentStatusCode = DocumentStatus.Completed;
                    documentApproval.CurrentApprover = null;

                    UnitOfWork.SaveChanges();

                    if (documentApproval.DocumentStatusCode == DocumentStatus.Completed && _completeHandlers.ContainsKey(formKey))
                    {
                        _completeHandlers[formKey](lastApprover, documentApproval);
                    }

                    if (formKey == "absence")
                    {
                        DomainEventManager.Raise(new DocumentApprovalEvent(ApprovalAction.Approve, documentApproval, approverActualOrganizationStructure, string.Empty));
                    }
                }
            });

        }

        /// <summary>
        /// Create multiple approval documents and details
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="viewModel">Document Request Detail Object</param>
        public DocumentApproval CreateApprovalDocuments<T>(string noreg, ParentDocumentRequestDetailViewModel<T> parentDocumentRequestDetail) where T : class
        {
            var user = UserRepository.Find(x => x.NoReg == noreg).FirstOrDefaultIfEmpty();

            Assert.ThrowIf(parentDocumentRequestDetail.Id != Guid.Empty, "Cannot create because document approval already exist");
            var now = DateTime.Now;

            var dicts = new Dictionary<string, object>();
            dicts.Add("noreg", user.NoReg);
            dicts.Add("name", user.Name);
            dicts.Add("day", now.Day);
            dicts.Add("month", now.Month);
            dicts.Add("year", now.Year);

            var documentApproval = new DocumentApproval();

            UnitOfWork.Transact(() =>
            {
                var formKey = parentDocumentRequestDetail.FormKey;
                var form = FormRepository.Find(x => x.FormKey == formKey).FirstOrDefault();

                Assert.ThrowIf(form == null, $"Form with key '{formKey}' is not found");

                var formSequence = FormSequenceRepository.Fetch().FirstOrDefault(x => x.FormId == form.Id && x.Period == now.Year);
                var sequence = (formSequence?.SequenceNumber ?? 0) + 1;

                if (formSequence != null)
                {
                    formSequence.SequenceNumber = sequence;
                }
                else
                {
                    FormSequenceRepository.Add(FormSequence.Create(form.Id, now.Year, sequence));
                }

                dicts.Add("sequenceNumber", sequence.ToString("D4"));

                var documentNumber = StringHelper.Format(form.DocumentNumberFormat, dicts);

                dicts.Add("documentNumber", documentNumber);

                documentApproval = new DocumentApproval
                {
                    FormId = form.Id,
                    DocumentNumber = documentNumber,
                    Title = StringHelper.Format(form.TitleFormat, dicts),
                    DocumentStatusCode = EnumHelper.eDocumentAction.draft.ToString()
                };

                var documentRequestDetail = new DocumentRequestDetail
                {
                    DocumentApprovalId = documentApproval.Id,
                    ObjectValue = parentDocumentRequestDetail.ObjectValue ?? "{}",
                    RequestTypeCode = "new"
                };

                documentApproval.EnableDocumentAction = false;
                documentApproval.ObjDocumentRequestDetail = documentRequestDetail;

                DocumentApprovalRepository.Add(documentApproval);

                UnitOfWork.SaveChanges();

                var parentId = documentApproval.Id;
                var counter = 1;

                foreach (var viewModel in parentDocumentRequestDetail.RequestDetails)
                {
                    var childDocumentNumber = documentNumber + "/" + counter.ToString("D4");

                    var chlidDocumentApproval = new DocumentApproval
                    {
                        FormId = form.Id,
                        DocumentNumber = childDocumentNumber,
                        Title = documentApproval.Title + " #" + counter,
                        ParentId = documentApproval.Id,
                        DocumentStatusCode = EnumHelper.eDocumentAction.draft.ToString(),
                        VisibleInHistory = false
                    };

                    var childRequestDetail = new DocumentRequestDetail
                    {
                        DocumentApprovalId = chlidDocumentApproval.Id,
                        ObjectValue = viewModel.ObjectValue,
                        RequestTypeCode = "new"
                    };

                    chlidDocumentApproval.ObjDocumentRequestDetail = childRequestDetail;

                    DocumentApprovalRepository.Add(chlidDocumentApproval);

                    counter++;
                }

                UnitOfWork.SaveChanges();
            });

            return documentApproval;
        }

        /// <summary>
        /// Check whether user can create form with the given key or not
        /// </summary>
        /// <param name="formKey">Form Key</param>
        /// <param name="noReg">NoReg</param>
        /// <returns>True if user can create form</returns>
        public void ValidateCreateDocument(string formKey, string noReg)
        {
            var now = DateTime.Now;

            // --- BEFORE (dikomentari agar jejak perubahan tersimpan) ---
            // var statusCodes = new[] { DocumentStatus.Draft, DocumentStatus.InProgress, DocumentStatus.Revised };
            // var hasPendingDocument = DocumentApprovalRepository.Fetch().AsNoTracking()
            //     .Any(x => x.CreatedBy == noReg
            //         && statusCodes.Contains(x.DocumentStatusCode) // <- ini yang memicu OPENJSON
            //         && x.RowStatus && x.VisibleInHistory
            //         && x.Form.FormKey == formKey);

            // --- AFTER (tanpa Contains -> tidak ada OPENJSON) ---
            var hasPendingDocument = DocumentApprovalRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.CreatedBy == noReg
                    && (
                           x.DocumentStatusCode == DocumentStatus.Draft
                        || x.DocumentStatusCode == DocumentStatus.InProgress
                        || x.DocumentStatusCode == DocumentStatus.Revised
                       )
                    && x.RowStatus
                    && x.VisibleInHistory
                    && x.Form.FormKey == formKey)
                .Any();

            var lastCompletedDocument = DocumentApprovalRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus
                    && x.VisibleInHistory
                    && x.DocumentStatusCode == DocumentStatus.Completed
                    && x.Form.FormKey == formKey)
                .OrderByDescending(x => x.ModifiedOn)
                .FirstOrDefault();

            var output = !hasPendingDocument
                         || (lastCompletedDocument != null
                             && ((now - lastCompletedDocument.ModifiedOn.Value).TotalDays >= 30));

            Assert.ThrowIf(!output,
                "Cannot create new request because existing request is already running. Please contact administrator for more information");
        }

        public string ValidateLoadDocument(string formKey, string noReg)
        {
            var now = DateTime.Now;
            var statusCodes = new[] { DocumentStatus.Draft, DocumentStatus.InProgress, DocumentStatus.Revised };
            var hasPendingDocument = DocumentApprovalRepository.Fetch().AsNoTracking().Any(x => x.CreatedBy == noReg && statusCodes.Contains(x.DocumentStatusCode) && x.RowStatus && x.VisibleInHistory && x.Form.FormKey == formKey);
            var lastCompletedDocument = DocumentApprovalRepository.Fetch().AsNoTracking().Where(x => x.RowStatus && x.VisibleInHistory && x.DocumentStatusCode == DocumentStatus.Completed && x.Form.FormKey == formKey).OrderByDescending(x => x.ModifiedOn).FirstOrDefault();

            var output = !hasPendingDocument || (lastCompletedDocument != null && ((now - lastCompletedDocument.ModifiedOn.Value).TotalDays >= 30));

            if (!output)
            {
               return "Cannot create new request because existing request is already running. Please contact administrator for more information";
            }

            return "";
        }

        /// <summary>
        /// Check whether user can update the document or not
        /// </summary>
        /// <param name="id">Document Id</param>
        /// <param name="noReg">NoReg</param>
        /// <returns>True if user can update document</returns>
        public bool CanUpdateDocument(Guid id, string noReg)
        {
            var documentApproval = DocumentApprovalRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == id);
            var form = FormRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == documentApproval.FormId);

            if (form.FormKey == "termination")
            {
                var terminationUserNoreg = (from user in UserRepository.Fetch().Where(x => x.NoReg == noReg)
                                            join userrole in UserRoleRepository.Fetch() on user.Id equals userrole.UserId
                                            join role in RoleRepository.Fetch().Where(x => x.RoleKey == "TERMINATION_USER") on userrole.RoleId equals role.Id
                                            select new
                                            {
                                                user.NoReg
                                            }).ToList();

                if (terminationUserNoreg.Count > 0)
                {
                    return ObjectHelper.IsIn(documentApproval.DocumentStatusCode, DocumentStatus.Draft, DocumentStatus.Revised);
                }
                else
                {
                    return documentApproval.CreatedBy == noReg && ObjectHelper.IsIn(documentApproval.DocumentStatusCode, DocumentStatus.Draft, DocumentStatus.Revised);
                }
            }
            else
            {
                return documentApproval.CreatedBy == noReg && ObjectHelper.IsIn(documentApproval.DocumentStatusCode, DocumentStatus.Draft, DocumentStatus.Revised);
            }
        }

        public void UploadAttachment(GenericUploadViewModel viewModel, string fieldCategory = "AttachmentFilePath")
        {
            viewModel.Attachments.ForEach(x =>
            {
                x.DocumentApprovalId = viewModel.DocumentApprovalId;
                DocumentApprovalFileRepository.Add(x);
            });

            var documentApproval = DocumentApprovalRepository.FindById(viewModel.DocumentApprovalId);
            documentApproval.CanSubmit = true;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete document approval and its dependencies (if any)
        /// </summary>
        /// <param name="id">Document Approval Id</param>
        public void SoftDeleteDocumentApproval(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == id).Update(x => new DocumentRequestDetail { RowStatus = false });
                DocumentApprovalParticipantRepository.Fetch().Where(x => x.DocumentApprovalId == id).Update(x => new DocumentApprovalParticipant { RowStatus = false });
                DocumentApprovalConvertationRepository.Fetch().Where(x => x.DocumentApprovalId == id).Update(x => new DocumentApprovalConversation { RowStatus = false });
                DocumentApprovalHistoryRepository.Fetch().Where(x => x.DocumentApprovalId == id).Update(x => new DocumentApprovalHistory { RowStatus = false });
                DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == id).Update(x => new DocumentApprovalFile { RowStatus = false });
                TrackingApprovalRepository.Fetch().Where(x => x.DocumentApprovalId == id).Update(x => new TrackingApproval { RowStatus = false });
                DocumentApprovalRepository.Fetch().Where(x => x.Id == id).Update(x => new DocumentApproval { RowStatus = false });

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Delete document approval by id
        /// </summary>
        /// <param name="id">Document Approval Id</param>
        public void DeleteDocumentApproval(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == id).Delete();
                DocumentApprovalParticipantRepository.Fetch().Where(x => x.DocumentApprovalId == id).Delete();
                DocumentApprovalConvertationRepository.Fetch().Where(x => x.DocumentApprovalId == id).Delete();
                DocumentApprovalHistoryRepository.Fetch().Where(x => x.DocumentApprovalId == id).Delete();
                DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == id).Delete();
                TrackingApprovalRepository.Fetch().Where(x => x.DocumentApprovalId == id).Delete();
                DocumentApprovalRepository.Fetch().Where(x => x.Id == id).Delete();

                UnitOfWork.SaveChanges();
            });
        }
        #endregion

        #region Document Request Detail and Common File Area
        /// <summary>
        /// Get list document request detail
        /// </summary>
        /// <returns>List Document Request Detail</returns>
        public IQueryable<DocumentRequestDetail> GetDocumentRequestDetail() => DocumentRequestDetailRepository.Fetch().Where(x => x.RowStatus);

        /// <summary>
        /// Get document request detail by document approval
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns></returns>
        public T GetDocumentRequestDetail<T>(Guid documentApprovalId) where T : class
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.DocumentApprovalId == documentApprovalId);

            return JsonConvert.DeserializeObject<T>(documentRequestDetail.ObjectValue);
        }

        /// <summary>
        /// Get document request detail by document approval id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>Document Request Detail</returns>
        public DocumentRequestDetail GetDocumentRequestDetailByApprovalId(Guid documentApprovalId) => DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApprovalId).FirstOrDefault();

        /// <summary>
        /// Get document approvals by document approval id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <returns>Document Approvals</returns>
        public IQueryable<DocumentApprovalFile> GetDocumentApprovalFileDocId(Guid documentApprovalId)
        {
            return DocumentApprovalFileRepository.Fetch()
                .AsNoTracking()
                .Include(x => x.CommonFile)
                .Where(x => x.DocumentApprovalId == documentApprovalId);
        }

        /// <summary>
        /// Update document request detail
        /// </summary>
        /// <param name="viewModel">Document Request Detail View Model</param>
        public void UpdateDocumentRequestDetail(DocumentRequestDetailViewModel viewModel, Func<string, Dictionary<string, object>, string> callback = null)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.FindById(viewModel.Id);

            documentRequestDetail.ObjectValue = viewModel.ObjectValue;
            if (viewModel != null && viewModel.Attachments != null)
            {
                var arrayFields = viewModel.Attachments.Where(x => x.FieldCategory.Contains("["))
                    .Select(x => x.FieldCategory.Substring(0, x.FieldCategory.IndexOf('[')))
                    .Distinct();

                if (arrayFields.Count() > 0)
                {
                    DocumentApprovalFileRepository.Fetch()
                        .Where(x => x.DocumentApprovalId == documentRequestDetail.DocumentApprovalId && arrayFields.Any(y => x.FieldCategory.Contains(y + "[%%]")))
                        .Delete();
                }

                foreach (var item in viewModel.Attachments)
                {
                    item.CommonFile = GetCommonFileById(item.CommonFileId);
                    item.DocumentApprovalId = documentRequestDetail.DocumentApprovalId;
                }

                documentRequestDetail.DocumentApproval = GetDocumentApprovalById(documentRequestDetail.DocumentApprovalId);
                documentRequestDetail.DocumentApproval.DocumentApprovalFiles = viewModel.Attachments;
            }

            UnitOfWork.SaveChanges();
            MoveCommonFile(documentRequestDetail.DocumentApproval);

            var docApproval = DocumentApprovalRepository.FindById(documentRequestDetail.DocumentApprovalId);

            if (callback != null)
            {
                var newTitle = GetNewTitle(docApproval.FormId, docApproval.DocumentNumber, docApproval.CreatedBy, callback);

                if (docApproval.Title != newTitle)
                {
                    docApproval.Title = newTitle;
                }
            }

            docApproval.CanSubmit = true;

            DocumentApprovalRepository.Upsert<Guid>(docApproval);

            UnitOfWork.SaveChanges();

            documentUpdated(this, viewModel);
        }

        private string GetNewTitle(Guid formId, string documentNumber, string noreg, Func<string, Dictionary<string, object>, string> callback)
        {
            var form = FormRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == formId);
            var user = UserRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.NoReg == noreg);
            var now = DateTime.Now;
            var dicts = new Dictionary<string, object>
            {
                { "noreg", user.NoReg },
                { "name", user.Name },
                { "day", now.Day },
                { "month", now.Month },
                { "year", now.Year },
                { "documentNumber", documentNumber }
            };

            var newTitle = callback(form.TitleFormat, dicts);

            return newTitle;
        }

        /// <summary>
        /// Update multiple document request details
        /// </summary>
        /// <param name="viewModel">Document Request Detail View Model</param>
        public DocumentApproval UpdateDocumentRequestDetails<T>(ParentDocumentRequestDetailViewModel<T> parentDocumentRequestDetail) where T : class
        {
            var parentDocumentApproval = DocumentApprovalRepository.Fetch()
                .AsNoTracking()
                .Include(x => x.Form)
                .FirstOrDefault(x => x.Id == parentDocumentRequestDetail.Id);

            var docIds = parentDocumentRequestDetail.RequestDetails.Select(x => x.Id);
            var parentDocumentNumber = parentDocumentApproval.DocumentNumber;
            var formId = parentDocumentApproval.FormId;
            var counter = 1;

            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_CLEAR_DOCUMENT_REQUEST_DETAILS", new { id = parentDocumentApproval.Id }, trans);

                foreach (var viewModel in parentDocumentRequestDetail.RequestDetails)
                {
                    var childDocumentNumber = parentDocumentNumber + "/" + counter.ToString("D4");

                    var chlidDocumentApproval = new DocumentApproval
                    {
                        FormId = formId,
                        DocumentNumber = childDocumentNumber,
                        Title = parentDocumentApproval.Title + " #" + counter,
                        ParentId = parentDocumentApproval.Id,
                        DocumentStatusCode = EnumHelper.eDocumentAction.draft.ToString(),
                        VisibleInHistory = false
                    };

                    var childRequestDetail = new DocumentRequestDetail
                    {
                        DocumentApprovalId = chlidDocumentApproval.Id,
                        ObjectValue = viewModel.ObjectValue,
                        RequestTypeCode = "new"
                    };

                    chlidDocumentApproval.ObjDocumentRequestDetail = childRequestDetail;

                    DocumentApprovalRepository.Add(chlidDocumentApproval);

                    counter++;
                }

                UnitOfWork.SaveChanges();
            });

            return parentDocumentApproval;
        }

        /// <summary>
        /// Get document request detail
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="id">Document Approval Id</param>
        /// <returns>Document Request Detail Object</returns>
        public DocumentRequestDetailViewModel<T> GetDocumentRequestDetailViewModel<T>(Guid id, string noreg) where T : class
        {
            var dokumenApproval = DocumentApprovalRepository.Find(x => x.Id == id).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Find(x => x.DocumentApproval.Id == id).FirstOrDefaultIfEmpty();
            if (documentRequestDetail.Id != default(Guid))
            {
                documentRequestDetail.DocumentApproval = DocumentApprovalRepository.Fetch()
                    .AsNoTracking()
                    .Include(x => x.Form)
                    .Where(x => x.Id == id)
                    .FirstOrDefaultIfEmpty();

                if (documentRequestDetail.DocumentApproval != null)
                {
                    documentRequestDetail.DocumentApproval.DocumentApprovalFiles = GetDocumentApprovalFileDocId(documentRequestDetail.DocumentApproval.Id).ToList();
                    //documentRequestDetail.DocumentApproval.DocumentApprovalHistories = GetDocumentApprovalHistories(documentRequestDetail.DocumentApproval.Id).ToList();
                }
            }

            ApprovalMatrix approvalMatrix = new ApprovalMatrix();
            var group = TrackingApprovalRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.DocumentApprovalId == id && string.IsNullOrEmpty(x.ApprovalActionCode))
                .ToList()
                .GroupBy(x => x.ApprovalLevel)
                .OrderBy(x => x.Key)
                .FirstOrDefault();

            if (group != null)
            {
                var trackingApproval = group.FirstOrDefault(x => x.NoReg == noreg);

                if (trackingApproval != null)
                {
                    if (trackingApproval.ApprovalMatrixId.HasValue)
                    {
                        approvalMatrix = ApprovalMatrixRepository.FindById(trackingApproval.ApprovalMatrixId.Value);
                    }
                }
                else
                {
                    //if (group.FirstOrDefault().ApprovalMatrixId.HasValue)
                    //{
                    //    approvalMatrix = ApprovalMatrixRepository.FindById(group.FirstOrDefault().ApprovalMatrixId.Value);
                    //}
                }
            }

            if (approvalMatrix == null)
            {
                approvalMatrix = new ApprovalMatrix();
            }

            var viewModel = DocumentRequestDetailViewModel<T>.Create(documentRequestDetail, approvalMatrix.Approver, approvalMatrix.Permissions, dokumenApproval.CreatedBy);

            return viewModel;
        }

        public IEnumerable<DocumentRequestDetailViewModel<T>> GetDocumentRequestDetails<T>(Guid id) where T : class
        {
            var documents = from d in DocumentApprovalRepository.Fetch()
                            join dd in DocumentRequestDetailRepository.Fetch()
                            on d.Id equals dd.DocumentApprovalId
                            where d.ParentId.Equals(id)
                            select dd;

            return DocumentRequestDetailViewModel<T>.Create(documents.ToArray());
        }

        /// <summary>
        /// Update or insert document request
        /// </summary>
        /// <param name="DocumentApproval">DocumentApproval Object</param>
        /// <param name="DocumentRequestDetail">DocumentRequestDetail Object</param>
        public void UpsertRequestApprovalDoc(DocumentApproval documentApproval)
        {
            UnitOfWork.Transact(() =>
            {
                DocumentApprovalRepository.Upsert<Guid>(documentApproval);

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Get CommonFile by id
        /// </summary>
        /// <param name="id">CommonFile Id</param>
        /// <returns>CommonFile</returns>
        public CommonFile GetCommonFileById(Guid id) => CommonFileRepository.Fetch().Where(x => x.Id == id).FirstOrDefault();

        /// <summary>
        /// Update or insert common file
        /// </summary>
        /// <param name="commonFile">CommonFile Object</param>
        public void SaveCommonFile(CommonFile commonFile)
        {
            UnitOfWork.Transact(() =>
            {
                CommonFileRepository.Upsert<Guid>(commonFile);

                UnitOfWork.SaveChanges();
            });
        }

        public DocumentApproval GetDocumentApprovalCommonFileById(Guid id)
        {
            var docFile = DocumentApprovalFileRepository.Fetch().Where(x => x.CommonFileId == id).FirstOrDefault();
            if (docFile != null) return GetDocumentApprovalById(docFile.DocumentApprovalId);

            return null;
        }




        public void MoveCommonFile(DocumentApproval documentApproval)
        {

            if (documentApproval == null) return;
            if (documentApproval.DocumentApprovalFiles == null) return;


            foreach (var file in documentApproval?.DocumentApprovalFiles)
            {
                // 1. Convert & validate ID sebagai GUID
                if (!Guid.TryParse(file?.CommonFile?.Id.ToString(), out Guid safeGuid))
                {
                    throw new SecurityException("Invalid file ID");
                }

                // 2. Ambil filename dan sanitize
                var safeOriginalFileName =
                    Path.GetFileName(file?.CommonFile?.FileName)?.SanitizeFileName();

                // 3. Gabungkan GUID + filename
                var fileName = $"{safeGuid}-{safeOriginalFileName}";
                //var fileName = file?.CommonFile?.Id + "-" + Path.GetFileName(file?.CommonFile?.FileName);

                var config = ConfigRepository.Fetch().Where(x => x.ConfigKey == "Upload.Path").FirstOrDefault();

                var configPath = config?.ConfigValue;

                var filesPath = !string.IsNullOrEmpty(configPath) ? configPath : "";

                string sourceFile = System.IO.Path.Combine(filesPath, fileName);

                System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9 -]");
                var docNumber = rgx.Replace(documentApproval.DocumentNumber, "-");

                string targetPath = System.IO.Path.Combine(configPath, documentApproval.CreatedOn.Year.ToString(), docNumber);

                if (System.IO.File.Exists(sourceFile))
                {
                    string destFile = System.IO.Path.Combine(targetPath, fileName);

                    if (!System.IO.Directory.Exists(targetPath))
                    {
                        System.IO.Directory.CreateDirectory(targetPath);
                    }

                    System.IO.File.Move(sourceFile, destFile);
                }
            }

        }


        /// <summary>
        /// Delete document file by id
        /// </summary>
        /// <param name="commonFileId">Common File Id</param>
        public void DeleteDocumentFile(Guid commonFileId)
        {
            UnitOfWork.Transact(() =>
            {
                DocumentApprovalFileRepository.Fetch().Where(x => x.CommonFileId == commonFileId).Delete();
                CommonFileRepository.Fetch().Where(x => x.Id == commonFileId).Delete();

                UnitOfWork.SaveChanges();
            });
        }
        #endregion

        #region User Task Area
        /// <summary>
        /// Get child task query
        /// </summary>
        /// <returns>Child Task Query</returns>
        public IQueryable<DocumentApprovalView> GetChilds(Guid id)
        {
            return DocumentApprovalReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.ParentId == id);
        }

        /// <summary>
        /// Get task query
        /// </summary>
        /// <returns>Task Query</returns>
        public IQueryable<DocumentApprovalView> GetTasks()
        {
            return DocumentApprovalReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.VisibleInHistory);
        }

        /// <summary>
        /// Get task query
        /// </summary>
        /// <returns>Task Query for Hybrid Work Planning</returns>
        public IQueryable<HybridWorkPlanningView> GetOtherTasks()
        {
            return HybridWorkPlanningRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.VisibleInHistory);
        }

        /// <summary>
        /// Get completed tasks query
        /// </summary>
        /// <returns>Completed Tasks Query</returns>
        public IQueryable<DocumentApprovalView> GetCompletedTasks()
        {
            return DocumentApprovalReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.VisibleInHistory && x.IntegrationDownload && x.DocumentStatusCode == DocumentStatus.Completed);
        }


        /// <summary>
        /// Get task query by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Task Query</returns>
        public IQueryable<DocumentApprovalView> GetTasks(string username)
        {
            return GetTasks().Where(x => x.CurrentApprover.Contains("(" + username + ")"));
        }

        /// <summary>
        /// Get task query by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Task Query for Hybrid Work Planning</returns>
        public IQueryable<HybridWorkPlanningView> GetOtherTasks(string username)
        {
            return GetOtherTasks().Where(x => x.CurrentApprover.Contains("(" + username + ")"));
        }

        public IQueryable<DocumentApprovalView> GetTasksTermination(string username, string formKey)
        {
            return GetTasks().Where(x => x.CurrentApprover.Contains("(" + username + ")") && x.FormKey == formKey && (x.DocumentStatusCode).ToLower() != "revised");
        }
        /// <summary>
        /// Get task query by noreg and form key
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="formKey">Form key</param>
        /// <returns>Task Query</returns>
        public IQueryable<DocumentApprovalView> GetTasks(string noreg, string formKey)
        {
            return GetTasks().Where(x => x.CreatedBy == noreg && (x.FormKey == formKey || string.IsNullOrEmpty(formKey)));
        }

        /// <summary>
        /// Get task query by noreg and form key
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="formKey">Form key</param>
        /// <returns>Task Query for Hybrid Work Planning</returns>
        public IQueryable<HybridWorkPlanningView> GetOtherTasks(string noreg, string formKey)
        {
            return GetOtherTasks().Where(x => x.CreatedBy == noreg && (x.FormKey == formKey || string.IsNullOrEmpty(formKey)));
        }

        public IQueryable<DocumentApprovalView> GetTasksListTermintion(string noreg, bool canTerminate, string formKey)
        {

            if (canTerminate == true)
            {
                //var terminationUserNoreg = (from user in UserRepository.Fetch()
                //                            join userrole in UserRoleRepository.Fetch() on user.Id equals userrole.UserId
                //                            join role in RoleRepository.Fetch().Where(x => x.RoleKey == "TERMINATION_USER") on userrole.RoleId equals role.Id
                //                            select new
                //                            {
                //                                user.NoReg
                //                            }).ToList();

                //string[] result = terminationUserNoreg.Where(x => x != null)
                //           .Select(x => x.NoReg.ToString())
                //           .ToArray();

                //return GetTasks().Where(x => result.Contains(x.CreatedBy) && (x.FormKey == formKey || string.IsNullOrEmpty(formKey)));
                return GetTasks().Where(x => x.FormKey == formKey || string.IsNullOrEmpty(formKey));
            }
            else
            {
                return GetTasks().Where(x => x.CreatedBy == noreg && (x.FormKey == formKey || string.IsNullOrEmpty(formKey)));
            }

        }

        /// <summary>
        /// Get task histories query by noreg
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Task Histories Query</returns>
        public IQueryable<DocumentApproval> GetTaskHistories(string noreg)
        {
            return DocumentApprovalRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.DocumentApprovalHistories.Any(y => y.NoReg == noreg && y.RowStatus) && x.RowStatus && x.VisibleInHistory)
                .Include(x => x.Form);
        }

        public IQueryable<DocumentApproval> GetTaskHistoriesTermination(string noreg, string formKey)
        {
            return DocumentApprovalRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.DocumentApprovalHistories.Any(y => y.NoReg == noreg && y.RowStatus) && x.RowStatus && x.VisibleInHistory && x.Form.FormKey == formKey)
                .Include(x => x.Form);
        }

        /// <summary>
        /// Get list of tasks with limit
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="limit">Limit Number</param>
        /// <returns>List of Tasks Object</returns>
        public NotificationViewModel GetTopTasks(string username, int limit)
        {
            var totalUnread = GetTasks(username).Count();
            var topTasks = GetTasks(username)
                .OrderByDescending(x => x.ModifiedOn)
                .Take(limit)
                .Select(x => new { x.Title, x.Progress, x.FormKey, x.Id });

            var notificationViewModel = new NotificationViewModel { Total = totalUnread, Messages = topTasks };

            return notificationViewModel;
        }
        /// <summary>
        /// Find duplicate bpkb request in Document Approval with status inprogress and completed in range between Loan Date and Return Date
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="formkey">FormKey</param>
        /// <param name="loanDate">Loan Date Or Borrow Date</param>
        /// <returns>Document Number, Loan Date, Return Date</returns>
        public IEnumerable<(string, DateTime, DateTime)> FindDuplicateBpkbRequest(string noreg, string formkey, DateTime loanDate, DateTime returnDate)
        {
            var connection = UnitOfWork.GetConnection();
            var any = connection.Query<(string, DateTime, DateTime)>(
                    @"SELECT vda.DocumentNumber, 
                        TRY_CONVERT(datetime, SUBSTRING(trdrd.objectvalue, CHARINDEX('""LoanDate"":""', trdrd.objectvalue) + 12, 19), 126) AS LoanDate,
                        TRY_CONVERT(datetime, SUBSTRING(trdrd.objectvalue, CHARINDEX('""ReturnDate"":""', trdrd.objectvalue) + 14, 19), 126) AS ReturnDate
                      FROM VW_DOCUMENT_APPROVAL vda
                      JOIN TB_R_DOCUMENT_REQUEST_DETAIL trdrd ON trdrd.DocumentApprovalId = vda.Id
                      WHERE vda.CreatedBy = @NoReg AND vda.FormKey = @FormKey AND vda.DocumentStatusCode IN ('inprogress', 'completed')
                        AND (
                        (TRY_CONVERT(datetime, SUBSTRING(trdrd.objectvalue, CHARINDEX('""LoanDate"":""', trdrd.objectvalue) + 12, 19), 126) <= @LoanDate
                        AND TRY_CONVERT(datetime, SUBSTRING(trdrd.objectvalue, CHARINDEX('""ReturnDate"":""', trdrd.objectvalue) + 14, 19), 126) >= @LoanDate) OR 
                        (TRY_CONVERT(datetime, SUBSTRING(trdrd.objectvalue, CHARINDEX('""LoanDate"":""', trdrd.objectvalue) + 12, 19), 126) <= @ReturnDate
                        AND TRY_CONVERT(datetime, SUBSTRING(trdrd.objectvalue, CHARINDEX('""ReturnDate"":""', trdrd.objectvalue) + 14, 19), 126) >= @ReturnDate)
                        )", new
                    {
                        NoReg = noreg,
                        FormKey = formkey,
                        LoanDate = loanDate,
                        ReturnDate = returnDate
                    });

            return any;

        }
        #endregion

        #region Workflow Area
        private void UpdateParentDocument(string username, bool isComplete, DocumentApproval documentApproval, ActualOrganizationStructure actualOrganizationStructure)
        {
            if (documentApproval.ParentId.HasValue)
            {
                var parentDocumentApproval = DocumentApprovalRepository.Fetch().FirstOrDefault(x => x.Id == documentApproval.ParentId);

                if (parentDocumentApproval == null || parentDocumentApproval.Progress == 100) return;

                if (isComplete)
                {
                    var now = DateTime.Now;

                    var totalChilds = DocumentApprovalRepository.Fetch()
                        .AsNoTracking()
                        .Where(x => x.ParentId == parentDocumentApproval.Id)
                        .Count();

                    var totalCompleteChilds = DocumentApprovalRepository.Fetch()
                        .AsNoTracking()
                        .Where(x => x.ParentId == parentDocumentApproval.Id && (x.DocumentStatusCode == DocumentStatus.Completed || x.DocumentStatusCode == DocumentStatus.Rejected))
                        .Count();

                    var totalRejectChilds = DocumentApprovalRepository.Fetch()
                        .AsNoTracking()
                        .Where(x => x.ParentId == parentDocumentApproval.Id && (x.DocumentStatusCode == DocumentStatus.Rejected))
                        .Count();

                    var isAllReject = false;

                    if (documentApproval.DocumentStatusCode == DocumentStatus.Rejected)
                    {
                        isAllReject = totalRejectChilds + 1 == totalChilds ? true : false;
                    }



                    var parentProgress = (int)Math.Round(100 * (totalCompleteChilds + 1) / (decimal)totalChilds);

                    parentDocumentApproval.Progress = parentProgress;
                    parentDocumentApproval.DocumentStatusCode = parentProgress == 100 && !isAllReject
                        ? DocumentStatus.Completed
                        : parentProgress == 100 && isAllReject ? DocumentStatus.Rejected : parentDocumentApproval.DocumentStatusCode;

                    if (parentProgress == 100 && !isAllReject)
                    {
                        parentDocumentApproval.CurrentApprover = null;

                        TrackingApprovalRepository.Fetch()
                            .Where(x => x.DocumentApprovalId == parentDocumentApproval.Id && x.ApprovalActionCode == null)
                            .Update(x => new TrackingApproval
                            {
                                ApprovalActionCode = ApprovalAction.Approve,
                                ModifiedBy = actualOrganizationStructure.NoReg,
                                ModifiedOn = now
                            });

                        DocumentApprovalHistoryRepository.Add(DocumentApprovalHistory.Create(parentDocumentApproval.Id, ApprovalAction.Approve, actualOrganizationStructure, string.Empty));

                        DomainEventManager.Raise(new DocumentApprovalEvent(ApprovalAction.Approve, parentDocumentApproval, actualOrganizationStructure, null));
                    }
                    else if (parentProgress == 100 && isAllReject)
                    {
                        parentDocumentApproval.CurrentApprover = null;

                        TrackingApprovalRepository.Fetch()
                            .Where(x => x.DocumentApprovalId == parentDocumentApproval.Id && x.ApprovalActionCode == null)
                            .Update(x => new TrackingApproval
                            {
                                ApprovalActionCode = ApprovalAction.Reject,
                                ModifiedBy = actualOrganizationStructure.NoReg,
                                ModifiedOn = now
                            });

                        DocumentApprovalHistoryRepository.Add(DocumentApprovalHistory.Create(parentDocumentApproval.Id, ApprovalAction.Reject, actualOrganizationStructure, string.Empty));

                        DomainEventManager.Raise(new DocumentApprovalEvent(ApprovalAction.Reject, parentDocumentApproval, actualOrganizationStructure, null));
                    }
                    else
                    {
                        var parentCurrentApprover = (parentDocumentApproval.CurrentApprover ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (DocumentApprovalRepository.Fetch().AsNoTracking().Count(x => x.ParentId == documentApproval.ParentId && x.CurrentApprover.Contains("(" + username + ")")) == 1)
                        {
                            parentCurrentApprover.Remove("(" + username + ")");

                            parentDocumentApproval.CurrentApprover = string.Join(",", parentCurrentApprover);
                        }
                    }
                }
                else
                {
                    var parentCurrentApprover = (parentDocumentApproval.CurrentApprover ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    if (DocumentApprovalRepository.Fetch().AsNoTracking().Count(x => x.ParentId == documentApproval.ParentId && x.CurrentApprover.Contains("(" + username + ")")) == 1)
                    {
                        parentCurrentApprover.Remove("(" + username + ")");
                    }

                    var currentApprover = (documentApproval.CurrentApprover ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var newApprover = parentCurrentApprover.Union(currentApprover);

                    parentDocumentApproval.CurrentApprover = string.Join(",", newApprover);
                    if (parentDocumentApproval.CurrentApprover != null)
                    {
                        DomainEventManager.Raise(new DocumentApprovalEvent(parentDocumentApproval.DocumentStatusCode, parentDocumentApproval, actualOrganizationStructure, null));
                    }
                }
            }
        }

        /// <summary>
        /// Function that manage document approval workflow asynchronously
        /// </summary>
        /// <param name="username">UserName</param>
        /// <param name="actualOrganizationStructure">Actual Organization Structure Object</param>
        /// <param name="eventName">Event Name (initiate|approve|reject|revise|cancel)</param>
        /// <param name="documentApprovalId">Document Approval Id</param>
        /// <param name="remarks">Remarks (if any)</param>
        /// <param name="refId">Reference Id (for revise only)</param>
        /// <param name="completeHandler">Complete handler (if any)</param>
        public async Task<DocumentApproval> PostAsync(string username, ActualOrganizationStructure actualOrganizationStructure, string eventName, Guid documentApprovalId, string remarks = null, Guid? refId = null, Action<DocumentApproval> completeHandler = null)
        {
            var now = DateTime.Now;

            var documentApproval = await DocumentApprovalRepository.Fetch()
                .Where(x => x.Id == documentApprovalId)
                .Include(x => x.Form)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            var eventNameLower = string.IsNullOrEmpty(eventName)
                ? (documentApproval.DocumentStatusCode == DocumentStatus.Revised ? ApprovalAction.Initiate : ApprovalAction.Approve)
                : eventName.ToLower();

            Assert.ThrowIf(!documentApproval.EnableDocumentAction && !ObjectHelper.IsIn(eventNameLower, ApprovalAction.Initiate, ApprovalAction.Cancel), "This document cannot be approve/reject/revise");
            //2023-02-13 | Roni | tambah kondisi termination agar tidak di approve di my task, harus masuk ke form nya dulu
            if (documentApproval.FormKey != "termination")
            {
                Assert.ThrowIf(!documentApproval.CanSubmit && ObjectHelper.IsIn(eventNameLower, ApprovalAction.Approve), "This document cannot be approve");
            }

            Assert.ThrowIf(ObjectHelper.IsIn(documentApproval.DocumentStatusCode, DocumentStatus.Completed, DocumentStatus.Cancelled, DocumentStatus.Rejected, DocumentStatus.Expired), $"Document status with number <b>{documentApproval.DocumentNumber}</b> already completed");
            Assert.ThrowIf(!(eventNameLower == ApprovalAction.Cancel && ObjectHelper.IsIn(actualOrganizationStructure.NoReg, documentApproval.CreatedBy, documentApproval.SubmitBy)) && !string.IsNullOrEmpty(documentApproval.CurrentApprover) && !documentApproval.CurrentApprover.Contains("(" + username + ")"), "You dont have permission to approve/reject this document");

            UnitOfWork.Transact((transaction) =>
            {
                var hasChildDocument = !documentApproval.EnableDocumentAction;
                DocumentApprovalHistoryRepository.Add(DocumentApprovalHistory.Create(documentApprovalId, eventNameLower, actualOrganizationStructure, remarks));

                if (documentApproval.DocumentStatusCode == DocumentStatus.Revised && eventNameLower == ApprovalAction.Approve)
                {
                    eventNameLower = ApprovalAction.Initiate;
                }

                if (eventNameLower == ApprovalAction.Initiate && documentApproval.Form.NeedApproval)
                {
                    documentApproval.DocumentStatusCode = DocumentStatus.InProgress;
                    documentApproval.SubmitBy = actualOrganizationStructure.NoReg;
                    documentApproval.SubmitOn = now;

                    var trackingApprovals = UnitOfWork.UspQuery<TrackingApprovalStoredEntity>(new
                    {
                        documentApprovalId,
                        username,
                        noreg = actualOrganizationStructure.NoReg,
                        postCode = actualOrganizationStructure.PostCode
                    }, transaction);

                    var approvers = trackingApprovals.Where(x => x.ApprovalLevel > 1);

                    if (approvers.IsEmpty())
                    {
                        throw new Exception("Failed to submit document, there is no approvers from given approval matrix data. Please contact administrator for more information.");
                    }

                    var minApprovalLevel = approvers.Min(x => x.ApprovalLevel);
                    var nextApprovers = trackingApprovals.Where(x => x.ApprovalLevel == minApprovalLevel);
                    var firstApprover = nextApprovers.FirstOrDefault();

                    var progress = (int)Math.Round(100 / (decimal)trackingApprovals.Select(x => x.ApprovalLevel).Distinct().Count());

                    if (!hasChildDocument)
                    {
                        documentApproval.Progress = progress;
                    }

                    //var nextApproverUsernames = "(" + string.Join("),(", UserRepository.Find(x => nextApprovers.Any(y => y.NoReg == x.NoReg)).Select(x => x.Username)) + ")";
                    var nextApproverNoRegs = nextApprovers?.Select(y => y.NoReg).ToList() ?? new List<string>();

                    string nextApproverUsernames = "";

                    if (nextApproverNoRegs.Any())
                    {
                        // Step 1: Get all users first
                        var allUsers = UserRepository.Fetch().Where(x => x.RowStatus).ToList();

                        // Step 2: Filter in memory
                        var usernames = allUsers
                            .Where(x => nextApproverNoRegs.Contains(x.NoReg))
                            .Select(x => x.Username)
                            .ToList();

                        nextApproverUsernames = "(" + string.Join("),(", usernames) + ")";
                    }
                    documentApproval.CurrentApprover = nextApproverUsernames;
                    documentApproval.CanSubmit = !firstApprover.MandatoryInput;

                    DocumentApprovalRepository.Fetch()
                        .Where(x => x.ParentId == documentApproval.Id)
                        .Update(x => new DocumentApproval
                        {
                            DocumentStatusCode = DocumentStatus.InProgress,
                            SubmitBy = actualOrganizationStructure.NoReg,
                            SubmitOn = now,
                            Progress = progress,
                            CurrentApprover = nextApproverUsernames
                        });

                    if (_submitHandlers.ContainsKey(documentApproval.Form.FormKey))
                    {
                        _submitHandlers[documentApproval.Form.FormKey](actualOrganizationStructure.NoReg, documentApproval);
                    }
                }
                else if (eventNameLower == ApprovalAction.Initiate && !documentApproval.Form.NeedApproval)
                {
                    documentApproval.DocumentStatusCode = DocumentStatus.Completed;
                    documentApproval.SubmitBy = actualOrganizationStructure.NoReg;
                    documentApproval.SubmitOn = now;
                    documentApproval.Progress = 100;

                    if (_submitHandlers.ContainsKey(documentApproval.Form.FormKey))
                    {
                        _submitHandlers[documentApproval.Form.FormKey](actualOrganizationStructure.NoReg, documentApproval);
                    }
                }
                else if (eventNameLower == ApprovalAction.Approve || eventNameLower == ApprovalAction.Complete)
                {
                    var group = TrackingApprovalRepository
                        .Fetch()
                        .AsNoTracking()
                        .Where(x => x.DocumentApprovalId == documentApprovalId && string.IsNullOrEmpty(x.ApprovalActionCode))
                        .ToList()
                        .GroupBy(x => x.ApprovalLevel)
                        .OrderBy(x => x.Key)
                        .FirstOrDefault();

                    var trackingApproval = group.FirstOrDefault(x => x.NoReg == actualOrganizationStructure.NoReg);

                    Assert.ThrowIf(trackingApproval == null, "You dont have permission to approve this document");

                    TrackingApprovalRepository.Fetch()
                        .Where(x => x.DocumentApprovalId == documentApprovalId && x.ApprovalLevel == trackingApproval.ApprovalLevel)
                        .Update(x => new TrackingApproval { ApprovalActionCode = eventNameLower, Remarks = remarks, ModifiedBy = actualOrganizationStructure.NoReg, ModifiedOn = now });

                    UnitOfWork.SaveChanges();

                    var trackingApprovals = TrackingApprovalRepository.Fetch()
                        .AsNoTracking()
                        .Where(x => x.DocumentApprovalId == documentApprovalId)
                        .ToList();

                    var minApprovalLevel = trackingApprovals.Where(x => x.ApprovalLevel > trackingApproval.ApprovalLevel)
                        .DefaultIfEmpty(new TrackingApproval())
                        .Min(x => x.ApprovalLevel);

                    var nextApprovers = trackingApprovals.Where(x => x.ApprovalLevel == minApprovalLevel);

                    var isComplete = nextApprovers == null || nextApprovers.Count() == 0;

                    var total = trackingApprovals
                        .Select(x => x.ApprovalLevel)
                        .Distinct()
                        .Count();

                    var totalDone = trackingApprovals
                        .Where(x => !string.IsNullOrEmpty(x.ApprovalActionCode))
                        .Select(x => x.ApprovalLevel)
                        .Distinct()
                        .Count();

                    var progress = isComplete
                        ? 100
                        : (int)Math.Round(100 * totalDone / (decimal)total);

                    if (!isComplete)
                    {
                        var firstApprover = nextApprovers.FirstOrDefault();
                        var approvalMatrixId = firstApprover.ApprovalMatrixId;
                        var approvalMatrix = ApprovalMatrixRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == approvalMatrixId);

                        documentApproval.CanSubmit = approvalMatrix != null ? !approvalMatrix.MandatoryInput : true;
                    }

                    documentApproval.Progress = progress;
                    documentApproval.DocumentStatusCode = !isComplete ? DocumentStatus.InProgress : DocumentStatus.Completed;
                    //documentApproval.CurrentApprover = !isComplete
                    //    ? "(" + string.Join("),(", UserRepository.Find(x => nextApprovers.Any(y => y.NoReg == x.NoReg)).Select(x => x.Username)) + ")"
                    //    : null;
                    var nextApproverNoRegs = nextApprovers
                        .Select(y => y.NoReg)
                        .ToList();

                    var usernames = UserRepository.Fetch()
                        .Where(x => x.RowStatus)
                        .AsEnumerable() // stop EF → lanjut LINQ di memory
                        .Where(x => nextApproverNoRegs.Contains(x.NoReg))
                        .Select(x => x.Username)
                        .ToList();

                    documentApproval.CurrentApprover = !isComplete
                        ? "(" + string.Join("),(", usernames) + ")"
                        : null;

                    UpdateParentDocument(username, isComplete, documentApproval, actualOrganizationStructure);
                }
                else if (eventNameLower == ApprovalAction.Reject)
                {
                    var group = TrackingApprovalRepository
                        .Fetch()
                        .AsNoTracking()
                        .Where(x => x.DocumentApprovalId == documentApprovalId && string.IsNullOrEmpty(x.ApprovalActionCode))
                        .ToList()
                        .GroupBy(x => x.ApprovalLevel)
                        .OrderBy(x => x.Key)
                        .FirstOrDefault();

                    var trackingApproval = group.FirstOrDefault(x => x.NoReg == actualOrganizationStructure.NoReg);

                    Assert.ThrowIf(trackingApproval == null, "You dont have permission to reject this document");

                    TrackingApprovalRepository.Fetch()
                        .Where(x => x.DocumentApprovalId == documentApprovalId && x.ApprovalLevel == trackingApproval.ApprovalLevel)
                        .Update(x => new TrackingApproval { Remarks = remarks, ApprovalActionCode = eventNameLower, ModifiedBy = actualOrganizationStructure.NoReg, ModifiedOn = now });

                    documentApproval.DocumentStatusCode = DocumentStatus.Rejected;
                    documentApproval.CurrentApprover = null;


                    UpdateParentDocument(username, true, documentApproval, actualOrganizationStructure);
                }
                else if (eventNameLower == ApprovalAction.Revise)
                {
                    if (refId == null)
                    {
                        var submitter = UserRepository.Fetch()
                            .AsNoTracking()
                            .Where(x => x.NoReg == documentApproval.SubmitBy)
                            .FirstOrDefaultIfEmpty();

                        TrackingApprovalRepository.Fetch()
                            .Where(x => x.DocumentApprovalId == documentApprovalId)
                            .Update(x => new TrackingApproval { ApprovalActionCode = null, Remarks = null, ModifiedBy = null, ModifiedOn = null });

                        documentApproval.CanSubmit = true;
                        if (documentApproval.FormKey == "termination")
                        {
                            if (documentApproval.Title.Contains("Resignation"))
                            {
                                documentApproval.CurrentApprover = "(" + submitter.Username + ")";
                            }
                            else
                            {
                                documentApproval.CurrentApprover = documentApproval.CurrentApprover;
                            }
                        }
                        else
                        {
                            documentApproval.CurrentApprover = "(" + submitter.Username + ")";
                        }
                        documentApproval.Progress = 0;
                    }

                    documentApproval.DocumentStatusCode = DocumentStatus.Revised;
                }
                else if (eventNameLower == ApprovalAction.Cancel)
                {
                    documentApproval.CurrentApprover = null;
                    documentApproval.DocumentStatusCode = DocumentStatus.Cancelled;

                    DocumentApprovalRepository.Fetch()
                        .Where(x => x.ParentId == documentApproval.Id && x.RowStatus && (x.DocumentStatusCode == DocumentStatus.InProgress || x.DocumentStatusCode == DocumentStatus.Draft || x.DocumentStatusCode == DocumentStatus.Revised))
                        .Update(x => new DocumentApproval
                        {
                            DocumentStatusCode = DocumentStatus.Cancelled,
                            CurrentApprover = null,
                            LastApprovedBy = actualOrganizationStructure.NoReg,
                            LastApprovedOn = now
                        });

                    if (hasChildDocument)
                    {
                        TrackingApprovalRepository.Fetch()
                            .Where(x => x.ApprovalLevel == 1 && DocumentApprovalRepository.Fetch().AsNoTracking().Any(y => y.Id == x.DocumentApprovalId && y.ParentId == documentApproval.Id && y.RowStatus && y.DocumentStatusCode == DocumentStatus.Cancelled))
                            .Update(x => new TrackingApproval { Remarks = remarks, ApprovalActionCode = eventNameLower, ModifiedBy = actualOrganizationStructure.NoReg, ModifiedOn = now });
                    }

                    var initiator = TrackingApprovalRepository.Fetch()
                            .Where(x => x.DocumentApprovalId == documentApprovalId && x.ApprovalLevel == 1)
                            .FirstOrDefault();

                    if (initiator != null)
                    {
                        initiator.Remarks = remarks;
                        initiator.ApprovalActionCode = eventNameLower;
                    }
                }

                if (eventNameLower != ApprovalAction.Initiate)
                {
                    documentApproval.LastApprovedBy = actualOrganizationStructure.NoReg;
                    documentApproval.LastApprovedOn = now;
                }

                UnitOfWork.SaveChanges();

                if (eventNameLower == ApprovalAction.Reject && _rejectHandlers.ContainsKey(documentApproval.Form.FormKey))
                {
                    _rejectHandlers[documentApproval.Form.FormKey](actualOrganizationStructure.NoReg, documentApproval);
                }
                else if (eventNameLower == ApprovalAction.Revise && _reviseHandlers.ContainsKey(documentApproval.Form.FormKey))
                {
                    _reviseHandlers[documentApproval.Form.FormKey](actualOrganizationStructure.NoReg, documentApproval);
                }
                else if (eventNameLower == ApprovalAction.Approve && _approveHandlers.ContainsKey(documentApproval.Form.FormKey))
                {
                    _approveHandlers[documentApproval.Form.FormKey](actualOrganizationStructure.NoReg, documentApproval);
                }
                else if (eventNameLower == ApprovalAction.Cancel && _cancelHandlers.ContainsKey(documentApproval.Form.FormKey))
                {
                    _cancelHandlers[documentApproval.Form.FormKey](actualOrganizationStructure.NoReg, documentApproval);
                }

                if ((documentApproval.DocumentStatusCode == DocumentStatus.Completed || eventNameLower == ApprovalAction.Complete) && _completeHandlers.ContainsKey(documentApproval.Form.FormKey))
                {
                    _completeHandlers[documentApproval.Form.FormKey](actualOrganizationStructure.NoReg, documentApproval);
                }
            });

            var needToRaiseEvent = !documentApproval.ParentId.HasValue;

            if (needToRaiseEvent)
            {
                DomainEventManager.Raise(new DocumentApprovalEvent(eventNameLower, documentApproval, actualOrganizationStructure, remarks));
            }

            return documentApproval;
        }
        #endregion

        public void CreateChangeTracking(string noreg, Guid documentApprovalId, string fieldName, string formattedValue)
        {
            DocumentApprovalChangeTrackingRepository.Add(new DocumentApprovalChangeTracking
            {
                DocumentApprovalId = documentApprovalId,
                FieldName = fieldName,
                FormattedValue = formattedValue,
                CreatedBy = noreg
            });

            UnitOfWork.SaveChanges();
        }

        public async Task<IEnumerable<DocumentApprovalChangeTracking>> GetChangeTrackers(Guid documentApprovalId)
        {
            var output = from t in DocumentApprovalChangeTrackingRepository.Fetch().AsNoTracking()
                         join u in UserRepository.Fetch().AsNoTracking() on t.CreatedBy equals u.NoReg
                         where t.DocumentApprovalId == documentApprovalId
                         select DocumentApprovalChangeTracking.Create(t, u);

            return await output.ToListAsync();
        }

        public bool NotifLongLeave(EmployeeLeaveViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@noreg", model.noreg }
                    };

                    UnitOfWork.UspQuery("SP_GET_LONG_LEAVE_NOTIF", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

    }
}
