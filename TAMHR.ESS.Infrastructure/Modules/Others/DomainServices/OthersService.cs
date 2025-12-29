using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainEvents;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;
using System.Globalization;
using TAMHR.ESS.Infrastructure.Responses;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle others module.
    /// </summary>
    public class OthersService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Document request detail repository object.
        /// </summary>
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();

        /// <summary>
        /// Document approval repository object.
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public OthersService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Health Declaration Form
        public bool MarkedByDoctor(string noreg, DateTime keyDate)
        {
            var set = UnitOfWork.GetRepository<FormLog>();

            return set.Fetch()
                .AsNoTracking()
                .Any(x => x.NoReg == noreg && !x.Closed && x.RowStatus && keyDate >= x.CreatedOn.Date);
        }

        /// <summary>
        /// Get list of health declaration summaries by division from given submission date.
        /// </summary>
        /// <param name="keyDate">This key (submission) date.</param>
        /// <returns>This list of <see cref="HealthDeclarationSummaryStoredEntity"/> objects.</returns>
        public IEnumerable<HealthDeclarationSummaryStoredEntity> GetHealthDeclarationSummaries(DateTime keyDate)
        {
            // Get list of health declaration summaries from given submission date without object tracking.
            return UnitOfWork.UdfQuery<HealthDeclarationSummaryStoredEntity>(new { keyDate })
                .OrderBy(x => x.Division);
        }

        /// <summary>
        /// Get list of latest form answers view model by noreg.
        /// </summary>
        /// <param name="noreg">This noreg.</param>
        /// <returns>This list of <see cref="FormAnswerViewModel"/> objects.</returns>
        public FormAnswerViewModel[] GetLatestFormAnswerViewModels(string noreg)
        {
            // Get latest submitted health declaration document from given noreg.
            var latestSubmittedHealthDeclaration = GetLatestSubmittedHealthDeclaration(noreg);

            // If the latest document is not exist then return empty list.
            if (latestSubmittedHealthDeclaration == null) return Array.Empty<FormAnswerViewModel>();

            // Create document request detail repository object.
            var documentRequestDetailSet = UnitOfWork.GetRepository<DocumentRequestDetail>();

            // Get and set document request detail by document approval id.
            var documentRequestDetail = documentRequestDetailSet.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.DocumentApprovalId == latestSubmittedHealthDeclaration.ReferenceDocumentApprovalId);

            // Deserialize the JSON value.
            var viewModel = JsonConvert.DeserializeObject<HealthDeclarationViewModel>(documentRequestDetail?.ObjectValue ?? string.Empty);

            // Return the list of form answers.
            return viewModel == null
                ? Array.Empty<FormAnswerViewModel>()
                : viewModel.FormAnswers;
        }

        /// <summary>
        /// Get latest submitted health declaration document by noreg.
        /// </summary>
        /// <param name="noreg">This noreg.</param>
        /// <returns>This <see cref="HealthDeclaration"/> object.</returns>
        public HealthDeclaration GetLatestSubmittedHealthDeclaration(string noreg)
        {
            // Get and set health declaration repository object.
            var set = UnitOfWork.GetRepository<HealthDeclaration>();

            return set.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg)
                .OrderByDescending(x => x.SubmissionDate)
                .FirstOrDefault();
        }

        /// <summary>
        /// Marked health declaration submission document as no need to be monitored.
        /// </summary>
        /// <param name="documentApprovalId">This document approval id.</param>
        /// <param name="marked">Marker flag (if current submission document was to be monitoring then if the value is true its mean no need to be monitored, false otherwise).</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool MarkedHealthDeclaration(Guid documentApprovalId, bool marked)
        {
            // Get and set health declaration repository object.
            var set = UnitOfWork.GetRepository<HealthDeclaration>();

            var healthDeclaration = set.Fetch()
                .FirstOrDefault(x => x.ReferenceDocumentApprovalId == documentApprovalId);

            healthDeclaration.Marked = marked;

            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Get list of health declarations master data by noreg.
        /// </summary>
        /// <param name="noreg">This noreg.</param>
        /// <returns>This list of <see cref="HealthDeclarationHistoryResponse"/> object.</returns>
        public IQueryable<HealthDeclarationHistoryResponse> GetHealthDeclarations(string noreg)
        {
            // Get and set health declaration repository object.
            var set = UnitOfWork.GetRepository<HealthDeclaration>();

            // Get and set document approval repository object.
            var documentApprovalSet = UnitOfWork.GetRepository<DocumentApproval>();

            var query = from hd in set.Fetch().AsNoTracking()
                        join da in documentApprovalSet.Fetch().AsNoTracking() on hd.ReferenceDocumentApprovalId equals da.Id
                        where hd.NoReg == noreg && hd.RowStatus && da.RowStatus
                        select new HealthDeclarationHistoryResponse
                        {
                            Id = hd.Id,
                            ReferenceDocumentApprovalId = hd.ReferenceDocumentApprovalId,
                            DocumentNumber = da.DocumentNumber,
                            SubmissionDate = hd.SubmissionDate,
                            IsSick = hd.IsSick,
                            HaveFever = hd.HaveFever,
                            Remarks = hd.Remarks
                        };

            return query;
        }

        /// <summary>
        /// Get health declaration master data by document approval id.
        /// </summary>
        /// <param name="documentApprovalId">This document approval id.</param>
        /// <returns>This <see cref="HealthDeclaration"/> object.</returns>
        public HealthDeclaration GetHealthDeclaration(Guid documentApprovalId)
        {
            // Get and set health declaration repository object.
            var set = UnitOfWork.GetRepository<HealthDeclaration>();

            return set.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.ReferenceDocumentApprovalId == documentApprovalId);
        }

        /// <summary>
        /// Get health declaration master data by noreg and date.
        /// </summary>
        /// <param name="noreg">This noreg.</param>
        /// <param name="date">This submission date.</param>
        /// <returns>This <see cref="HealthDeclaration"/> object.</returns>
        public HealthDeclaration GetHealthDeclaration(string noreg, DateTime date)
        {
            // Get and set health declaration repository object.
            var set = UnitOfWork.GetRepository<HealthDeclaration>();

            return set.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.NoReg == noreg && x.SubmissionDate == date);
        }

        /// <summary>
        /// Check whether health declaration document with given noreg and date need to be monitoring or not.
        /// </summary>
        /// <param name="noreg">This noreg.</param>
        /// <param name="keyDate">This submission date.</param>
        /// <returns>True if need to be monitoring, false otherwise.</returns>
        public bool NeedToBeMonitoring(string noreg, DateTime keyDate)
        {
            // Get and set health declaration repository object.
            var set = UnitOfWork.GetRepository<HealthDeclaration>();

            return set.Fetch()
                .Any(x => x.NoReg == noreg && x.SubmissionDate == keyDate && x.IsSick);
        }

        /// <summary>
        /// Validate health declaration form (only one submission per day).
        /// </summary>
        /// <param name="noreg">This noreg.</param>
        public void PreValidateHealthDeclaration(string noreg)
        {
            // Get current date.
            var now = DateTime.Now.Date;

            // Get and set health declaration repository object.
            var set = UnitOfWork.GetRepository<HealthDeclaration>();

            // Determine whether user with given noreg has submitted the form or not.
            var hasSubmitted = set.Fetch()
                .Any(x => x.NoReg == noreg && x.SubmissionDate == now && x.RowStatus);

            // Throw an exception if any.
            Assert.ThrowIf(hasSubmitted, "You can only submit once per day");
        }

        /// <summary>
        /// Auto approve when health declaration form has been submitted.
        /// </summary>
        /// <param name="noreg">This noreg.</param>
        /// <param name="documentApproval">This <see cref="DocumentApproval"/> object.</param>
        public void SubmitHealthDeclaration(string noreg, DocumentApproval documentApproval)
        {
            // Get current date & time.
            var now = DateTime.Now;

            // Create tracking approval repository object.
            var trackingApprovalSet = UnitOfWork.GetRepository<TrackingApproval>();

            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Update document approval progress to 100.
                documentApproval.Progress = 100;

                // Update document status code to completed.
                documentApproval.DocumentStatusCode = "completed";

                // Clear current approver.
                documentApproval.CurrentApprover = null;

                // Update tracking approval status to approve from given document approval id.
                trackingApprovalSet.Fetch()
                    .Where(x => x.DocumentApprovalId == documentApproval.Id && x.ApprovalActionCode == null)
                    .Update(x => new TrackingApproval
                    {
                        // Get and set remarks to auto approve.
                        Remarks = "Acknowledge",
                        // Get and set action code to acknowledge.
                        ApprovalActionCode = "acknowledge",
                        // Update ModifiedBy with given noreg.
                        ModifiedBy = noreg,
                        // Update ModifiedOn with current date & time.
                        ModifiedOn = now
                    });
            });
        }

        /// <summary>
        /// Complete health declaration submission when the document has been fully approved.
        /// </summary>
        /// <param name="noreg">This noreg.</param>
        /// <param name="documentApproval">This <see cref="DocumentApproval"/> object.</param>
        public void CompleteHealthDeclaration(string noreg, DocumentApproval documentApproval)
        {
            // Get current date & time.
            var now = DateTime.Now.Date;

            var configRepository = UnitOfWork.GetRepository<Config>();

            var minOffsetConfig = configRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.ConfigKey == "Health.MinOffset" && x.RowStatus);

            decimal minOffset = 0;

            if (minOffsetConfig != null)
            {
                decimal.TryParse(minOffsetConfig.ConfigValue, out minOffset);
            }

            if (minOffset == 0)
            {
                minOffset = (decimal)37.3;
            }

            // Create health declaration repository object.
            var healthDeclarationSet = UnitOfWork.GetRepository<HealthDeclaration>();

            // Create proxy time repository object.
            //var proxyTimeSet = UnitOfWork.GetRepository<ProxyTime>();

            // Create form question group answer repository object.
            var formQuestionGroupAnswerSet = UnitOfWork.GetRepository<FormQuestionGroupAnswer>();

            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Get document request detail by document approval id.
                var documentRequestDetail = DocumentRequestDetailRepository.Find(x => x.DocumentApprovalId == documentApproval.Id)
                    .FirstOrDefaultIfEmpty();

                // Get the view model from given JSON string value.
                var viewModel = JsonConvert.DeserializeObject<HealthDeclarationViewModel>(documentRequestDetail.ObjectValue);

                // Get health declaration from given noreg and submission date.
                var healthDeclaration = healthDeclarationSet.Fetch()
                    .FirstOrDefault(x => x.NoReg == noreg && x.SubmissionDate == now && x.RowStatus);

                // If health declaration is null then push to the master data table.
                if (healthDeclaration != null) return;

                var groups = formQuestionGroupAnswerSet.Fetch()
                    .Include(x => x.FormQuestion)
                    .Where(x => x.GroupAnswer == "SickGroup")
                    .ToList();

                var match = groups
                    .Join(viewModel.FormAnswers, x => x.FormQuestionId, y => y.FormQuestionId, (data, y) => new { data.FormQuestion, Value1 = data.Value, Value2 = y.Value })
                    .Where(x => x.Value1 == x.Value2);

                var types = match.Select(x => x.FormQuestion.Title).ToList();
                
                var temperatureOffset = viewModel.BodyTemperature == "others" && decimal.Parse(viewModel.BodyTemperatureOtherValue.Replace(",", "."), new CultureInfo("en-US")) >= minOffset;
                
                var isSick = match.Any() || viewModel.HaveFever.Value || temperatureOffset;

                if (temperatureOffset)
                {
                    types.Add("Temperature >= " + minOffset + " C");
                }

                if (viewModel.HaveFever.Value)
                {
                    types.Add("Has Fever");
                }

                var remarks = string.Join(", ", types.OrderBy(x => x));

                // Create new health declaration object and insert it into repository object.
                healthDeclarationSet.Add(new HealthDeclaration
                {
                    // Get and set reference document approval id.
                    ReferenceDocumentApprovalId = documentApproval.Id,
                    // Get and set noreg.
                    NoReg = noreg,
                    // Get and set submission date.
                    SubmissionDate = now,
                    // Get and set phone number.
                    PhoneNumber = viewModel.PhoneNumber,
                    // Get and set email.
                    Email = viewModel.Email,
                    // Get and set EmergencyFamilyStatus.
                    EmergencyFamilyStatus = viewModel.EmergencyFamilyStatus,
                    // Get and set EmergencyName.
                    EmergencyName = viewModel.EmergencyName,
                    // Get and set EmergencyPhoneNumber.
                    EmergencyPhoneNumber = viewModel.EmergencyPhoneNumber,
                    // Get and set working type code.
                    WorkTypeCode = viewModel.WorkTypeCode,
                    // Get and set body temperature.
                    BodyTemperature = viewModel.BodyTemperature,
                    // Get and set body temperature other value.
                    BodyTemperatureOtherValue = viewModel.BodyTemperatureOtherValue,
                    // Get and set HaveFever indicator.
                    HaveFever = viewModel.HaveFever ?? false,
                    // Get and set remarks.
                    Remarks = remarks,
                    // Get and set IsSick indicator.
                    IsSick = isSick,
                    // Marked as true when not sick.
                    Marked = !isSick
                });

                //if (isSick || viewModel.WorkTypeCode == "wt-wfh")
                //{
                //    proxyTimeSet.Add(new ProxyTime
                //    {
                //        NoReg = noreg,
                //        WorkingDate = now,
                //        WorkingTypeCode = "wt-wfh"
                //    });
                //}

                UnitOfWork.SaveChanges();
            });
        }
        #endregion

        #region Reference Letter Form
        /// <summary>
        /// Complete reference letter submission when the document has been fully approved.
        /// </summary>
        /// <param name="noreg">This noreg.</param>
        /// <param name="documentApprovalId">This document approval id.</param>
        public void CompleteReferenceLetter(string noreg, Guid documentApprovalId)
        {
            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Get and set document request detail by document approval id.
                var requestDetail = DocumentRequestDetailRepository.Find(x => x.DocumentApprovalId == documentApprovalId)
                    .FirstOrDefaultIfEmpty();

                // Update ModifiedBy to given noreg.
                requestDetail.ModifiedBy = noreg;

                // Push pending changes into database.
                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Update reference letter sequence number by document approval id.
        /// </summary>
        /// <param name="documentApprovalId">This document approval id.</param>
        /// <param name="sequenceNumber">This generated sequence number.</param>
        public void UpdateReferenceLetterSequenceNumber(Guid documentApprovalId, int sequenceNumber)
        {
            // Create new config service object.
            var configService = new ConfigService(UnitOfWork);

            // Get reference letter document format from configuration.
            var referenceLetterDocumentNumberFormat = configService.GetConfig("ReferenceLetter.DocumentFormat")?.ConfigValue;

            // Get and set current date and time.
            var now = DateTime.Now;

            // Create new dictionary object.
            var dicts = new Dictionary<string, object>
            {
                // Get and set current day.
                { "day", now.Day },
                // Get and set current month.
                { "month", now.Month },
                // Get and set current year.
                { "year", now.Year },
                // Get and set sequence number.
                { "sequenceNumber", sequenceNumber }
            };

            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Get and set document request detail by document approval id.
                var requestDetail = DocumentRequestDetailRepository.Find(x => x.DocumentApprovalId == documentApprovalId)
                    .FirstOrDefaultIfEmpty();

                // Get and set reference letter view model from document request detail JSON value.
                var detail = JsonConvert.DeserializeObject<ReferenceLetterViewModel>(requestDetail.ObjectValue);

                // Update reference letter number.
                detail.ReferenceLetterNumber = StringHelper.Format(referenceLetterDocumentNumberFormat, dicts);

                // Update JSON value.
                requestDetail.ObjectValue = JsonConvert.SerializeObject(detail);

                // Commit transaction.
                UnitOfWork.SaveChanges();
            });
        }
        #endregion

        #region Complaint Request Form
        /// <summary>
        /// Update complaint request solutions.
        /// </summary>
        /// <param name="viewModel">This <see cref="ComplaintRequestSolutionViewModel"/> object.</param>
        public void UpdateComplaintRequestSolutions(ComplaintRequestSolutionViewModel viewModel)
        {
            // Get and set document request detail by document approval id.
            var requestDetail = DocumentRequestDetailRepository.Find(x => x.Id == viewModel.Id)
                .FirstOrDefaultIfEmpty();

            // Get and set complaint request view model from document request detail JSON value.
            var objects = JsonConvert.DeserializeObject<ComplaintRequestViewModel[]>(requestDetail.ObjectValue);

            Assert.ThrowIf(viewModel.Solutions == null || viewModel.Solutions.Length != objects.Length || viewModel.Solutions.Any(x => string.IsNullOrEmpty(x)), "Solution cannot be empty");

            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Enumerate through view model solutions.
                for (var i = 0; i < viewModel.Solutions.Length; i++)
                {
                    // Update the solution from given view model.
                    objects[i].Solution = viewModel.Solutions[i];
                }

                // Update JSON value.
                requestDetail.ObjectValue = JsonConvert.SerializeObject(objects);

                // Get and set document approval by document approval id.
                var documentApproval = DocumentApprovalRepository.FindById(requestDetail.DocumentApprovalId);

                // Update can submit flag to true.
                documentApproval.CanSubmit = true;

                // Commit transaction;
                UnitOfWork.SaveChanges();
            });
        }
        #endregion

    }
}
