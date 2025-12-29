using Agit.Common;
using Agit.Common.Utility;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainEvents;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class TerminationService : DomainServiceBase
    {
        #region Repositories

        /// <summary>
        /// Termination Type repository object.
        /// </summary>
        protected IRepository<TerminationType> TerminationTypeRepository => UnitOfWork.GetRepository<TerminationType>();
        protected IRepository<TerminationHistory> TerminationHistoryRepository => UnitOfWork.GetRepository<TerminationHistory>();
        protected IRepository<GeneralCategory> GeneralCategoryRepository => UnitOfWork.GetRepository<GeneralCategory>();
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();
        protected IReadonlyRepository<EmployeeWorkScheduleView> EmployeeWorkScheduleReadonlyRepository => UnitOfWork.GetRepository<EmployeeWorkScheduleView>();
        protected IRepository<CommonFile> CommonFileRepository => UnitOfWork.GetRepository<CommonFile>();
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();
        protected IRepository<Termination> TerminationRepository => UnitOfWork.GetRepository<Termination>();
        protected IRepository<TrackingApproval> TrackingApprovalRepository => UnitOfWork.GetRepository<TrackingApproval>();
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();
        protected IRepository<Notification> NotificationRepository => UnitOfWork.GetRepository<Notification>();
        protected IRepository<ApprovalMatrix> ApprovalMatrixRepository => UnitOfWork.GetRepository<ApprovalMatrix>();
        protected IRepository<DocumentApprovalHistory> DocumentApprovalHistoryRepository => UnitOfWork.GetRepository<DocumentApprovalHistory>();
        protected IRepository<Config> ConfigRepository => UnitOfWork.GetRepository<Config>();
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository { get { return UnitOfWork.GetRepository<ActualOrganizationStructure>(); } }
        protected IReadonlyRepository<ActualReportingStructureView> ActualReportingStructureReadonlyRepository => UnitOfWork.GetRepository<ActualReportingStructureView>();
        protected IReadonlyRepository<Role> RoleRepository => UnitOfWork.GetRepository<Role>();
        protected IReadonlyRepository<UserRole> UserRoleRepository => UnitOfWork.GetRepository<UserRole>();
        #endregion

        #region Domain Event Manager
        //protected readonly DomainEventManager DomainEventManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public TerminationService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Termination Form

        public IEnumerable<TerminationType> GetTerminationTypes()
        {
            return TerminationTypeRepository.FindAll();
        }

        public IEnumerable<TerminationExitClearanceUsernameStoredEntity> GetExitClearanceUsers()
        {
            var form = FormRepository.Fetch()
                    .AsNoTracking()
                    .FirstOrDefault(x => x.FormKey == "termination");
            return UnitOfWork.UspQuery<TerminationExitClearanceUsernameStoredEntity>(new { FormId = form.Id });
        }

        public GeneralCategory GetTerminationTypeById(string Id)
        {
            return GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == new Guid(Id));
        }

        public Config GetConfigByConfigKey(string configKey)
        {
            return ConfigRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.ConfigKey == configKey);
        }

        public TerminationViewModel GetTerminationViewModel(Guid id)
        {
            var detail = DocumentRequestDetailRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.DocumentApprovalId == id);

            TerminationViewModel data = JsonConvert.DeserializeObject<TerminationViewModel>(detail.ObjectValue);
            return data;
        }


        public Termination GetTermination(Guid id)
        {
            var data = TerminationRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.DocumentApprovalId == id);

            return data;
        }
        public ActualOrganizationStructure GetHRGADivHeadByPostCode(string Postcode)
        {
            var data = ActualOrganizationStructureRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.PostCode == Postcode && x.PostName.Contains("human"));

            return data;
        }

        public bool GetTerminationUserRoleKey(string noReg)
        {
            var data = UnitOfWork.UspQuery<TerminationUserRoleKeyStoredEntity>(new { NoReg = noReg });

            if (data.Count() > 0)
            {
                var terminationRole = data.FirstOrDefault(x => x.RoleKey == "TERMINATION_USER");
                if (terminationRole == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public GeneralCategory GetTerminationTypeByTypeKey(string TerminationTypeKey)
        {
            return GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Code == TerminationTypeKey & x.Category == "TerminationType");
        }

        public GeneralCategory GetTerminationTypeById(Guid id)
        {
            var data = GeneralCategoryRepository.Fetch()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.Id == id & x.Category == "TerminationType");
            return data;
        }

        public IEnumerable<TerminationTypeStoredEntity> GetTerminationTypes(string type)
        {
            var results = UnitOfWork.UspQuery<TerminationTypeStoredEntity>(new { Type = type });
            return results;
        }

        public IEnumerable<TerminationEmployeeInfoStoredEntity> GetTerminationEmployeeInfo(string noReg)
        {
            var results = UnitOfWork.UspQuery<TerminationEmployeeInfoStoredEntity>(new { NoReg = noReg });
            return results;
        }

        public IEnumerable<TerminationPdfEmployeeInfoStoredEntity> GetTerminationPdfEmployeeInfo(string noReg)
        {
            var results = UnitOfWork.UspQuery<TerminationPdfEmployeeInfoStoredEntity>(new { NoReg = noReg });
            return results;
        }
        public IEnumerable<TerminationExitClearanceStoredEntity> GetTerminationExitClearancePdfData(Guid documentApprovalId)
        {
            var results = UnitOfWork.UspQuery<TerminationExitClearanceStoredEntity>(new { documentApprovalId });

            return results;
        }

        public IEnumerable<TerminationApproverLevelStoredEntity> GetAllLevels (string noReg, string formKey)
        {
                var form = FormRepository.Fetch()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.FormKey == formKey);
            var data = UnitOfWork.UspQuery<TerminationApproverLevelStoredEntity>(new { NoReg = noReg, FormId = form.Id, DocumentApprovalId = "" });
            return data;
        }   

    public int GetCurrentLevel(string noReg, string formKey, Guid documentApprovalId)
        {
            var docId = "";
            if(documentApprovalId != new Guid())
            {
                docId = documentApprovalId.ToString();
            }
            var form = FormRepository.Fetch()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.FormKey == formKey);
            var currentApprover = TrackingApprovalRepository.Fetch().AsNoTracking()           
                        .FirstOrDefault(x => x.DocumentApprovalId == documentApprovalId && x.ApprovalLevel==2 && string.IsNullOrEmpty(x.ApprovalActionCode));

            var data = UnitOfWork.UspQuery<TerminationApproverLevelStoredEntity>(new { NoReg = noReg, FormId = form.Id, DocumentApprovalId = "" });
            int result = 0;
            if (data.Count() != 0 && currentApprover != null)
            {
               result = data.OrderBy(x=>x.ApproverLevel).First().ApproverLevel;
            }
            else if (data.Count() != 0 && currentApprover == null)
            {
                result = data.OrderByDescending(x => x.ApproverLevel).First().ApproverLevel;
            }
            else if(data.Count() == 0 )
            {
                var datahistory = UnitOfWork.UspQuery<TerminationApproverLevelStoredEntity>(new { NoReg = noReg, FormId = form.Id, DocumentApprovalId = docId });
                if(datahistory.Count() != 0)
                {
                    result = datahistory.First().ApproverLevel;
                }
            }
            return result;
        }

        public DocumentApproval GetDocumentApprovalById(Guid id)
        {
            var data = DocumentApprovalRepository.Fetch()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.Id == id);
            return data;
        }

        public void updateEndDate(Guid documentApprovalId, DateTime EndDate, string noReg, string buildingName, string PICExitInterview)
        {
            var detail = DocumentRequestDetailRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.DocumentApprovalId == documentApprovalId);
            var terminationData = TerminationRepository.Fetch().FirstOrDefault(x => x.DocumentApprovalId == documentApprovalId);

            var json = JsonConvert.DeserializeObject<TerminationViewModel>(detail.ObjectValue);
            var oldDate = json.EndDate;
            var terminationId = json.Id;
            json.EndDate = EndDate;
            json.BuildingName = buildingName;
            json.PICExitInterview = PICExitInterview;

            UnitOfWork.Transact((transaction) =>
            {
                TerminationRepository.Fetch()
                   .Where(x => x.DocumentApprovalId == documentApprovalId)
                   .Update(x => new Termination
                   {
                       EndDate = EndDate,
                       ModifiedBy = noReg,
                       ModifiedOn = DateTime.Now
                   });

                DocumentRequestDetailRepository.Fetch()
                    .Where(x => x.DocumentApprovalId == documentApprovalId)
                    .Update(x => new DocumentRequestDetail
                    {
                        ObjectValue = JsonConvert.SerializeObject(json),
                        ModifiedBy = noReg,
                        ModifiedOn = DateTime.Now
                    });

                var updateEndDateResult = UnitOfWork.UspQuery<TerminationUpdateAbnormal>(
                   new
                   {
                       TerminationId = terminationData.Id,
                       oldDate = oldDate
                   },
                   transaction
               );

                UnitOfWork.SaveChanges();
            });
        }

        public void addTerminationCommonFile(CommonFile data, string type, Guid documentApprovalId, string noReg)
        {
            UnitOfWork.Transact((transaction) =>
            {
                CommonFileRepository.Add(data);
                UnitOfWork.SaveChanges();

                if(type == "invitation")
                {
                    TerminationRepository.Fetch()
                    .Where(x => x.DocumentApprovalId == documentApprovalId)
                    .Update(x => new Termination
                    {
                        InterviewCommonFileId = data.Id,
                        ModifiedBy = noReg,
                        ModifiedOn = DateTime.Now
                    });

                    UnitOfWork.SaveChanges();
                }
                else
                {
                    TerminationRepository.Fetch()
                    .Where(x => x.DocumentApprovalId == documentApprovalId)
                    .Update(x => new Termination
                    {
                        VerklaringCommonFileId = data.Id,
                        ModifiedBy = noReg,
                        ModifiedOn = DateTime.Now
                    });

                    UnitOfWork.SaveChanges();
                }
            });
        }

        public bool ResignationCreateValidate(string noReg, string type, string saveType)
        {
            var typedata = GeneralCategoryRepository.Fetch()
                            .AsNoTracking()
                            .FirstOrDefault(x => x.Name == type & x.Category == "TerminationType");

            var query =
               from post in DocumentApprovalRepository.Fetch()
               //join meta in TerminationRepository.Fetch().Where(x => x.NoReg == noReg && x.TerminationTypeId == typedata.Id)
               join meta in TerminationRepository.Fetch().Where(x => x.NoReg == noReg)
               on post.Id equals meta.DocumentApprovalId
               where
                    //!post.DocumentStatusCode.Contains("draft") &&
                    !post.DocumentStatusCode.Contains("expired") &&
                    !post.DocumentStatusCode.Contains("cancelled") &&
                    !post.DocumentStatusCode.Contains("rejected") 
                    //!post.DocumentStatusCode.Contains("revised") 
               select new { Post = post, Meta = meta };

            if( type == "Resignation" && saveType == "Create")
            {
                var data = query.ToList();
                return data.Count() > 0 ? false : true;
            }
            else
            {
                var data = query.Where(x => !x.Post.DocumentStatusCode.Contains("draft") && !x.Post.DocumentStatusCode.Contains("revised")).ToList();
                if(type != "Resignation" && saveType == "Create")
                {
                    data = query.ToList();
                }
                return data.Count() > 0 ? false : true;
            }
        }

        public bool ContractValidate(string noReg)
        {
            var data = ActualOrganizationStructureRepository.Fetch()
                            .AsNoTracking()
                            .Where(x => x.NoReg == noReg & x.WorkContractText.Contains("Contract")).ToList();
            return data.Count() > 0 ? true : false;
        }

        public string ExitClearanceRoleValidate(string[] roles, string userid)
        {
            var result = "";
            for (int i = 0; i < roles.Count(); i++)
            {
                var roleData = RoleRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.Id == new Guid(roles[i]) && x.RoleKey.Contains("EXIT_CLEARANCE") && !x.RoleKey.Contains("EXIT_CLEARANCE_USER"));

                if(roleData != null)
                {
                    var user = UserRoleRepository.Fetch()
                                    .AsNoTracking()
                                    .Where(x => x.RoleId == roleData.Id)
                                    .ToList();
                    
                    if(user.Count > 0)
                    {
                        if(user[0].UserId != new Guid(userid))
                        { 
                            result = roleData.Description;
                        }
                    }
                }
            }
            return result;
        }

        public void UpdateTerminationExitClearanceTask(string[] roles, string noReg)
        {
            var type = "";
            var userRoleKey = UnitOfWork.UspQuery<TerminationUserRoleKeyStoredEntity>(new { NoReg = noReg }).ToList();
            var userData = UserRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.NoReg == noReg);

            List<string> existingRoleKeyData = new List<string>();
            List<string> newRoleKeyData = new List<string>();
            for (int i = 0; i < userRoleKey.Count(); i++)
            {
                var roleData = RoleRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.RoleKey == userRoleKey[i].RoleKey && x.RoleKey.Contains("EXIT_CLEARANCE") && !x.RoleKey.Contains("EXIT_CLEARANCE_USER"));
                if (roleData != null)
                {
                    existingRoleKeyData.Add(roleData.Id.ToString());
                }
            }
            for (int i = 0; i < roles.Count(); i++)
            {
                var id = roles[i].ToString();
                var roleData = RoleRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.Id == new Guid(id) && x.RoleKey.Contains("EXIT_CLEARANCE") && !x.RoleKey.Contains("EXIT_CLEARANCE_USER"));
                if (roleData != null)
                {
                    newRoleKeyData.Add(roleData.Id.ToString());
                }
            }

            string[] existingRoleKey = existingRoleKeyData.ToArray();
            string[] newRoleKey = newRoleKeyData.ToArray();
            var added = newRoleKey.Except(existingRoleKey).ToArray();
            var removed = existingRoleKey.Except(newRoleKey).ToArray();

            if(added.Count() != 0)
            {
                type = "AddRolePIC";
                for (int i = 0; i < added.Count(); i++)
                {
                    var roleData = RoleRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.Id == new Guid(added[i].ToString()));

                    UnitOfWork.UspQuery<TerminationUpdateRolePICStoredEntity>(new { NoReg = noReg, RoleKey = roleData.RoleKey, UserName = userData.Username, Type = type });
                }
            }

            if (removed.Count() != 0)
            {
                type = "DeleteRolePIC";
                for (int i = 0; i < removed.Count(); i++)
                {
                    var roleData = RoleRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.Id == new Guid(removed[i].ToString()));
                    
                    UnitOfWork.UspQuery<TerminationUpdateRolePICStoredEntity>(new { NoReg = noReg, RoleKey = roleData.RoleKey, UserName = userData.Username, Type = type });
                }
            }
        }

        public bool TerminationApprovedByTerminationUser (Guid documentApprovalId)
        {
            var approvalHistory = UnitOfWork.UspQuery<TerminationApprovalHistoryStoredEntity>(new { DocumentApprovalId = documentApprovalId });

            if (approvalHistory.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetTerminationTooltipContent(string code)
        {
            var results = GeneralCategoryRepository.Fetch()
                            .AsNoTracking()
                            .FirstOrDefault(x => x.Code == code & x.Category == "TerminationFormatNotes");
            return results.Description;
        }

        #endregion

        #region Approval

        public void Submit(string noreg, DocumentApproval documentApproval)
        {
            var detail = DocumentRequestDetailRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.DocumentApprovalId == documentApproval.Id);

            var json = JsonConvert.DeserializeObject<Termination>(detail.ObjectValue);

            var terminationTypeName = GeneralCategoryRepository.Fetch()
                                        .AsNoTracking()
                                        .FirstOrDefault(x => x.Id == json.TerminationTypeId & x.Category == "TerminationType");

            CustomApprovalTitle(Convert.ToString(documentApproval.Id), terminationTypeName.Name, json.NoReg);

            SendNotification(documentApproval.Id, json.NoReg, noreg);

            //if(terminationTypeName.Name != "Resignation")
            //{
            AddUserToTrackingApproval(documentApproval.Id, json.NoReg, noreg, terminationTypeName.Name);
            //}

            TerminationUpdate(noreg, documentApproval);
        }

        public void AddUserToTrackingApproval(Guid id, string noReg, string noRegCreator, string type)
        {
            UnitOfWork.Transact((transaction) =>
            {
                var documentApproval = DocumentApprovalRepository.Fetch()
                                      .AsNoTracking()
                                      .FirstOrDefault(x => x.Id == id);

                var employeeData = ActualOrganizationStructureRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.NoReg == noReg);

                var userData = UserRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.NoReg == noRegCreator);

                if(type == "Retirement")
                {
                    //send notif for terminated user
                    var notification = new Notification();
                    notification.FromNoReg = noRegCreator;
                    notification.ToNoReg = noReg;
                    notification.Message = "Document with number <a data-trigger='handler' data-handler='redirectHandler' data-url='~/core/form/view?formKey=termination&id=" + documentApproval.Id + "'><b>" + documentApproval.DocumentNumber + "</b></a> has been submited by " + userData.Name;
                    notification.NotificationTypeCode = noReg;
                    notification.TriggerDate = null;
                    notification.CreatedBy = noRegCreator;
                    notification.CreatedOn = DateTime.Now;
                    notification.RowStatus = true;
                    NotificationRepository.Add(notification);
                }

                if (type == "Retirement")
                {
                    //add to tracking approval for conversation mention
                    var tracking = new TrackingApproval();
                    tracking.DocumentApprovalId = id;
                    tracking.NoReg = noReg;
                    tracking.Name = employeeData.Name;
                    tracking.JobCode = employeeData.JobCode;
                    tracking.JobName = employeeData.JobName;
                    tracking.PostCode = employeeData.PostCode;
                    tracking.PostName = employeeData.PostName;
                    tracking.ApprovalLevel = 0;
                    tracking.ApprovalActionCode = "acknowledge";
                    tracking.Remarks = "Acknowledge";
                    tracking.CreatedOn = DateTime.Now;
                    tracking.CreatedBy = noRegCreator;
                    tracking.ModifiedOn = DateTime.Now;
                    tracking.ModifiedBy = noRegCreator;
                    tracking.RowStatus = true;
                    TrackingApprovalRepository.Add(tracking);
                }
            });
        }

        public void SendNotification(Guid id, string noReg, string noRegCreator)
        {
            UnitOfWork.Transact((transaction) => { 
                var documentApproval = DocumentApprovalRepository.Fetch()
                                       .AsNoTracking()
                                       .FirstOrDefault(x => x.Id == id);

                var employeeData = ActualReportingStructureReadonlyRepository.Fetch()
                                    .AsNoTracking()
                                    .Where(x => x.NoReg == noReg && !x.ParentJobName.Contains("Director") && !x.ParentJobName.Contains("Vice President"))
                                    .OrderBy(x => x.HierarchyLevel)
                                    .ToList();

                var userData = UserRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.NoReg == noRegCreator);

                if (employeeData != null && employeeData.Count() > 0)
                {
                    string[] employeeDistinct = employeeData.Select(x => x.ParentNoReg).Distinct().ToArray();

                    for (int i = 0; i < employeeDistinct.Count(); i++)
                    {
                        //Notif untuk atasan
                        var notification = new Notification();
                        notification.FromNoReg = noReg;
                        notification.ToNoReg = employeeDistinct[i];
                        notification.Message = "Document with number <a data-trigger='handler' data-handler='redirectHandler' data-url='~/core/form/view?formKey=termination&id=" + documentApproval.Id + "'><b>" + documentApproval.DocumentNumber + "</b></a> has been submited by " + userData.Name;
                        notification.NotificationTypeCode = noReg;
                        notification.TriggerDate = null;
                        notification.CreatedBy = noRegCreator;
                        notification.CreatedOn = DateTime.Now;
                        notification.RowStatus = true;
                        NotificationRepository.Add(notification);
                    }

                    for (int i = 0; i < employeeData.Count(); i++)
                    {
                        //add to tracking approval atasan
                        var tracking = new TrackingApproval();
                        tracking.DocumentApprovalId = id;
                        tracking.NoReg = noReg;
                        tracking.Name = employeeData[i].ParentName;
                        tracking.JobCode = employeeData[i].ParentJobCode;
                        tracking.JobName = employeeData[i].ParentJobName;
                        tracking.PostCode = employeeData[i].ParentPostCode;
                        tracking.PostName = employeeData[i].ParentPostName;
                        tracking.ApprovalLevel = -1;
                        tracking.ApprovalActionCode = "acknowledge";
                        tracking.Remarks = "Acknowledge";
                        tracking.CreatedOn = DateTime.Now;
                        tracking.CreatedBy = noRegCreator;
                        tracking.ModifiedOn = DateTime.Now;
                        tracking.ModifiedBy = noRegCreator;
                        tracking.RowStatus = true;
                        TrackingApprovalRepository.Add(tracking);

                        var documentApprovalHistory = new DocumentApprovalHistory();
                        documentApprovalHistory.DocumentApprovalId = documentApproval.Id;
                        documentApprovalHistory.NoReg = employeeData[i].ParentNoReg;
                        documentApprovalHistory.Name = employeeData[i].ParentName;
                        documentApprovalHistory.PostCode = employeeData[i].ParentPostCode;
                        documentApprovalHistory.PostName = employeeData[i].ParentPostName;
                        documentApprovalHistory.JobCode = employeeData[i].ParentJobCode;
                        documentApprovalHistory.JobName = employeeData[i].ParentJobName;
                        documentApprovalHistory.ApprovalActionCode = "acknowledge";
                        documentApprovalHistory.Remarks = null;
                        documentApprovalHistory.CreatedBy = noRegCreator;
                        documentApprovalHistory.CreatedOn = DateTime.Now;
                        DocumentApprovalHistoryRepository.Add(documentApprovalHistory);
                    }
                }

                //exit cleareance level hierarcy
                //var data = UnitOfWork.UspQuery<TerminationExitClearanceUsernameStoredEntity>(new { FormId = documentApproval.FormId }, transaction);
                //var exitClearanceData = data.ToList();

                //for (int i = 0; i < exitClearanceData.Count(); i++)
                //{
                //    TrackingApprovalRepository.Fetch()
                //    .Where(x => x.DocumentApprovalId == documentApproval.Id && x.NoReg == exitClearanceData[i].NoReg)
                //    .Update(x => new TrackingApproval
                //    {
                //        ApprovalLevel = 3,
                //        ModifiedBy = noRegCreator,
                //        ModifiedOn = DateTime.Now
                //    });
                //}

                UnitOfWork.SaveChanges();
            });
        }

        public bool TrackingUser(Guid documentapprovalId, string noReg)
        {
            var data = TrackingApprovalRepository.Fetch()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.DocumentApprovalId == documentapprovalId && x.NoReg == noReg && x.ApprovalLevel != 0);

            if(data == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Approve(string noreg, DocumentApproval documentApproval)
        {
            var terminationdata = TerminationRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            UnitOfWork.Transact((transaction) =>
            {
                var docId = "";
                if (documentApproval.Id != new Guid())
                {
                    docId = documentApproval.Id.ToString();
                }
                var level = UnitOfWork.UspQuery<TerminationApproverLevelStoredEntity>(new { NoReg = noreg, FormId = documentApproval.FormId, DocumentApprovalId = "" }, transaction);

                if (level.First().ApproverLevel == 1)
                {
                    var approvalHistory = UnitOfWork.UspQuery<TerminationApprovalHistoryStoredEntity>(new { DocumentApprovalId = documentApproval.Id }, transaction);

                    if(approvalHistory.Count() > 0)
                    {
                        var data = UnitOfWork.UspQuery<TerminationExitClearanceUsernameStoredEntity>(new { FormId = documentApproval.FormId }, transaction);
                        var exitClearanceData = data.ToList();
                        string exitClearanceUsername = "";

                        for (int i = 0; i < exitClearanceData.Count(); i++)
                        {
                            TrackingApprovalRepository.Fetch()
                            .Where(x => x.DocumentApprovalId == documentApproval.Id && x.NoReg == exitClearanceData[i].NoReg && x.ApprovalMatrixId == exitClearanceData[i].MatrixId)
                            .Update(x => new TrackingApproval
                            {
                                ApprovalLevel = 3,
                                ModifiedBy = noreg,
                                ModifiedOn = DateTime.Now
                            });

                            if (i == (exitClearanceData.Count() - 1))
                            {
                                exitClearanceUsername = exitClearanceUsername + "(" + exitClearanceData[i].Username + ")";
                            }
                            else
                            {
                                exitClearanceUsername = exitClearanceUsername + "(" + exitClearanceData[i].Username + "),";
                            }

                            var terminationHistory = new TerminationHistory();
                            terminationHistory.DocumentApprovalId = documentApproval.Id;
                            terminationHistory.ApprovalMatrixId = exitClearanceData[i].MatrixId;
                            terminationHistory.NoReg = exitClearanceData[i].NoReg;
                            terminationHistory.PICName = exitClearanceData[i].Name;
                            terminationHistory.Code = exitClearanceData[i].Code;
                            terminationHistory.Title = exitClearanceData[i].Title;
                            terminationHistory.RowStatus = true;
                            terminationHistory.CreatedBy = noreg;
                            terminationHistory.CreatedOn = DateTime.Now;
                            TerminationHistoryRepository.Add(terminationHistory);
                        }


                        DocumentApprovalRepository.Fetch()
                        .Where(x => x.Id == documentApproval.Id)
                        .Update(x => new DocumentApproval
                        {
                            CurrentApprover = exitClearanceUsername,
                            ModifiedBy = noreg,
                            ModifiedOn = DateTime.Now
                        });


                        //add all termination user to history
                        var roleData = RoleRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.RoleKey == "TERMINATION_USER");
                        var userRole = UserRoleRepository.Fetch()
                                    .AsNoTracking()
                                    .Where(x => x.RoleId == roleData.Id)
                                    .ToList();

                        for (int i = 0; i < userRole.Count(); i++)
                        {
                            var userData = UserRepository.Fetch()
                                .AsNoTracking()
                                .FirstOrDefault(x => x.Id == userRole[i].UserId);

                            if(userData.NoReg != noreg)
                            {
                                var employeeData = ActualOrganizationStructureRepository.Fetch()
                                     .AsNoTracking()
                                     .FirstOrDefault(x => x.NoReg == userData.NoReg);

                                var documentApprovalHistory = new DocumentApprovalHistory();
                                documentApprovalHistory.DocumentApprovalId = documentApproval.Id;
                                documentApprovalHistory.NoReg = userData.NoReg;
                                documentApprovalHistory.Name = userData.Name;
                                documentApprovalHistory.PostCode = employeeData.PostCode;
                                documentApprovalHistory.PostName = employeeData.PostName;
                                documentApprovalHistory.JobCode = employeeData.JobCode;
                                documentApprovalHistory.JobName = employeeData.JobName;
                                documentApprovalHistory.ApprovalActionCode = "acknowledge";
                                documentApprovalHistory.Remarks = null;
                                documentApprovalHistory.CreatedBy = noreg;
                                documentApprovalHistory.CreatedOn = DateTime.Now;
                                DocumentApprovalHistoryRepository.Add(documentApprovalHistory);
                            }
                        }

                        UnitOfWork.SaveChanges();
                    }
                }
            });
        }

        public void Reject(string noreg, DocumentApproval documentApproval)
        {
            UnitOfWork.Transact((transaction) =>
            {
                // Call the stored procedure SP_TERMINATION_UPDATE_ABNORMAL
                var rollbackTermination = UnitOfWork.UspQuery<TerminationRollback>(
                    new
                    {
                        DocumentApprovalId = documentApproval.Id,
                    },
                    transaction
                );


            });
        }

        public void Revise(string noreg, DocumentApproval documentApproval)
        {
            if (documentApproval.Title.Contains("Resignation"))
            {
                UnitOfWork.Transact((transaction) =>
                {
                    // Call the stored procedure SP_TERMINATION_UPDATE_ABNORMAL
                    var rollbackTermination = UnitOfWork.UspQuery<TerminationRollback>(
                        new
                        {
                            DocumentApprovalId = documentApproval.Id,
                        },
                        transaction
                    );


                });
                //Do Nothing
            }
            else
            {
                //add all termination user revise notif
                UnitOfWork.Transact((transaction) =>
                {
                    var revisedUser = UserRepository.Fetch()
                            .AsNoTracking()
                            .FirstOrDefault(x => x.NoReg == noreg);

                    var roleData = RoleRepository.Fetch()
                            .AsNoTracking()
                            .FirstOrDefault(x => x.RoleKey == "TERMINATION_USER");
                    var userRole = UserRoleRepository.Fetch()
                                .AsNoTracking()
                                .Where(x => x.RoleId == roleData.Id)
                                .ToList();

                    for (int i = 0; i < userRole.Count(); i++)
                    {
                        var userData = UserRepository.Fetch()
                            .AsNoTracking()
                            .FirstOrDefault(x => x.Id == userRole[i].UserId);

                        if (userData.NoReg != documentApproval.SubmitBy && userData.NoReg != documentApproval.CreatedBy)
                        {
                            var notification = new Notification();
                            notification.FromNoReg = noreg;
                            notification.ToNoReg = userData.NoReg;
                            notification.Message = "Document with number <a data-trigger='handler' data-handler='redirectHandler' data-url='~/core/form/view?formKey=termination&id=" + documentApproval.Id + "'><b>" + documentApproval.DocumentNumber + "</b></a> has been revised by " + revisedUser.Name;
                            notification.NotificationTypeCode = "default";
                            notification.TriggerDate = null;
                            notification.CreatedBy = noreg;
                            notification.CreatedOn = DateTime.Now;
                            notification.RowStatus = true;
                            NotificationRepository.Add(notification);
                        }
                    };

                    var rollbackTermination = UnitOfWork.UspQuery<TerminationRollback>(
                       new
                       {
                           DocumentApprovalId = documentApproval.Id,
                       }
                   );

                    UnitOfWork.SaveChanges();
                });
            }
        }

        public void Complete(string noreg, DocumentApproval documentApproval)
        {
            // Fetch the termination data based on the DocumentApproval ID
            var terminationData = TerminationRepository.Fetch()
                .Where(x => x.DocumentApprovalId == documentApproval.Id)
                .FirstOrDefault();

            if (terminationData == null)
            {
                throw new Exception("Termination data not found.");
            }

            // Begin a transaction
            UnitOfWork.Transact((transaction) =>
            {
                var docId = documentApproval.Id != Guid.Empty ? documentApproval.Id.ToString() : string.Empty;

                // Ensure termination data has a valid EndDate
                if (terminationData.EndDate == null)
                {
                    throw new Exception("End date is not set for the termination data.");
                }

                var endingDate = terminationData.EndDate.Date;

                // Call the stored procedure SP_UPDATE_TERMINATION_DATE
                var updateEndDateResult = UnitOfWork.UspQuery<TerminationUpdateEndDate>(
                    new
                    {
                        NoReg = terminationData.NoReg,
                        EndDate = endingDate,
                        DocumentApprovalId = docId
                    },
                    transaction
                );

            });
        }

        public void TerminationUpdate(string noreg, DocumentApproval documentApproval)
        {
            // Fetch the termination data based on the DocumentApproval ID
            var terminationData = TerminationRepository.Fetch()
                .Where(x => x.DocumentApprovalId == documentApproval.Id)
                .FirstOrDefault();

            if (terminationData == null)
            {
                throw new Exception("Termination data not found.");
            }

            // Begin a transaction
            UnitOfWork.Transact((transaction) =>
            {
                var docId = documentApproval.Id != Guid.Empty ? documentApproval.Id.ToString() : string.Empty;

                // Ensure termination data has a valid EndDate
                if (terminationData.EndDate == null)
                {
                    throw new Exception("End date is not set for the termination data.");
                }

                var endingDate = terminationData.EndDate.Date;

                // Call the stored procedure SP_UPDATE_TERMINATION_DATE
                var updateEndDateResult = UnitOfWork.UspQuery<TerminationUpdateEndDate>(
                    new
                    {
                        NoReg = terminationData.NoReg,
                        EndDate = endingDate,
                        DocumentApprovalId = docId
                    },
                    transaction
                );

            });
        }


        #endregion

        #region Workflow Area
        public bool GetListWorkSchEmp(string noreg, DateTime? startDate, DateTime? endDate)
        {
            var data = EmployeeWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Date >= startDate && x.Date <= endDate)
                .FirstOrDefault();

            return data.Off;
        }

        public bool AttachmentValidation(DocumentRequestDetailViewModel<TerminationViewModel> terminationViewModel, string validationType)
        {
            foreach (var attachments in terminationViewModel.Attachments)
            {
                var CommonFileId = attachments.CommonFileId.ToString();
                var Test = CommonFileRepository.Fetch().Where(x => x.Id == new Guid(CommonFileId));
                var Data = Test.First();

                if (validationType == "filesize")
                {
                    if (Data.FileSize >= 5120)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (validationType == "filetype")
                {
                    if (Data.FileType == "application/pdf")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void CustomApprovalTitle(string id, string type, string noreg)
        {
            string titleNew = type;
            Guid docId = new Guid(id);

            UnitOfWork.Transact((transaction) =>
            {
                var documentApprovalData = DocumentApprovalRepository.Fetch().Where(x => x.Id == docId).FirstOrDefault();
                var userData = UserRepository.Fetch().Where(x => x.NoReg == noreg).FirstOrDefault();
                string currentTitle = documentApprovalData.Title;
                string newTitle = currentTitle.Replace("{terminationType}", type + " document").Replace("{employeename}", userData.Name);

                DocumentApprovalRepository.Fetch()
                    .Where(x => x.Id == docId)
                    .Update(x => new DocumentApproval
                    {
                        Title = newTitle
                    });
                //UnitOfWork.SaveChanges();

                var terminationdata = TerminationRepository.Fetch().Where(x => x.DocumentApprovalId == documentApprovalData.Id).FirstOrDefault();

                var detail = DocumentRequestDetailRepository.Fetch()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.DocumentApprovalId == docId);

                var detaildata = JsonConvert.DeserializeObject<Termination>(detail.ObjectValue);

                var userDetailStructure = UnitOfWork.UspQuery<TerminationEmployeeInfoStoredEntity>(new { NoReg = noreg }, transaction).FirstOrDefault();

                if (terminationdata == null)
                {
                    var termination = new Termination();
                    termination.DocumentApprovalId = docId;
                    termination.NoReg = detaildata.NoReg;
                    termination.Name = userData.Name;
                    termination.EndDate = detaildata.EndDate;
                    termination.TerminationTypeId = detaildata.TerminationTypeId;
                    termination.Reason = detaildata.Reason;
                    termination.AttachmentCommonFileId = detaildata.AttachmentCommonFileId;
                    termination.Position = userDetailStructure.Position;
                    termination.Division = userDetailStructure.Division;
                    termination.Class = userDetailStructure.Class;
                    termination.Email = userDetailStructure.Email;
                    termination.RowStatus = true;
                    termination.CreatedBy = documentApprovalData.CreatedBy;
                    termination.CreatedOn = DateTime.Now;
                    termination.ModifiedBy = documentApprovalData.CreatedBy;
                    termination.ModifiedOn = DateTime.Now;
                    TerminationRepository.Add(termination);

                    //UnitOfWork.SaveChanges();
                }
                else
                {
                    TerminationRepository.Fetch()
                    .Where(x => x.Id == terminationdata.Id)
                    .Update(x => new Termination
                    {
                        NoReg = detaildata.NoReg,
                        Name = userData.Name,
                        EndDate = detaildata.EndDate,
                        TerminationTypeId = detaildata.TerminationTypeId,
                        Reason = detaildata.Reason,
                        AttachmentCommonFileId = detaildata.AttachmentCommonFileId,
                        Position = userDetailStructure.Position,
                        Division = userDetailStructure.Division,
                        Class = userDetailStructure.Class,
                        Email = userDetailStructure.Email,
                        ModifiedBy = documentApprovalData.ModifiedBy,
                        ModifiedOn = DateTime.Now
                    });

                    //UnitOfWork.SaveChanges();
                }
                UnitOfWork.SaveChanges();
            });
        }

        public async Task<DocumentApproval> PostAsync(string username, ActualOrganizationStructure actualOrganizationStructure, string eventName, Guid documentApprovalId, string remarks)
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
            //Assert.ThrowIf(!documentApproval.CanSubmit && ObjectHelper.IsIn(eventNameLower, ApprovalAction.Approve), "This document cannot be approve");
            Assert.ThrowIf(ObjectHelper.IsIn(documentApproval.DocumentStatusCode, DocumentStatus.Completed, DocumentStatus.Cancelled, DocumentStatus.Rejected, DocumentStatus.Expired), $"Document status with number <b>{documentApproval.DocumentNumber}</b> already completed");
            Assert.ThrowIf(!(eventNameLower == ApprovalAction.Cancel && ObjectHelper.IsIn(actualOrganizationStructure.NoReg, documentApproval.CreatedBy, documentApproval.SubmitBy)) && !string.IsNullOrEmpty(documentApproval.CurrentApprover) && !documentApproval.CurrentApprover.Contains("(" + username + ")"), "You dont have permission to approve/reject this document");

            UnitOfWork.Transact((transaction) =>
            {
                if (eventNameLower == ApprovalAction.Approve)
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

                    //2023 - 02 - 08 | Dwiky | perbaikan tracking approval yang tiba-tiba complete
                    //var sql = "UPDATE TB_R_TRACKING_APPROVAL SET ApprovalActionCode=@eventNameLower,Remarks=@remarks, ModifiedBy=@NoReg, ModifiedOn=GETDATE() " +
                    //"WHERE DocumentApprovalId=@DocumentApprovalId And ApprovalLevel=@ApprovalLevel And NoReg=@NoReg";

                    //UnitOfWork.GetConnection().ExecuteAsync(sql, new
                    //{
                    //    eventNameLower = eventNameLower,
                    //    remarks = remarks,
                    //    NoReg = actualOrganizationStructure.NoReg,
                    //    DocumentApprovalId = documentApprovalId,
                    //    ApprovalLevel = trackingApproval.ApprovalLevel
                    //}, transaction);
                    //2023 - 02 - 08 | Dwiky | perbaikan tracking approval yang tiba-tiba complete

                    TrackingApprovalRepository.Fetch()
                        .Where(x => x.DocumentApprovalId == documentApprovalId && x.ApprovalLevel == trackingApproval.ApprovalLevel && x.NoReg == actualOrganizationStructure.NoReg)
                        .Update(x => new TrackingApproval { ApprovalActionCode = eventNameLower, Remarks = remarks, ModifiedBy = actualOrganizationStructure.NoReg, ModifiedOn = now });

                    //2022-12-29 | Roni | perbaikan tracking approval yang tiba-tiba complete
                    UnitOfWork.GetConnection().ExecuteScalarAsync("exec SP_TERMINATION_UPDATE_ABNORMAL_TRACKING_APPROVAL @DocumentNumber", new { DocumentNumber = documentApproval.DocumentNumber });
                    //2022-12-29 | Roni | perbaikan tracking approval yang tiba-tiba complete

                    UnitOfWork.SaveChanges();

                    var trackingApprovals = TrackingApprovalRepository.Fetch()
                        .AsNoTracking()
                        .Where(x => x.DocumentApprovalId == documentApprovalId)
                        .ToList();

                    var minApprovalLevel = trackingApprovals.Where(x => x.ApprovalLevel == trackingApproval.ApprovalLevel)
                        .DefaultIfEmpty(new TrackingApproval())
                        .Min(x => x.ApprovalLevel);

                    var nextApprovers = trackingApprovals.Where(x => x.ApprovalLevel == minApprovalLevel && x.ApprovalActionCode == null);

                    var isComplete = nextApprovers == null || nextApprovers.Count() == 0;

                    var total = trackingApprovals
                        .Select(x => x.ApprovalLevel)
                        .Count();

                    var totalDone = trackingApprovals
                        .Where(x => !string.IsNullOrEmpty(x.ApprovalActionCode))
                        .Select(x => x.ApprovalLevel)
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
                    documentApproval.CurrentApprover = !isComplete ? nextApproverUsernames : null;

                    DocumentApprovalHistoryRepository.Add(DocumentApprovalHistory.Create(documentApproval.Id, ApprovalAction.Approve, actualOrganizationStructure, remarks));

                    if (isComplete)
                    {
                        // Fetch a single tracking approval record
                        var trackingApprovalData = TrackingApprovalRepository.Fetch()
                            .AsNoTracking()
                            .FirstOrDefault(x => x.DocumentApprovalId == documentApproval.Id && x.ApprovalActionCode == "initiate");

                        // Ensure the record exists before calling Complete
                        if (trackingApprovalData != null)
                        {
                            //Complete(trackingApprovalData.NoReg, documentApproval);
                        }
                        else
                        {
                            // Handle the case where no matching record is found
                            Console.WriteLine("No tracking approval data found for the specified document.");
                        }
                    }
                }

                if (eventNameLower != ApprovalAction.Initiate)
                {
                    documentApproval.LastApprovedBy = actualOrganizationStructure.NoReg;
                    documentApproval.LastApprovedOn = now;
                }

                UnitOfWork.SaveChanges();


            });

            return documentApproval;
        }
        #endregion
    }
}
