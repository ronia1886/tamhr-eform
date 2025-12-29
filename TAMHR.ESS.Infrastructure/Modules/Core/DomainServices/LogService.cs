using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using System;
using System.Net;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle logging event.
    /// </summary>
    public class LogService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Application log repository object.
        /// </summary>
        protected IRepository<ApplicationLog> ApplicationLogRepository => UnitOfWork.GetRepository<ApplicationLog>();

        /// <summary>
        /// Sync log repository object.
        /// </summary>
        protected IRepository<SyncLog> SyncLogRepository => UnitOfWork.GetRepository<SyncLog>();
        protected IRepository<Log> LogRepository => UnitOfWork.GetRepository<Log>();
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="unitOfWork">Unit of Work Object</param>
        public LogService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get application log query by username
        /// </summary>
        /// <param name="userName">UserName</param>
        /// <returns>Application Log Query</returns>
        public IQueryable<ApplicationLog> GetApplicationLogs(string userName)
        {
            return ApplicationLogRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Username == userName && x.RowStatus);
        }

        /// <summary>
        /// Get sync log query by noreg
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Sync Log Query</returns>
        public IQueryable<SyncLog> GetSyncLogs(string noreg)
        {
            return SyncLogRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.CreatedBy == noreg && x.RowStatus);
        }

        /// <summary>
        /// Log and mark as warning
        /// </summary>
        /// <param name="username">Session Username</param>
        /// <param name="ip">Client IP Address</param>
        /// <param name="browser">Client Browser</param>
        /// <param name="messageLocation">Message Location (From Method, Controller, or Action)</param>
        /// <param name="messageDescription">Message Details</param>
        public void LogWarning(string username, string ip, string browser, string messageLocation, string messageDescription) => Log(LogType.Warning, username, ip, browser, messageLocation, messageDescription);

        /// <summary>
        /// Log and mark as success
        /// </summary>
        /// <param name="username">Session Username</param>
        /// <param name="ip">Client IP Address</param>
        /// <param name="browser">Client Browser</param>
        /// <param name="messageLocation">Message Location (From Method, Controller, or Action)</param>
        /// <param name="messageDescription">Message Details</param>
        public void LogSuccess(string username, string ip, string browser, string messageLocation, string messageDescription) => Log(LogType.Success, username, ip, browser, messageLocation, messageDescription);

        /// <summary>
        /// Log and mark as error
        /// </summary>
        /// <param name="username">Session Username</param>
        /// <param name="ip">Client IP Address</param>
        /// <param name="browser">Client Browser</param>
        /// <param name="messageLocation">Message Location (From Method, Controller, or Action)</param>
        /// <param name="messageDescription">Message Details</param>
        public void LogError(string username, string ip, string browser, string messageLocation, string messageDescription) => Log(LogType.Error, username, ip, browser, messageLocation, messageDescription);

        /// <summary>
        /// Log and mark
        /// </summary>
        /// <param name="logType">Log Type (Success|Warning|Error)</param>
        /// <param name="username">Session Username</param>
        /// <param name="ip">Client IP Address</param>
        /// <param name="browser">Client Browser</param>
        /// <param name="messageLocation">Message Location (From Method, Controller, or Action)</param>
        /// <param name="messageDescription">Message Details</param>
        public void Log(LogType logType, string username, string ip, string browser, string messageLocation, string messageDescription)
        {
            var applicationLog = ApplicationLog.Create(logType.ToString().ToLower(), username, ip, browser, messageLocation, messageDescription);

            ApplicationLogRepository.Add(applicationLog);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Log and mark multiple
        /// </summary>
        /// <param name="logType">Log Type (Success|Warning|Error)</param>
        /// <param name="username">Session Username</param>
        /// <param name="ip">Client IP Address</param>
        /// <param name="browser">Client Browser</param>
        /// <param name="messageLocation">Message Location (From Method, Controller, or Action)</param>
        /// <param name="messageDescription">Message Details</param>
        public void Logs(LogType logType, string username, string ip, string browser, string messageLocation, IEnumerable<string> messageDescriptions)
        {
            foreach (var messageDescription in messageDescriptions)
            {
                var applicationLog = ApplicationLog.Create(logType.ToString().ToLower(), username, ip, browser, messageLocation, messageDescription);

                ApplicationLogRepository.Add(applicationLog);
            }

            UnitOfWork.SaveChanges();
        }

        public void InsertLog(string logCategory, string Activity, string applicationModule,string status, string additionalInformation, string createdBy)
        {
            string applicationName = "TAMHR-ESS";
            int maxLog = 1;
            if (LogRepository.Fetch().AsNoTracking().Where(wh => wh.ApplicationName == applicationName).FirstOrDefault() != null)
            {
                maxLog += Convert.ToInt32(LogRepository.Fetch().AsNoTracking().Where(wh => wh.ApplicationName == applicationName).Max(x => x.LogID).Replace(applicationName + "_", ""));
            }

            string hostName = Dns.GetHostName();

            Log log = new Log
            {
                Id = Guid.NewGuid(),
                ApplicationName = applicationName,
                LogID = applicationName + "_" + String.Format("{0:0000000000}", maxLog),
                LogCategory = logCategory,
                Activity = Activity,
                ApplicationModule = applicationModule,
                IPHostName = hostName,
                Status = status /*true or false*/,
                AdditionalInformation = additionalInformation,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now,
                RowStatus = true
            };
            LogRepository.Add(log);

            UnitOfWork.SaveChanges();
        }
        #endregion
    }
}
