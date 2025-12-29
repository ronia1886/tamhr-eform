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
    public class LogTrackingService : DomainServiceBase
    {
        public LogTrackingService(IUnitOfWork unitOfWork)
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

        public IEnumerable<AbsenceLogTrackingStoredEntity> GetAbsenceLogTracking(DateTime dateFrom, DateTime dateTo, string NoReg, string PostCode, DateTime? createdDate)
        {
            return UnitOfWork.UspQuery<AbsenceLogTrackingStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, NoReg = NoReg, PostCode = PostCode, CreatedDate = createdDate });
        }

        public IEnumerable<OvertimeLogTrackingStoredEntity> GetOvertimeLogTracking(DateTime dateFrom, DateTime dateTo, string NoReg, string PostCode, DateTime? createdDate)
        {
            return UnitOfWork.UspQuery<OvertimeLogTrackingStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, NoReg = NoReg, PostCode = PostCode, CreatedDate = createdDate });
        }

        public List<Absence> GetPresenceCode()
        {
            return UnitOfWork.GetConnection().Query<Absence>(@"
            select * FROM TB_M_ABSENT
            ").ToList();
        }
    }
}
