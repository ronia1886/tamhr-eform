using System;
using TAMHR.ESS.Domain;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections;
using Agit.Common.Extensions;
using System.Linq;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class SimpleDocumentRequestDetailViewModel<T> where T : class
    {
        public Guid DocumentApprovalId { get; private set; }
        public int Progress { get; private set; }
        public string DocumentNumber { get; private set; }
        public string Division { get; private set; }
        public string EmployeeSubgroup { get; private set; }
        public string EmployeeSubgroupText { get; private set; }
        public string DocumentStatusCode { get; private set; }
        public string Requester { get; private set; }
        public string RequesterName { get; private set; }
        public string Gender { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public DateTime? SubmitOn { get; private set; }
        public DateTime? LastApprovedOn { get; private set; }
        public T Object { get; private set; }
        public SimpleDocumentRequestDetailViewModel(DocumentRequestDetailStoredEntity input)
        {
            DocumentApprovalId = input.Id;
            Progress = input.Progress;
            DocumentNumber = input.DocumentNumber;
            DocumentStatusCode = input.DocumentStatusCode;
            Requester = input.CreatedBy;
            RequesterName = input.Name;
            Gender = input.Gender;
            Division = input.Division;
            EmployeeSubgroup = input.EmployeeSubgroup;
            EmployeeSubgroupText = input.EmployeeSubgroupText;
            CreatedOn = input.CreatedOn;
            SubmitOn = input.SubmitOn;
            LastApprovedOn = input.LastApprovedOn;
            Object = JsonConvert.DeserializeObject<T>(input.ObjectValue);
        }
    }

    public class DocumentRequestDetailViewModel
    {
        protected object _value = null;
        protected string _objectValue = null;
        public string FormKey { get; set; }
        public int Progress { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentStatusCode { get; set; }
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string ReferenceTable { get; set; }
        public string RequestTypeCode { get; set; }
        public string FormRoleKey { get; set; }
        public string Permision { get; set; }
        public string Requester { get; set; }
        public DateTime CreateOn { get; set; }
        public bool CanDownload { get; set; }
        [JsonIgnore]
        public IEnumerable<string> Permissions
        {
            get
            {
                var permissions = (Permision ?? string.Empty).Split(",").Select(x => x.Trim());

                return permissions;
            }
        }
        public IEnumerable<DocumentApprovalFile> Attachments { get; set; }
        //public IEnumerable<DocumentApprovalHistory> ApprovalHistories { get; set; }

        public string ObjectValue
        {
            get
            {
                if (string.IsNullOrEmpty(_objectValue) && _value != null)
                {
                    _objectValue = JsonConvert.SerializeObject(_value);
                }

                return _objectValue;
            }
            set
            {
                _objectValue = value;
            }
        }
    }

    public class ParentDocumentRequestDetailViewModel<T> where T : class
    {
        public Guid Id { get; set; }
        public string FormKey { get; set; }
        public string ObjectValue { get; set; }
        public IEnumerable<DocumentRequestDetailViewModel<T>> RequestDetails { get; set; }
    }

    public class DocumentRequestDetailViewModel<T> : DocumentRequestDetailViewModel where T : class
    {
        public T Object
        {
            get
            {
                if (_value == null)
                {
                    try
                    {
                        _value = !string.IsNullOrEmpty(ObjectValue)
                        ? JsonConvert.DeserializeObject<T>(ObjectValue)
                        : (typeof(IEnumerable).IsAssignableFrom(typeof(T)) ? null : Activator.CreateInstance<T>());
                    }
                    catch(Exception e)
                    {
                        
                        //LogService.InsertLog("Data Changes", "Update", "WeeklyWFHPlanning", "false", e.ToString(), "system");
                        throw new Exception("Something went wrong when submitting changes.");
                    }
                    
                }

                return (T)_value;
            }
            set
            {
                _value = value;
            }
        }

        public void Refresh()
        {
            ObjectValue = JsonConvert.SerializeObject(Object);
        }

        public static DocumentRequestDetailViewModel<T> Create(DocumentApproval approval, DocumentRequestDetail detail)
        {
            var documentRequestDetailViewModel = new DocumentRequestDetailViewModel<T>
            {
                Id = detail.Id,
                Requester = approval.CreatedBy,
                DocumentStatusCode = approval.DocumentStatusCode,
                Progress = approval.Progress,
                DocumentApprovalId = detail.DocumentApprovalId,
                DocumentNumber = approval.DocumentNumber,
                ObjectValue = detail.ObjectValue,
                CreateOn = detail.CreatedOn
            };

            return documentRequestDetailViewModel;
        }

        public static DocumentRequestDetailViewModel<T> Create(DocumentRequestDetail detail,string formRoleKey, string permision, string requester)
        {
            var documentRequestDetailViewModel = new DocumentRequestDetailViewModel<T>
            {
                Id = detail.Id,
                DocumentStatusCode = detail.DocumentApproval?.DocumentStatusCode,
                Progress = detail.DocumentApproval != null ? detail.DocumentApproval.Progress : 0,
                DocumentApprovalId = detail.DocumentApprovalId,
                DocumentNumber = detail.DocumentApproval?.DocumentNumber,
                ReferenceId = detail.ReferenceId,
                ReferenceTable = detail.ReferenceTable,
                RequestTypeCode = detail.RequestTypeCode,
                ObjectValue = detail.ObjectValue,
                Attachments = detail.DocumentApproval?.DocumentApprovalFiles,
                CanDownload = detail.DocumentApproval != null && detail.DocumentApproval.Form != null ? detail.DocumentApproval.Form.CanDownload : false,
                //ApprovalHistories = detail.DocumentApproval?.DocumentApprovalHistories,
                FormRoleKey = string.IsNullOrEmpty(formRoleKey) ? "" : formRoleKey,
                Permision = string.IsNullOrEmpty(permision) ? "" : permision,
                Requester = string.IsNullOrEmpty(requester) ? "" : requester,
                CreateOn = detail.CreatedOn
            };

            return documentRequestDetailViewModel;
        }

        public static IEnumerable<DocumentRequestDetailViewModel<T>> Create(DocumentRequestDetail[] details)
        {
            var list = new List<DocumentRequestDetailViewModel<T>>();

            details.ForEach(x =>
            {
                list.Add(Create(x, null, null, null));
            });

            return list;
        }
    }
}
