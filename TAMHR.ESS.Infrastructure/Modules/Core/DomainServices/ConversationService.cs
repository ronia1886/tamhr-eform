using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Scriban;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle conversation.
    /// </summary>
    public class ConversationService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Document approval convertation repository object.
        /// </summary>
        protected IRepository<DocumentApprovalConversation> DocumentApprovalConvertationRepository => UnitOfWork.GetRepository<DocumentApprovalConversation>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold core service object.
        /// </summary>
        private readonly CoreService _coreService;

        /// <summary>
        /// Field that hold config service object.
        /// </summary>
        private readonly ConfigService _configService;

        /// <summary>
        /// Field that hold user service object.
        /// </summary>
        private readonly UserService _userService;

        /// <summary>
        /// Field that hold email service object.
        /// </summary>
        private readonly EmailService _emailService;

        /// <summary>
        /// Field that hold approval service object.
        /// </summary>
        private readonly ApprovalService _approvalService;
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="coreService">This <see cref="CoreService"/> object.</param>
        /// <param name="configService">This <see cref="ConfigService"/> object.</param>
        /// <param name="userService">This <see cref="UserService"/> object.</param>
        /// <param name="emailService">This <see cref="EmailService"/> object.</param>
        /// <param name="approvalService">This <see cref="ApprovalService"/> object.</param>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public ConversationService(
            CoreService coreService,
            ConfigService configService,
            UserService userService,
            EmailService emailService,
            ApprovalService approvalService,
            IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            // Get and set core service object from DI container.
            _coreService = coreService;

            // Get and set config service object from DI container.
            _configService = configService;

            // Get and set user service object from DI container.
            _userService = userService;

            // Get and set email service object from DI container.
            _emailService = emailService;

            // Get and set approval service object from DI container.
            _approvalService = approvalService;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of conversations by document approval id asynchronously.
        /// </summary>
        /// <param name="id">This document approval id.</param>
        /// <returns>This list of <see cref="DocumentApprovalConversation"/> objects.</returns>
        public Task<DocumentApprovalConversation[]> GetConversations(Guid id)
        {
            // Get list of document approval conversations by document approval id asynchronously.
            return DocumentApprovalConvertationRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.DocumentApprovalId == id)
                .OrderBy(x => x.CreatedOn)
                .ToArrayAsync();
        }

        /// <summary>
        /// Save conversation asynchronously.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <param name="name">This current user session name.</param>
        /// <param name="conversationViewModel">This <see cref="ConversationViewModel"/> object.</param>
        /// <returns>This <see cref="DocumentApprovalConversation"/> object.</returns>
        public async Task<DocumentApprovalConversation> SaveConversationAsync(string noreg, string name, ConversationViewModel conversationViewModel)
        {
            // Create conversation document object.
            var conversation = DocumentApprovalConversation.Create(conversationViewModel.DocumentApprovalId, noreg, name, conversationViewModel.Message);

            // Get and set document approval by given document approval id.
            var documentApproval = _approvalService.GetDocumentApprovalById(conversationViewModel.DocumentApprovalId);

            // Get notice email template.
            var emailTemplate = _coreService.GetEmailTemplate("notice-template");

            // Throw an exception if email template was not found.
            Assert.IsNotNull(emailTemplate, "emailTemplate");

            // Get and set current period.
            var year = DateTime.Now.Year;

            // Create new instance key.
            var instanceKey = $"app-notice";

            // Get and set mail subject from given template.
            var mailSubject = emailTemplate.Subject;

            // Get and set mail from from given template.
            var mailFrom = emailTemplate.MailFrom;

            // Parse body email template.
            var template = Template.Parse(emailTemplate.MailContent);

            // Get and set application url from configuration.
            var appUrl = _configService.GetConfig("Application.Url")?.ConfigValue;

            // Set mention pattern (like @username1, @username2, @username3, etc) to get list of usernames mentioned by current user session.
            var pattern = @"@([\w.]+)";

            // Create new regex with given pattern.
            var regex = new Regex(pattern, RegexOptions.None);

            // Get list of usernames matches by given regex pattern object.
            var userNames = regex.Matches(conversationViewModel.Message).OfType<Match>().Where(x => x.Success).Select(x => x.Value.Replace("@", string.Empty));

            // Get list of users by list of usernames.
            var users = _userService.GetByUserNames(userNames);

            // Determine whether list of mentioned users exist or not and current user session noreg not match with the owner of this approval document.
            var noMentions = users.IsEmpty() && noreg != documentApproval.CreatedBy;

            // If no mentions then set the list of mentioned users to the document owner.
            if (noMentions)
            {
                // Get and set form key.
                var formKey = documentApproval.FormKey;

                // Get and set notify all from configuration.
                var notifyAll = _configService.GetConfigValue(formKey + ".notify-all", defaultValue: false);

                // Add the document owner to the list.
                users = notifyAll
                    ? _approvalService.GetTrackingApprovalUsers(documentApproval.Id, documentApproval.CreatedBy)
                    : new[] { _userService.GetByNoReg(documentApproval.CreatedBy) };
            }

            // Begin transaction with read uncommitted isolation level.
            UnitOfWork.Transact(() =>
            {
                // If list of mentioned users was not empty.
                if (!users.IsEmpty())
                {
                    // Create new list of notifications.
                    var notifications = new List<Notification>();

                    // Enumerate through list of mentioned users.
                    foreach (var user in users)
                    {
                        // Create new notification message.
                        var notificationMessage = $"You are mentioned by {name} in <b>{documentApproval.Form.Title}</b> with document <a class='' data-trigger='handler' data-handler='redirectHandler' data-url='~/core/form/view?formKey={documentApproval.Form.FormKey}&id={documentApproval.Id}'><b>{documentApproval.DocumentNumber}</b></a>";

                        // Create new notification and add it into the list.
                        notifications.Add(Notification.Create(noreg, user.NoReg, notificationMessage, "notice"));
                    }

                    // Add the notifications list into repository.
                    _coreService.CreateNotifications(notifications);
                }

                // Add conversation object into repository.
                DocumentApprovalConvertationRepository.Add(conversation);

                // Push pending changes into database.
                UnitOfWork.SaveChanges();
            }, System.Data.IsolationLevel.ReadUncommitted);

            // If mentioned user was not empty.
            if (!users.IsEmpty() && !noMentions)
            {
                // Create new email manager object.
                var mailManager = _emailService.CreateEmailManager();

                // Get module name from general category.
                var module = _configService.GetGeneralCategory(documentApproval.Form.ModuleCode);

                // Parse mail's body template with given parameter.
                var mailContent = template.Render(new
                {
                    // Get and set module name.
                    Module = module?.Name,
                    // Get and set sender name.
                    From = name,
                    // Get and set list of mentioned user names.
                    Names = string.Join(", ", users.Select(x => x.Name)),
                    // Get and set form title.
                    FormTitle = documentApproval.Form.Title,
                    // Get and set conversation message.
                    conversationViewModel.Message,
                    // Get and set document number.
                    documentApproval.DocumentNumber,
                    // Get and set current period.
                    Year = year,
                    // Get and set application url.
                    Url = $"{appUrl}/core/form/view?formKey={documentApproval.Form.FormKey}&id={documentApproval.Id}"
                });

                try
                {
                    // Send email asynchronously.
                    await mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", users.Select(x => x.Email)));
                }
                catch { }
            }

            // Return the conversation object.
            return conversation;
        } 
        #endregion
    }
}
