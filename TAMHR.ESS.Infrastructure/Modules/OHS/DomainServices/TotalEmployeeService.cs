using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;
using Agit.Domain;
using Agit.Domain.Repository;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Agit.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class TotalEmployeeService : DomainServiceBase
    {
        #region Constructor
        //private readonly ITotalEmployeeRepository _repo;
        //private readonly IServiceProxy _serviceProxy;
        private readonly ILogger<TotalEmployeeService> _logger;
        private readonly IReadonlyRepository<TotalEmployeeViewModel> _readOnlyRepo;
        public TotalEmployeeService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        #endregion

        #region Domain Repository
        // Repository untuk TotalEmployee
        protected IRepository<TotalEmployeeModel> TotalEmployeeTBRepository => UnitOfWork.GetRepository<TotalEmployeeModel>();
        protected IReadonlyRepository<TotalEmployeeModel> TotalEmployeeReadOnlyTBRepository => UnitOfWork.GetRepository<TotalEmployeeModel>();
        protected IRepository<TotalEmployeeViewModel> TotalEmployeeViewRepository => UnitOfWork.GetRepository<TotalEmployeeViewModel>();
        protected IReadonlyRepository<TotalEmployeeViewModel> TotalEmployeeReadOnlyViewRepository => UnitOfWork.GetRepository<TotalEmployeeViewModel>();

        // Repository untuk Role Akses berdasarkan NoReg
        protected IRepository<RoleAreaActivitytModel> RoleAreaActivityRepository => UnitOfWork.GetRepository<RoleAreaActivitytModel>();
        protected IReadonlyRepository<RoleAreaActivitytModel> RoleAreaActivityReadOnlyRepository => UnitOfWork.GetRepository<RoleAreaActivitytModel>();
        #endregion


        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for bank entity.
        /// </summary>
        private readonly string[] Properties = new[] {
            //"Id",
            "TotalEmployee",
            "TotalEmployeeOutsourcing",
            "TotalWorkDay",
            "TotalOvertime",
            "DivisionCode",
            "DivisionName",
            "AreaId",
            "AreaName",
            "RowStatus",
            "DeletedOn"
        };
        #endregion
        public TotalEmployeeService(object @object, IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<TotalEmployeeViewModel> GetQuery()
        {
            var query = TotalEmployeeReadOnlyViewRepository.Fetch();
            query.Where(x => x.RowStatus == true);

            return query.AsNoTracking();
        }


        // Query LINQ untuk mengambil TotalEmployee dan TotalEmployeeOutsourcing
        public TotalEmployeeSummaryViewModel GetTotalEmployeeSummary(string periode = null, string divisionCode = null, string areaId = null, string NoReg = null)
        {
            if (periode == null)
            {
                periode = string.Empty;
            }
            if (divisionCode == null)
            {
                divisionCode = string.Empty;
            }
            if (areaId == null)
            {
                areaId = string.Empty;
            }

            // Mulai dari query dasar
            var query = GetQuery();

            // Tambahkan filter dinamis berdasarkan parameter yang diberikan
            if (!string.IsNullOrEmpty(periode))
            {
                // Parse periode menjadi bulan dan tahun
                var periodeDate = DateTime.ParseExact(periode, "yyyy-MM", null);
                var startDate = new DateTime(periodeDate.Year, periodeDate.Month, 1); // Awal bulan
                var endDate = startDate.AddMonths(1).AddDays(-1); // Akhir bulan

                // Filter IncidentDate dalam rentang bulan dan tahun yang diberikan
                query = query.Where(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate);
            }

            if (!string.IsNullOrEmpty(divisionCode))
            {
                query = query.Where(x => x.DivisionCode == divisionCode);
            }

            if (!string.IsNullOrEmpty(areaId))
            {
                query = query.Where(x => x.AreaId == areaId);
            }

            // **Filter berdasarkan hak akses NoReg**
            if (!string.IsNullOrEmpty(NoReg))
            {
                Console.WriteLine($"Filtering by NoReg: {NoReg}");

                // Ambil daftar role akses dari tabel RoleAreaActivity berdasarkan NoReg
                var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                    .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => new { a.DivisionCode, a.AreaId })
                    .ToList(); // Eksekusi query ke database

                var allowedAreaIds = userAccess
                    .Select(a => a.AreaId.ToString()) // Konversi Guid ke string untuk perbandingan
                    .Distinct()
                    .ToList();

                // Terapkan filter jika ada akses yang ditemukan
                if (allowedAreaIds.Any())
                {
                    query = query.Where(e =>
                        allowedAreaIds.Contains(e.AreaId)
                    );
                }
            }

            var summary = query.Select(e => new
                {
                    e.TotalEmployee,
                    e.TotalEmployeeOutsourcing
                })
                .AsEnumerable() // Eksekusi di memori jika menggunakan IQueryable
                .Aggregate(new TotalEmployeeSummaryViewModel(), (acc, curr) =>
                {
                    acc.TotalEmployee += curr.TotalEmployee;
                    acc.TotalEmployeeOutsourcing += curr.TotalEmployeeOutsourcing;
                    acc.Total = acc.TotalEmployee + acc.TotalEmployeeOutsourcing;
                    return acc;
                });

            return summary ?? new TotalEmployeeSummaryViewModel();
        }

        public IEnumerable<TotalEmployeeViewModel> Gets(
            string divisionCode = null,
            string areaId = null,
            string periode = null,
            string NoReg = null)
        {
            var query = GetQuery();

            // Filter RowStatus aktif
            query = query.Where(x => x.RowStatus == true);

            // Filter berdasarkan DivisionCode
            if (!string.IsNullOrEmpty(divisionCode))
            {
                Console.WriteLine($"Division: {divisionCode}");
                query = query.Where(e => e.DivisionCode == divisionCode);
            }

            // Filter berdasarkan AreaId
            if (!string.IsNullOrEmpty(areaId))
            {
                Console.WriteLine($"areaId: {areaId}");
                query = query.Where(e => e.AreaId == areaId);
            }

            // Filter berdasarkan Periode
            if (!string.IsNullOrEmpty(periode))
            {
                Console.WriteLine($"periode: {periode}");
                query = query.Where(e => e.CreatedOnSearch == periode);
            }

            // **Filter berdasarkan hak akses NoReg**
            if (!string.IsNullOrEmpty(NoReg))
            {
                Console.WriteLine($"Filtering by NoReg: {NoReg}");

                // Ambil daftar role akses dari tabel RoleAreaActivity berdasarkan NoReg
                var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                    .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => new { a.DivisionCode, a.AreaId })
                    .ToList(); // Eksekusi query ke database

                var allowedAreaIds = userAccess
                    .Select(a => a.AreaId.ToString()) // Konversi Guid ke string untuk perbandingan
                    .Distinct()
                    .ToList();

                // Terapkan filter jika ada akses yang ditemukan
                if (allowedAreaIds.Any())
                {
                    //query = query.Where(e =>
                    //    allowedAreaIds.Contains(e.AreaId)
                    //);
                    var allowedAreaQueryable = allowedAreaIds.AsQueryable();

                    query = query.Join(
                        allowedAreaQueryable,
                        a => a.AreaId,
                        id => id,
                        (a, id) => a
                    );
                }
            }

            return query.ToList(); // Eksekusi query terakhir
        }

        public TotalEmployeeViewModel GetById(Guid id)
        {
            return GetQuery().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }
        public void Upsert(TotalEmployeeModel param)
        {
            TotalEmployeeTBRepository.Upsert<Guid>(param, Properties);
            UnitOfWork.SaveChanges();
        }
        public void Delete(Guid id)
        {
            var item = TotalEmployeeTBRepository.Fetch()
                .Where(x => x.Id == id)
                .FirstOrDefault();

            if (item != null)
            {
                item.RowStatus = false;
                item.DeletedOn = DateTime.Now;
                TotalEmployeeTBRepository.Upsert<Guid>(item, Properties);
                UnitOfWork.SaveChanges();
            }
        }

    }
}