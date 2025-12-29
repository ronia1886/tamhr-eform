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
    /// Service class that handle common conversation.
    /// </summary>
    public class CommonConversationService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Common convertation repository object.
        /// </summary>
        protected IRepository<CommonConversation> CommonConversationRepository => UnitOfWork.GetRepository<CommonConversation>();

        /// <summary>
        /// Notification repository object.
        /// </summary>
        protected IRepository<Notification> NotificationRepository => UnitOfWork.GetRepository<Notification>();
        #endregion

        #region Variables & Properties
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
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="configService">This <see cref="ConfigService"/> object.</param>
        /// <param name="userService">This <see cref="UserService"/> object.</param>
        /// <param name="emailService">This <see cref="EmailService"/> object.</param>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public CommonConversationService(
            ConfigService configService,
            UserService userService,
            EmailService emailService,
            IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            // Get and set config service object from DI container.
            _configService = configService;

            // Get and set user service object from DI container.
            _userService = userService;

            // Get and set email service object from DI container.
            _emailService = emailService;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of common conversations by channel id asynchronously.
        /// </summary>
        /// <param name="id">This channel id.</param>
        /// <returns>This list of <see cref="CommonConversation"/> objects.</returns>
        public Task<CommonConversation[]> GetConversations(string id)
        {
            // Get list of common conversations by channel id asynchronously.
            return CommonConversationRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.ChannelId == id)
                .OrderBy(x => x.CreatedOn)
                .ToArrayAsync();
        }

        /// <summary>
        /// Save conversation asynchronously.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <param name="name">This current user session name.</param>
        /// <param name="commonConversationRequest">This <see cref="ConversationViewModel"/> object.</param>
        /// <returns>This <see cref="CommonConversation"/> object.</returns>
        public async Task<CommonConversation> SaveConversationAsync(CommonConversationRequest commonConversationRequest)
        {
            var creator = commonConversationRequest.ChannelId.Split("_")[0];

            // Create conversation document object.
            var conversation = new CommonConversation
            {
                ChannelId = commonConversationRequest.ChannelId,
                NoReg = commonConversationRequest.NoReg,
                Name = commonConversationRequest.Name,
                Message = commonConversationRequest.Message
            };

            // Get and set application url from configuration.
            var appUrl = _configService.GetConfigValue("Application.Url", string.Empty);

            // Set mention pattern (like @username1, @username2, @username3, etc) to get list of usernames mentioned by current user session.
            var pattern = @"@([\w.]+)";

            // Create new regex with given pattern.
            var regex = new Regex(pattern, RegexOptions.None);

            // Get list of usernames matches by given regex pattern object.
            var userNames = regex.Matches(commonConversationRequest.Message).OfType<Match>().Where(x => x.Success).Select(x => x.Value.Replace("@", string.Empty));

            // Get list of users by list of usernames.
            var users = _userService.GetByUserNames(userNames);

            // Determine whether list of mentioned users exist or not and current user session noreg not match with the owner of this channel.
            var noMentions = users.IsEmpty();

            // If no mentions then set the list of mentioned users to the document owner.
            if (noMentions)
            {
                // Add the document owner to the list.
                users = !string.IsNullOrEmpty(commonConversationRequest.Members)
                    ? _userService.GetByNoRegs(commonConversationRequest.Members.Split(","))
                    : new[] { _userService.GetByNoReg(creator) };
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
                        if (commonConversationRequest.NoReg == creator && commonConversationRequest.NoReg == user.NoReg) continue;

                        // Create new notification message.
                        var notificationMessage = $"There is new message sent by {commonConversationRequest.Name} on conversation room with channel id <a data-trigger='handler' data-handler='redirectHandler' data-url='{commonConversationRequest.Url}'><b>{commonConversationRequest.ChannelId}</b></a>";

                        // Create new notification and add it into the list.
                        notifications.Add(Notification.Create(commonConversationRequest.NoReg, user.NoReg, notificationMessage, "notice"));
                    }

                    notifications.ForEach(x => NotificationRepository.Add(x));
                }

                // Add conversation object into repository.
                CommonConversationRepository.Add(conversation);

                // Push pending changes into database.
                UnitOfWork.SaveChanges();
            }, System.Data.IsolationLevel.ReadUncommitted);

            // If mentioned user was not empty.
            if (!users.IsEmpty() && !noMentions)
            {
                var recipients = string.Join(",", users.Select(x => x.Email));

                // Parse mail's body template with given parameter.
                var data = new
                {
                    Url = commonConversationRequest.Url.Replace("~", appUrl),
                    // Get and set sender name.
                    From = commonConversationRequest.Name,
                    // Get and set list of mentioned user names.
                    Names = string.Join(", ", users.Select(x => x.Name)),
                    // Get and set conversation message.
                    commonConversationRequest.Message
                };

                _emailService.SendEmail("common-conversation-template", recipients, data);
            }

            // Return the conversation object.
            return await Task.FromResult(conversation);
        } 
        #endregion
    }
}
