using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TAMHR.ESS.Domain;


namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class PayrollReportService : DomainServiceBase
    {
        public PayrollReportService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

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
            var DirectorateQueryable = DirectorateList.AsQueryable();

            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Division");

            if(DirectorateQueryable.Any())
            {
                div = div.Join(DirectorateQueryable, // Gunakan IQueryable dari daftar ID
                  x => x.ParentOrgCode,
                  idItem => idItem,
                  (x, idItem) => x);
            }
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
            var DivisionQueryable = DivisionList.AsQueryable();

            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch().AsNoTracking().Where(x => x.ObjectDescription == "Department");
            if (DivisionQueryable.Any())
            {
                div  = div.Join(DivisionQueryable, // Gunakan IQueryable dari daftar ID
                  x => x.ParentOrgCode,
                  idItem => idItem,
                  (x, idItem) => x);
            }
                
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
            var DepartmentQueryable = DepartmentList.AsQueryable();
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Section");
            if (DepartmentQueryable.Any())
            {
                div = div.Join(DepartmentQueryable, // Gunakan IQueryable dari daftar ID
                  x => x.ParentOrgCode,
                  idItem => idItem,
                  (x, idItem) => x);
            }
            
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetLinesFromMultiSection(List<string> SectionList)
        {
            var SectionQueryable = SectionList.AsQueryable();
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Line");
            if (SectionQueryable.Any())
            {
                div = div.Join(SectionQueryable, // Gunakan IQueryable dari daftar ID
                  x => x.ParentOrgCode,
                  idItem => idItem,
                  (x, idItem) => x);
            }
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetGroupsFromMultiLine(List<string> LineList)
        {
            var LineQueryable = LineList.AsQueryable();
            var set = UnitOfWork.GetRepository<OrganizationStructureView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Group");
            if (LineQueryable.Any())
            {
                div = div.Join(LineQueryable, // Gunakan IQueryable dari daftar ID
                  x => x.ParentOrgCode,
                  idItem => idItem,
                  (x, idItem) => x);
            }
            
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public List<Vaccine> GetClass()
        {
            return UnitOfWork.GetConnection().Query<Vaccine>(@"
            select DISTINCT EmployeeSubgroupText as Class, OrderNumber
            FROM MDM_ACTUAL_ORGANIZATION_STRUCTURE aos 
            INNER JOIN MDM_EMPLOYEE_SUBGROUPNP es ON aos.EmployeeSubgroup = es.EmployeeSubgroup
            ORDER BY OrderNumber
            ").ToList();
        }

        public IEnumerable<PayrollSummaryReportStoredEntity> GetPayrollSummaryReport(DateTime keyDate)
        {
            return UnitOfWork.UspQuery<PayrollSummaryReportStoredEntity>(new { KeyDate = keyDate });
        }

        public IEnumerable<PayrollDetailReportStoredEntity> GetPayrollDetailReport(DateTime keyDate)
        {
            return UnitOfWork.UspQuery<PayrollDetailReportStoredEntity>(new { KeyDate = keyDate });
        }
        public IEnumerable<EmployeeHRHierarchiesStoredEntity> GetHRHierarchies(DateTime KeyDate)
        {
            return UnitOfWork.UdfQuery<EmployeeHRHierarchiesStoredEntity>(new { keyDate = KeyDate });
        }
    }
}
