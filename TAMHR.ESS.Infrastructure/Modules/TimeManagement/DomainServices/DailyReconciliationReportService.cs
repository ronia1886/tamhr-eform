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
    public class DailyReconciliationService : DomainServiceBase
    {
        public DailyReconciliationService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }

        public IEnumerable<DailyReconciliationReportView> GetDailyReconciliation(string EmployeeName,DateTime? DateFrom, DateTime? DateTo)
        {
             var data = UnitOfWork.GetRepository<DailyReconciliationReportView>().Fetch().Where(wh => wh.Name.Contains(EmployeeName) && wh.WorkingDate.Date >= DateFrom.Value.Date && wh.WorkingDate <= DateTo.Value.Date).OrderBy(ob => ob.NoReg).ThenBy(ob => ob.WorkingDate);
            return data; 
        }

        public IEnumerable<DailyReconciliationSummaryStoredEntity> GetDailyReconciliationSummary(DateTime? DateFrom, DateTime? DateTo, string category = "absence", bool showAll=false)
        {
            return UnitOfWork.UdfQuery<DailyReconciliationSummaryStoredEntity>(new { startDate = DateFrom, endDate = DateTo, category, showAll });
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
                .Where(x => x.ObjectDescription == "Division" 
                //&& DirectorateList.Contains(x.ParentOrgCode)
                ).Join(DirectorateQueryable, // Gunakan IQueryable dari daftar ID
                  x => x.ParentOrgCode,
                  idItem => idItem,
                  (x, idItem) => x);
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

            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Department"
                //&& DivisionList.Contains(x.ParentOrgCode)
                );

            if (DivisionQueryable.Any())
            {
                div = div.Join(DivisionQueryable, // Gunakan IQueryable dari daftar ID
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
                .Where(x => x.ObjectDescription == "Section"
                //&& DepartmentList.Contains(x.ParentOrgCode)
                );

            if (DepartmentQueryable.Any())
            {
                div = div.Join(DepartmentQueryable, // Gunakan IQueryable dari daftar ID
                  x => x.ParentOrgCode,
                  idItem => idItem,
                  (x, idItem) => x);
            }
            
            return div.Select(x => new OrganizationStructureView { OrgCode = x.OrgCode, ParentOrgCode = x.ParentOrgCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<OrganizationStructureView> GetSectionsFromMultiDepartmentAndDivision(List<string> DepartmentList, List<string> DivisionList)
        {
            var combinedParentList = DepartmentList.Union(DivisionList).ToList();
            var combinedParentQueryable = combinedParentList.AsQueryable();

            var set = UnitOfWork.GetRepository<OrganizationStructureView>();

            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Section"
                //&& (DepartmentList.Contains(x.ParentOrgCode)|| DivisionList.Contains(x.ParentOrgCode))
                );

            if (combinedParentQueryable.Any())
            {
                div = div.Join(combinedParentQueryable, // Gunakan IQueryable dari daftar ID
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
                .Where(x => x.ObjectDescription == "Line"
                //&& SectionList.Contains(x.ParentOrgCode)
                );

            if (SectionQueryable.Any()){
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
                .Where(x => x.ObjectDescription == "Group"
                //&& LineList.Contains(x.ParentOrgCode)
                );

            if(LineQueryable.Any())
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
            select DISTINCT NK_SubKelas as Class from vw_personal_data_information Order By NK_SubKelas
            ").ToList();
        }

        public List<Absence> GetPresenceCode()
        {
            return UnitOfWork.GetConnection().Query<Absence>(@"
            select * FROM TB_M_ABSENT Order by codepresensi
            ").ToList();
        }
    }
}
