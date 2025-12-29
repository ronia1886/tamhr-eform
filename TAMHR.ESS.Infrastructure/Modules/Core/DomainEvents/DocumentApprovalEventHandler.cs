using System;
using System.Linq;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Common;
using Agit.Domain.Event;
using Agit.Common.Utility;
using Scriban;
using TAMHR.ESS.Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace TAMHR.ESS.Infrastructure.DomainEvents
{
    /// <summary>
    /// Document approval event handler class
    /// </summary>
    public class DocumentApprovalEventHandler : IDomainEventHandler<DocumentApprovalEvent>
    {
        #region Domain Services
        /// <summary>
        /// Core service object
        /// </summary>
        protected readonly CoreService CoreService;

        /// <summary>
        /// Config service object
        /// </summary>
        protected readonly ConfigService ConfigService;

        /// <summary>
        /// MDM service object
        /// </summary>
        protected readonly MdmService MdmService;

        /// <summary>
        /// User service object
        /// </summary>
        protected readonly UserService UserService;

        /// <summary>
        /// Email service object
        /// </summary>
        protected readonly EmailService EmailService;

        protected readonly TerminationService TerminationService;
        protected readonly LogService LogService;

        #endregion

        #region Variables & Properties
        private readonly Dictionary<string, Action<DocumentApprovalEvent>> _handlers = new Dictionary<string, Action<DocumentApprovalEvent>>();
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="coreService">Core Service</param>
        /// <param name="configService">Config Service</param>
        /// <param name="userService">User Service</param>
        /// <param name="mdmService">MDM Service</param>
        public DocumentApprovalEventHandler(
            CoreService coreService,
            ConfigService configService,
            UserService userService,
            MdmService mdmService,
            EmailService emailService,
            TerminationService terminationService)
        {
            CoreService = coreService;
            ConfigService = configService;
            UserService = userService;
            MdmService = mdmService;
            EmailService = emailService;
            TerminationService = terminationService;

            _handlers.Add("health-declaration", HandleHealthDeclaration);
        }
        #endregion

        /// <summary>
        /// Handle approval event
        /// </summary>
        /// <param name="documentApprovalEvent">Document Approval Event Object</param>
        public void Handle(DocumentApprovalEvent documentApprovalEvent)
        {
            var formKey = documentApprovalEvent.DocumentApproval?.FormKey;

            var now = DateTime.Now;
            var documentApproval = documentApprovalEvent.DocumentApproval;
            var actualOrganizationStructure = documentApprovalEvent.ActualOrganizationStructure;
            var isComplete = documentApproval.DocumentStatusCode == DocumentStatus.Completed;
            var isCancel = documentApproval.DocumentStatusCode == DocumentStatus.Cancelled;
            var isInitiate = documentApprovalEvent.ActionCode == ApprovalAction.Initiate;
            var actionCode = isComplete ? ApprovalAction.Complete : documentApprovalEvent.ActionCode;
            

            //Create notification
            if (!(actualOrganizationStructure.NoReg == documentApproval.CreatedBy && ObjectHelper.IsIn(actionCode, ApprovalAction.Initiate, ApprovalAction.Cancel)))
            {
                var fromNoReg = actualOrganizationStructure.NoReg;
                var submitBy = documentApproval.SubmitBy;
                var toNoReg = documentApproval.CreatedBy;
                var message = $"Document with number <a data-trigger='handler' data-handler='redirectHandler' data-url='~/core/form/view?formKey={documentApproval.Form.FormKey}&id={documentApproval.Id}'><b>{documentApproval.DocumentNumber}</b></a> has been {(documentApproval.DocumentStatusCode == "inprogress" ? "approved" : documentApproval.DocumentStatusCode)} by {actualOrganizationStructure.Name}";

                CoreService.CreateNotification(Notification.Create(fromNoReg, toNoReg, message));

                if (documentApproval.CreatedBy != documentApproval.SubmitBy && !string.IsNullOrEmpty(documentApproval.SubmitBy))
                {
                    CoreService.CreateNotification(Notification.Create(fromNoReg, submitBy, message));
                }
            }

            if (_handlers.ContainsKey(formKey))
            {
                _handlers[formKey](documentApprovalEvent);

                return;
            }

            //Create mail queue
            var templateKey = $"{actionCode}-approval-template";

            //2022-04-03 | tambah kondisi termination
            if (documentApproval.FormKey == "termination") { 
                templateKey = $"{actionCode}-termination-approval-template";
            }
            
            
            var emailTemplate = CoreService.GetEmailTemplate(templateKey);
            var formTemplateKey = $"{actionCode}-{documentApproval.FormKey}-template";
            var formEmailTemplate = CoreService.GetEmailTemplate(formTemplateKey);

            if (formEmailTemplate != null)
            {
                emailTemplate = formEmailTemplate;
            }

            Assert.ThrowIf(emailTemplate == null, $"Email template with key '{templateKey}' is not registered");

            var instanceKey = $"{documentApproval.FormKey}.{documentApproval.Id}";
            var mailSubject = emailTemplate.Subject;
            var mailFrom = emailTemplate.MailFrom;
            var recipients = new List<string>();
            var names = new List<string>();
            var cc = new List<string>();
            var bcc = new List<string>();

            if (isComplete)
            {
                if (documentApproval.FormKey == "absence")
                {
                    var manager = MdmService.GetAllChief(documentApproval.CreatedBy, 2);
                    var emails = manager.Select(x => x.Email);

                    if (emails != null && emails.Count() > 0)
                    {
                        cc.AddRange(emails);

                        CoreService.CreateDocumentApprovalHistory(manager.Select(x => EmployeeAllChiefStoredEntity.CreateHistory(documentApproval.Id, ApprovalAction.Acknowledge, x)));
                    }
                }
            }

            if (isCancel && ObjectHelper.IsIn(documentApproval.FormKey, "cop", "cpp"))
            {
                var emails = CoreService.GetEmailsFromTrackingApprovals(documentApproval.Id, new[] { documentApproval.SubmitBy }, true);

                names.Add("All");
                recipients.AddRange(emails);
            }

            if (isInitiate)
            {
                if (documentApproval.FormKey == "maternity-leave")
                {
                    var emails = CoreService.GetEmailsFromTrackingApprovals(documentApproval.Id, new[] { documentApproval.SubmitBy });

                    cc.AddRange(emails);
                }
            }

            if (documentApproval.DocumentStatusCode == DocumentStatus.InProgress)
            {
                var currentApproverEmails = CoreService.GetEmails(documentApproval.CurrentApprover, "username");
                names.AddRange(currentApproverEmails.Select(x => x.Name));
                recipients.AddRange(currentApproverEmails.Select(x => x.Email));
            }

            if (ObjectHelper.IsIn(documentApproval.DocumentStatusCode, DocumentStatus.Rejected, DocumentStatus.Completed, DocumentStatus.Revised) && documentApproval.FormKey!= "weekly-wfh-planning")
            {
                var owners = new[] { documentApproval.CreatedBy, documentApproval.SubmitBy };
                var ownerEmails = CoreService.GetEmails("(" + string.Join("),(", owners) + ")", "noreg");
                names.AddRange(ownerEmails.Select(x => x.Name));

                recipients.AddRange(ownerEmails.Select(x => x.Email));
            }

            if (recipients.Count > 0)
            {
                var module = ConfigService.GetGeneralCategory(documentApproval.Form.ModuleCode);
                var submitter = UserService.GetByNoReg(documentApproval.SubmitBy);
                var appUrl = ConfigService.GetConfig("Application.Url")?.ConfigValue;
                var url = $"{appUrl}/core/form/view?formKey={documentApproval.Form.FormKey}&id={documentApproval.Id}";
                var template = Template.Parse(emailTemplate.MailContent);

                string mailContent = "";
                if (documentApproval.FormKey == "termination")
                {
                    var terminationData = TerminationService.GetTermination(documentApproval.Id);
                    var generalData = ConfigService.GetGeneralCategory(terminationData.TerminationTypeId);
                    var userTermination = UserService.GetByNoReg(terminationData.NoReg);
                    var data = new
                    {
                        document_type = generalData.Name,
                        documentApproval.DocumentNumber,
                        Url = url,
                        employee_name = userTermination.Name,
                        Names = string.Join(", ", names),
                        now.Year,
                        LastSubmitterName = actualOrganizationStructure.Name,
                        documentApprovalEvent.Remarks,
                    };

                    mailContent = template.Render(data);
                }
                else
                {
                    var data = new
                    {
                        Module = module?.Name,
                        FormTitle = documentApproval.Form.Title,
                        documentApproval.DocumentNumber,
                        documentApprovalEvent.Remarks,
                        Url = url,
                        InitiatorName = submitter.Name,
                        LastSubmitterName = actualOrganizationStructure.Name,
                        Names = string.Join(", ", names),
                        now.Year
                    };

                    mailContent = template.Render(data);
                }



                var mailer = EmailService.CreateEmailManager();
                var mailQueue = MailQueue.Create(instanceKey, mailSubject, mailContent, recipients, mailFrom, cc, bcc);

                try
                {
                    mailer.Send(mailQueue.MailFrom, mailQueue.MailSubject, mailQueue.MailContent, mailQueue.Recipients, mailQueue.CC, mailQueue.Bcc);
                    mailQueue.MailStatusCode = MailStatus.Sent;
                }
                catch (Exception ex)
                {
                    mailQueue.ExceptionMessage = ex.Message ?? ex.InnerException?.Message;

                }
                CoreService.CreateMailQueue(mailQueue);

                try
                {
                    var selfTemplate = $"initiate-self-{documentApproval.FormKey}-template";
                    if (isInitiate && CoreService.HasEmailTemplate(selfTemplate))
                    {
                        var initTemplate = CoreService.GetEmailTemplate(selfTemplate);
                        var owners = new[] { documentApproval.CreatedBy, documentApproval.SubmitBy };
                        var ownerEmails = CoreService.GetEmails("(" + string.Join("),(", owners) + ")", "noreg");

                        var dataObject = new
                        {
                            documentApprovalEvent.Remarks,
                            Url = url,
                            Names = string.Join(", ", ownerEmails.Select(x => x.Name)),
                            now.Year
                        };

                        SendMail(instanceKey,mailer, initTemplate, dataObject, string.Join(",", ownerEmails.Select(x => x.Email)), string.Empty, string.Empty);
                    }
                }
                catch
                {
                }

                //2022-04-03 | Roni | kirim initiate email ke karyawan yang termination
                if (documentApproval.FormKey == "termination")
                {
                    try
                    {
                        var termTemplate = $"initiate-termination-approval-template";
                        var terminationData = TerminationService.GetTermination(documentApproval.Id);
                        //jika yang membuat adalah HR, maka kirim ke karyawan yang termination
                        if (isInitiate && CoreService.HasEmailTemplate(termTemplate) && terminationData.NoReg!=terminationData.CreatedBy) 
                        {
                            var generalData = ConfigService.GetGeneralCategory(terminationData.TerminationTypeId);
                            var userTermination = UserService.GetByNoReg(terminationData.NoReg);

                            List<string> termEmails = new List<string>();
                            termEmails.Add(userTermination.Email);

                            var initTemplate = CoreService.GetEmailTemplate(termTemplate);

                            var dataObject = new
                            {
                                document_type = generalData.Name,
                                document_number = documentApproval.DocumentNumber,
                                employee_name = userTermination.Name,
                                Url = url,
                                Names = userTermination.Name,
                                now.Year
                            };
                            //----perubahan tidak mengirim yang contrak ended by : alfikri 09/10/2023
                            if (terminationData.Reason != "Contract Ended"){
                                SendMail(instanceKey, mailer, initTemplate, dataObject, string.Join(",", termEmails), string.Empty, string.Empty);
                            }
                            //SendMail(instanceKey, mailer, initTemplate, dataObject, string.Join(",", termEmails), string.Empty, string.Empty);
                        }
                    }
                    catch
                    {
                    }
                }
                
            }
            
        }

        public void HandleHealthDeclaration(DocumentApprovalEvent documentApprovalEvent)
        {
            var commonData = GetCommonData(documentApprovalEvent);
            var healthDeclaration = CoreService.QueryFirstOrDefault<HealthDeclaration>(x => x.ReferenceDocumentApprovalId == documentApprovalEvent.DocumentApproval.Id);

            string cc = null;

            if (healthDeclaration.IsSick) {
                var ccTrackings = commonData.TrackingUsers.Where(x => x.ApprovalLevel > 2)
                    .Select(x => x.NoReg)
                    .ToArray();

                var ccUsers = UserService.GetByNoRegs(ccTrackings);

                cc = string.Join(",", ccUsers.Select(x => x.Email));
            }

            var data = new
            {
                commonData.Url,
                commonData.Name,
                NeedMonitoring = healthDeclaration.IsSick,
                WorkingType = healthDeclaration.WorkTypeCode
            };

            EmailService.SendEmail("health-declaration-complete", commonData.Email, data, cc);
        }

        #region Private Methods
        private void SendMail(string instanceKey,EmailManager emailManager, EmailTemplate emailTemplate, object data, string recipients, string ccs, string bccs)
        {
            var template = Template.Parse(emailTemplate.MailContent);
            var mailContent = template.Render(data);
            var mailQueue = MailQueue.Create(instanceKey, emailTemplate.Subject, mailContent, recipients.Split(','), emailTemplate.MailFrom, ccs.Split(','), bccs.Split(','));
            try
            {
                emailManager.Send(emailTemplate.MailFrom, emailTemplate.Subject, mailContent, recipients, ccs, bccs);
                mailQueue.MailStatusCode = MailStatus.Sent;
            }
            catch(Exception ex)
            {
                mailQueue.ExceptionMessage = ex.Message ?? ex.InnerException?.Message;
                
            }
            CoreService.CreateMailQueue(mailQueue);

        }

        private CommonData GetCommonData(DocumentApprovalEvent documentApprovalEvent)
        {
            var documentApproval = documentApprovalEvent.DocumentApproval;

            var submitter = UserService.GetByNoReg(documentApproval.SubmitBy);
            var appUrl = ConfigService.GetConfig("Application.Url")?.ConfigValue;
            var url = $"{appUrl}/core/form/view?formKey={documentApproval.Form.FormKey}&id={documentApproval.Id}";
            var allTrackings = CoreService.Query<TrackingApproval>(x => x.DocumentApprovalId == documentApproval.Id && x.RowStatus);

            var commonData = new CommonData
            {
                Name = submitter.Name,
                Email = submitter.Email,
                Url = url,
                AppUrl = appUrl,
                TrackingUsers = allTrackings
            };

            return commonData;
        }
        #endregion

        #region Private Class
        private class CommonData
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string AppUrl { get; set; }
            public string Url { get; set; }
            public IEnumerable<TrackingApproval> TrackingUsers { get; set; }
        }
        #endregion
    }
}
