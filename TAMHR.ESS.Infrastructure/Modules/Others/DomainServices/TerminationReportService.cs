using Agit.Common.Extensions;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class TerminationReportService : DomainServiceBase
    {
        public TerminationReportService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }

        public IEnumerable<TerminationReportView> GetTerminationReport(DateTime? DateFrom, DateTime? DateTo)
        {
            return UnitOfWork.GetRepository<TerminationReportView>().Fetch().Where(wh => wh.EndDate >= DateFrom && wh.EndDate <= DateTo).OrderByDescending(ob => ob.EndDate).ThenBy(ob => ob.NoReg);
        }

        public IEnumerable<TerminationReportSummaryStoredEntity> GetTerminationReportSummary(DateTime? DateFrom, DateTime? DateTo, string category = "", bool showAll = true, string divisions = "", string emp_class = "", string documentStatus = "")
        {
            return UnitOfWork.UdfQuery<TerminationReportSummaryStoredEntity>(new { startDate = DateFrom, endDate = DateTo, category, showAll, divisions,emp_class, documentStatus });
        }

        public IQueryable<OrganizationStructureView> GetDirectorates()
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Directorate");
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetDivisions()
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Division");
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetDivisionsFromMultiDirectorate(List<string> DirectorateList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Division" && DirectorateList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetDepartments(string Division)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Department" && x.ParentOrgCode == Division);
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetDepartmentsFromMultiDivision(List<string> DivisionList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Department" && DivisionList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetSections(string Department)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Section" && x.ParentOrgCode == Department);
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetSectionsFromMultiDepartment(List<string> DepartmentList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Section" && DepartmentList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetLinesFromMultiSection(List<string> SectionList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Line" && SectionList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetGroupsFromMultiLine(List<string> LineList)
        {
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Group" && LineList.Contains(x.ParentOrgCode));
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public List<Vaccine> GetClass()
        {
            return UnitOfWork.GetConnection().Query<Vaccine>(@"
            select DISTINCT NK_SubKelas as Class from vw_personal_data_information Order By NK_SubKelas
            ").ToList();
        }

        public List<GeneralCategory> GetTerminationType()
        {
            return UnitOfWork.GetConnection().Query<GeneralCategory>(@"
            select * FROM TB_M_GENERAL_CATEGORY WHERE Category='TerminationType'
            ").ToList();
        }

        public List<GeneralCategory> GetDocumentStatus()
        {
            return UnitOfWork.GetConnection().Query<GeneralCategory>(@"
            select * FROM TB_M_GENERAL_CATEGORY WHERE Category='DocumentStatus'
            ").ToList();
        }

        public int UpdateTerminationDate(Guid id,DateTime EndDate,string noreg)
        {
            var set = UnitOfWork.GetRepository<Termination>();


            Termination data = set.FindById(id);
            var detail = UnitOfWork.GetRepository<DocumentRequestDetail>().Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.DocumentApprovalId == data.DocumentApprovalId);

            var json = JsonConvert.DeserializeObject<TerminationViewModel>(detail.ObjectValue);
            json.EndDate = EndDate;

            var oldDate = data.EndDate;
            var serializeData = JsonConvert.SerializeObject(data);
            string strDate = string.Format("{0:dd-MM-yyyy}", EndDate);
            serializeData  += ",{NewDate: " + strDate + "}";
            
            data.EndDate = EndDate;
            data.ModifiedBy = noreg;
            data.ModifiedOn = DateTime.Now;
            

            new LogService(UnitOfWork).InsertLog("Data Changes", "Update", "Termination", "false", serializeData, noreg);

            UnitOfWork.Transact((transaction) =>
            {
                UnitOfWork.GetRepository<DocumentRequestDetail>().Fetch()
                    .Where(x => x.DocumentApprovalId == data.DocumentApprovalId)
                    .Update(x => new DocumentRequestDetail
                    {
                        ObjectValue = JsonConvert.SerializeObject(json),
                        ModifiedBy = noreg,
                        ModifiedOn = DateTime.Now
                    });

                // Call the stored procedure SP_TERMINATION_UPDATE_ABNORMAL
                var updateEndDateResult = UnitOfWork.UspQuery<TerminationUpdateAbnormal>(
                    new
                    {
                        TerminationId = data.Id,
                        oldDate = oldDate
                    },
                    transaction
                );

                SendEmailTerminationUpdate(id,oldDate,EndDate);

            });
            return UnitOfWork.SaveChanges();
        }

        public bool SendEmailTerminationUpdate(Guid id,DateTime oldDate,DateTime newDate)
        {
            // Create my task view repository object.
            var set = UnitOfWork.GetRepository<TerminationReportView>();
            //cofig repositoy.
            var cf = UnitOfWork.GetRepository<Config>().Fetch();
            // Set default total failure to 0.
            var totalFailure = 0;
            var configService = new ConfigService(UnitOfWork);
            var emailService = new EmailService(UnitOfWork);

            // Get and set application url from configuration.
            var applicationUrl = configService.GetConfigValue<string>(Configurations.ApplicationUrl);

            var users = GetUsersByRole("HR_ADMIN");

            // Get list of task objects without object tracking.
            var dataTermination = set.Fetch().FirstOrDefault(x => x.Id == id);
            string ccEmail = null;

            //var ccTermMailStatus = configService.GetConfigValue<bool>("Email.TerminationCcStatus");
            //if (ccTermMailStatus)
            //{
            //    ccEmail = _configService.GetConfigValue<string>("Email.Cc");
            //}

            // Loop over task objects.
            // Create new email data.
            if (!users.IsEmpty())
            {
                var recipients = string.Join(",", users.Select(x => x.Email));
                var data = new
                {
                    names = string.Join(", ", users.Select(x => x.Name)),
                    document_type = dataTermination.Terminationtype,
                    document_number = dataTermination.DocumentNumber,
                    employee_name = dataTermination.Name,
                    termination_date = newDate,
                    // Get and set task link.
                    url = applicationUrl
                };

                // Send email notification.
                if (!emailService.SendEmail("termination-update",recipients, data, ccEmail))
                {
                    // Count the total failure.
                    totalFailure++;
                }

            }

            return true;
        }

        public IEnumerable<User> GetUsersByRole(string roleKey)
        {
            // Get list of users by role key.
            var users = (
                from u in UnitOfWork.GetRepository<User>().Fetch().AsNoTracking()
                join r in UnitOfWork.GetRepository<UserRole>().Fetch().AsNoTracking()
                on u.Id equals r.UserId
                where r.Role.RoleKey.Equals(roleKey)
                select r.User
            ).ToList();

            // Return the list.
            return users;
        }

    }
}
