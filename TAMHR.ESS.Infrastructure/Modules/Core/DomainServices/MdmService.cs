using Agit.Common.Extensions;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle master data management
    /// </summary>
    public class MdmService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Departments readonly repository
        /// </summary>
        protected IReadonlyRepository<DepartmentsView> DepartmentsReadonlyRepository => UnitOfWork.GetRepository<DepartmentsView>();

        /// <summary>
        /// Organization structure readonly repository
        /// </summary>
        protected IReadonlyRepository<OrganizationStructureView> OrganizationStructureReadonlyRepository => UnitOfWork.GetRepository<OrganizationStructureView>();

        /// <summary>
        /// Actual organization structure readonly repository
        /// </summary>
        protected IReadonlyRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();

        /// <summary>
        /// Organization object readonly repository
        /// </summary>
        protected IReadonlyRepository<EmployeeeOrganizationObjectStoredEntity> OrganizationObjectStoredRepository => UnitOfWork.GetRepository<EmployeeeOrganizationObjectStoredEntity>();

        /// <summary>
        /// Organization Object readonly repository
        /// </summary>
        protected IReadonlyRepository<OrganizationObjectStoredEntity> OrganizationObjectRepository => UnitOfWork.GetRepository<OrganizationObjectStoredEntity>();

        /// <summary>
        /// Recreation reward member repository
        /// </summary>
        protected IRepository<RecreationRewardMember> RecreationRewardMemberRepository => UnitOfWork.GetRepository<RecreationRewardMember>();

        /// <summary>
        /// Recreation reward repository
        /// </summary>
        protected IRepository<RecreationReward> RecreationRewardRepository => UnitOfWork.GetRepository<RecreationReward>();

        /// <summary>
        /// Form validation matrix repository
        /// </summary>
        protected IRepository<FormValidationMatrix> FormValidationMatrixRepository => UnitOfWork.GetRepository<FormValidationMatrix>();

        /// <summary>
        /// Performance development readonly repository
        /// </summary>
        protected IReadonlyRepository<PerformanceDevelopment> PerformanceDevelopmentReadonlyRepository => UnitOfWork.GetRepository<PerformanceDevelopment>();

        /// <summary>
        /// Config classification repository
        /// </summary>
        protected IRepository<ConfigClassification> ConfigClassificationRepository => UnitOfWork.GetRepository<ConfigClassification>();

        /// <summary>
        /// Organizational assignment repository
        /// </summary>
        protected IRepository<OrganizationalAssignment> OrganizationalAssignmentRepository => UnitOfWork.GetRepository<OrganizationalAssignment>();

        /// <summary>
        /// Actual reporting structure readonly repository
        /// </summary>
        protected IReadonlyRepository<ActualReportingStructureView> ActualReportingStructureReadonlyRepository => UnitOfWork.GetRepository<ActualReportingStructureView>();
        #endregion

        #region Constructor
        public MdmService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Organization Area
        /// <summary>
        /// Get chief by noreg
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Employee Chief Object</returns>
        public IEnumerable<EmployeeAllChiefStoredEntity> GetAllChief(string noreg, int level)
        {
            return UnitOfWork.UdfQuery<EmployeeAllChiefStoredEntity>(new { noreg, level });
        }

        /// <summary>
        /// Get actual organization structure by noreg
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Actual Organization Structure Object</returns>
        public ActualOrganizationStructure GetActualOrganizationStructure(string noreg)
        {
            return ActualOrganizationStructureRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Staffing == 100)
                .FirstOrDefault();
        }

        public OrganizationalAssignment GetOrganizationalAssignment(string noreg)
        {
            return OrganizationalAssignmentRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate)
                .FirstOrDefault();
        }

        public OrganizationLevelViewModel GetOrganizationLevel(string noreg, string postCode, string objectDescription = "")
        {
            var actualOrganizationStruture = GetActualOrganizationStructure(noreg, postCode);
            var organizationLevel = new OrganizationLevelViewModel { OrgCode = actualOrganizationStruture.OrgCode, OrgLevel = actualOrganizationStruture.OrgLevel };

            if (!string.IsNullOrEmpty(objectDescription))
            {
                var now = DateTime.Now.Date;
                var set = UnitOfWork.GetRepository<OrganizationObject>();

                var organizationObject = set.Fetch()
                    .FirstOrDefault(x => now >= x.StartDate && now <= x.EndDate && actualOrganizationStruture.Structure.Contains("(" + x.ObjectID + ")") || x.ObjectID == actualOrganizationStruture.OrgCode && x.ObjectDescription == objectDescription);

                if (organizationObject != null)
                {
                    organizationLevel.OrgCode = organizationObject.ObjectID;
                    organizationLevel.OrgLevel = 0;
                }
            }

            return organizationLevel;
        }

        /// <summary>
        /// Get actual organization structure by noreg and position code
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="postCode">Position Code</param>
        /// <returns>Actual Organization Structure Object</returns>
        public ActualOrganizationStructure GetActualOrganizationStructure(string noreg, string postCode)
        {
            return ActualOrganizationStructureRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.PostCode == postCode)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get list of actual organization structure by noreg
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>List of Actual Organization Structure Object</returns>
        public IEnumerable<ActualOrganizationStructure> GetActualOrganizationStructures(string noreg)
        {
            return ActualOrganizationStructureRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg)
                .ToList();
        }

        /// <summary>
        /// Get employee organization objects and its parents by noreg
        /// </summary>
        /// <param name="noReg">NoReg</param>
        /// <returns>List of Organization Objects</returns>
        public IEnumerable<EmployeeeOrganizationObjectStoredEntity> GetEmployeeOrganizationObjects(string noReg)
        {
            return UnitOfWork.UspQuery<EmployeeeOrganizationObjectStoredEntity>(new { NoReg = noReg });
        }

        /// <summary>
        /// Get employee organization objects and its parents by noreg
        /// </summary>
        /// <param name="noReg">NoReg</param>
        /// <returns>List of Organization Objects</returns>
        public IEnumerable<EmployeeeOrganizationObjectStoredEntity> GetEmployeeOrganizationObjects(string noReg, string postCode)
        {
            return UnitOfWork.UspQuery<EmployeeeOrganizationObjectStoredEntity>(new { NoReg = noReg, PostCode = postCode });
        }

        /// <summary>
        /// Get list of organization objects by object type and description
        /// </summary>
        /// <param name="objectType">Object Type</param>
        /// <param name="objectDescription">Object Description</param>
        /// <returns>List of Organization Objects</returns>
        public IEnumerable<OrganizationObjectStoredEntity> GetOrganizationObjects(string objectType, string objectDescription, string noReg, string postCode)
        {
            return UnitOfWork.UspQuery<OrganizationObjectStoredEntity>(new { objectType , objectDescription, noReg, postCode });
        }
 
        /// <summary>
        /// Get list of employee by department
        /// </summary>
        /// <param name="orgCode">Organization Code</param>
        /// <returns>List of Departments</returns>
        public IEnumerable<EmployeeDepartmentStroredEntity> GetEmployeeByDepartment(string orgCode)
        {
            return UnitOfWork.UspQuery<EmployeeDepartmentStroredEntity>(new { OrgCode = orgCode });
        }

        /// <summary>
        /// Get employee by department PTA
        /// </summary>
        /// <param name="orgCode">Organization Code</param>
        /// <param name="Type">Request Type</param>
        /// <param name="Date">Validation Date</param>
        /// <param name="MaxAmount">Max Ammount Number</param>
        /// <returns>List of Departments</returns>
        public IEnumerable<EmployeeDepartmentPtaStroredEntity> GetEmployeeByDepartmentPta(string orgCode, string Type, DateTime Date, decimal MaxAmount )
        {
            var validationForm = FormValidationMatrixRepository.Find(x => x.RequestType == Type).FirstOrDefault();
            DateTime StartDate = new DateTime(Date.AddYears(validationForm.PeriodYear.Value - 1).Year, 1, 1);
            DateTime EndDate = new DateTime(Date.Year, 12, 31);

            return UnitOfWork.UspQuery<EmployeeDepartmentPtaStroredEntity>(new { OrgCode = orgCode, Type, StartDate, EndDate, MaxAmount });
        }

        public IEnumerable<EmployeeDivisionPtaStroredEntity> GetEmployeeByDivisionPta(string orgCode, string Type, DateTime Date, decimal MaxAmount)
        {
            var validationForm = FormValidationMatrixRepository.Find(x => x.RequestType == Type).FirstOrDefault();
            DateTime StartDate = new DateTime(Date.AddYears(validationForm.PeriodYear.Value - 1).Year, 1, 1);
            DateTime EndDate = new DateTime(Date.Year, 12, 31);

            return UnitOfWork.UspQuery<EmployeeDivisionPtaStroredEntity>(new { OrgCode = orgCode, Type, StartDate, EndDate, MaxAmount });
        }

        /// <summary>
        /// Get employee by department vacation
        /// </summary>
        /// <param name="orgCode">Organization Code</param>
        /// <param name="keyDate">Key Date</param>
        /// <returns>List of Employee by Department Vacation</returns>
        public IEnumerable<EmployeeDepartmentVacationStroredEntity> GetEmployeeByDepartmentVacation(string orgCode, DateTime keyDate)
        {
            return UnitOfWork.UspQuery<EmployeeDepartmentVacationStroredEntity>(new { OrgCode = orgCode, keyDate });
        }

        public IEnumerable<EmployeeDivisionVacationStroredEntity> GetEmployeeByDivisionVacation(string orgCode, DateTime keyDate)
        {
            return UnitOfWork.UspQuery<EmployeeDivisionVacationStroredEntity>(new { OrgCode = orgCode, keyDate });
        }

        public IEnumerable<EmployeeDepartmentShiftStroredEntity> GetEmployeeByDepartmentShift(string orgCode, string noreg)
        {
            return UnitOfWork.UspQuery<EmployeeDepartmentShiftStroredEntity>(new { OrgCode = orgCode, Noreg = noreg});
        }
        /// <summary>
        /// Get list of employee allowance by department
        /// </summary>
        /// <param name="orgCode">Organization Code</param>
        /// <returns>List of Employee</returns>
        public IEnumerable<EmployeeAllowanceStroredEntity> GetEmployeeAllowanceByDepartment(string orgCode, string allowanceType)
        {
            return UnitOfWork.UspQuery<EmployeeAllowanceStroredEntity>(new { OrgCode = orgCode, AllowanceType = allowanceType });
        }

        /// <summary>
        /// Get list of employee shift meal allowance by department
        /// </summary>
        /// <param name="orgCode">Organization Code</param>
        /// <returns>List of Employee with allowance & attendance date</returns>
        public IEnumerable<EmployeeShiftMealAllowanceStroredEntity> GetEmployeeShiftMealAllowanceByDepartment(string orgCode, DateTime period)
        {
            return UnitOfWork.UspQuery<EmployeeShiftMealAllowanceStroredEntity>(new { OrgCode = orgCode, Period = period.ToString("yyyy-MM-dd") });
        }

        /// <summary>
        /// Get date specification by noreg
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Date Specification Object</returns>
        public DateSpecification GetDateSpecification(string noreg)
        {
            var date = DateTime.Now.Date;

            return UnitOfWork.SqlQuery<DateSpecification>(new { NoReg = noreg }).Where(x => x.StartDate.Date <= date && x.EndDate >= date).FirstOrDefaultIfEmpty();
        }

        public int GetTotalSubordinate(DateTime keyDate, string orgCode, int min, int max)
        {
            var dynamicParameters = new DynamicParameters();

            dynamicParameters.Add("@keyDate", keyDate);
            dynamicParameters.Add("@orgCode", orgCode);
            dynamicParameters.Add("@min", min);
            dynamicParameters.Add("@max", max);

            return UnitOfWork.GetConnection().ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.SF_GET_EMPLOYEE_BY_CLASS_RANGE(@orgCode, @keyDate, @min, @max)", dynamicParameters);
        }

        public IEnumerable<EmployeeClassStroredEntity> GetEmployeeByClassRange(string orgCode, DateTime keyDate, int min, int max)
        {
            return UnitOfWork.UdfQuery<EmployeeClassStroredEntity>(new { orgCode, keyDate, min, max });
        }

        public IEnumerable<ObjectDescriptionStoredEntity> GetObjectDescriptions(string orgCode, DateTime keyDate)
        {
            return UnitOfWork.UdfQuery<ObjectDescriptionStoredEntity>(new { orgCode, keyDate });
        }

        public Task<IEnumerable<OrganizationStructureView>> GetOrganizationStructuresAsync()
        {
            return OrganizationStructureReadonlyRepository.Fetch()
                .AsNoTracking()
                .OrderBy(x => x.ParentOrgCode)
                .FromCacheAsync("mdm_organization_structure");
        }

        public ConfigClassification GetConfigClassification(string noreg, DateTime keyDate)
        {
            var oa = OrganizationalAssignmentRepository.Fetch()
                .AsNoTracking()
                .Where(x => keyDate >= x.StartDate && keyDate <= x.EndDate && x.NoReg == noreg)
                .FirstOrDefaultIfEmpty();

            var configClassification = ConfigClassificationRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Code == oa.EmployeeSubgroup && x.Category == "EmployeeSubgroup")
                .FirstOrDefaultIfEmpty();

            return configClassification;
        }

        public async Task<IEnumerable<ActualReportingStructureView>> GetActualReportingStructuresAsync(string noreg, string postCode)
        {
            return await ActualReportingStructureReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.PostCode == postCode)
                .OrderBy(x => x.HierarchyLevel)
                .ToListAsync();
        }

        public IQueryable<DepartmentsView> GetDepartments()
        {
            return DepartmentsReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        public IEnumerable<SubordinateFamilyStroredEntity> GetSubordinateFamilies(string orgCode, int orgLevel)
        {
            var output = UnitOfWork.UdfQuery<SubordinateFamilyStroredEntity>(new { orgCode, orgLevel });

            return output;
        }
        #endregion

        public IQueryable<PerformanceDevelopment> GetPerformanceDevelopments(string noreg)
        {
            return PerformanceDevelopmentReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg);
        }

        public IEnumerable<EmployeeOrganizationStoredEntity> GetEmployeeByOrgCode(string orgCode, string keyDate)
        {
            var output = UnitOfWork.UdfQuery<EmployeeOrganizationStoredEntity>(new { orgCode, keyDate });

            return output;
        }

        public IEnumerable<ActualReportingStructureView> GetActualReportingStructuresParent(string noreg, string postCode)
        {
            return ActualReportingStructureReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.ParentNoReg == noreg && x.ParentPostCode == postCode)
                .OrderBy(x => x.HierarchyLevel);
        }

        public IEnumerable<AyoSekolahReportStoredEntity> GetAyoSekolahReport(string noreg, string username, string orgCode)
        {
            return UnitOfWork.UdfQuery<AyoSekolahReportStoredEntity>(new { noreg, username, orgCode });
        }

        public DataSourceResult GetMaternityLeaveReport(DataSourceRequest request, string noreg, string username, string postCode)
        {
            var data = UnitOfWork
            .UdfQuery<MaternityLeaveReportStoredEntity>(new { noreg, username, postCode})
            .OrderBy(x => x.Division);

            return data.ToDataSourceResult(request);
        }
    }
}
