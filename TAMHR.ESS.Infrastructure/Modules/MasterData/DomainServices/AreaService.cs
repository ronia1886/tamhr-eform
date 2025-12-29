using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;
using Agit.Domain;
using Agit.Domain.Repository;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Agit.Common.Extensions;

namespace TAMHR.ESS.Infrastructure.DomainServices
{

    public class AreaService : DomainServiceBase
    {
        #region Domain Repository
        protected IRepository<AreaView> AreaViewRepository => UnitOfWork.GetRepository<AreaView>();
        protected IReadonlyRepository<AreaView> AreaViewReadonlyRepository => UnitOfWork.GetRepository<AreaView>();
        protected IRepository<AreaTB> AreaTBRepository => UnitOfWork.GetRepository<AreaTB>();
        protected IReadonlyRepository<AreaTB> AreaTBReadonlyRepository => UnitOfWork.GetRepository<AreaTB>();

        #endregion
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for bank entity.
        /// </summary>
        private readonly string[] Properties = new[] {
            //"Id",
            "NamaArea",
            "DivisiCode",
            "Alamat"

        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public AreaService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
        public IQueryable<AreaView> GetQuery()
        {
            return AreaViewReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        public IEnumerable<AreaView> Gets()
        {
            return GetQuery().ToList();
        }
        public AreaView Get(Guid id)
        {
            return AreaViewReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public IEnumerable<AreaView> getkategoriPenyakit()
        {
            return AreaViewRepository.Fetch()
                .AsNoTracking().ToList();
        }
        public IQueryable<AreaView> GetPopUpArea()
        {
            return AreaViewReadonlyRepository.Fetch()
                .AsNoTracking();
        }
        public AreaView GetPopUpArea(Guid id)
        {
            return GetPopUpArea().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public IQueryable<ActualEntityStructure> GetDivisions()
        {
            var set = UnitOfWork.GetRepository<ActualEntityStructure>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Division");
            return div.Select(x => new ActualEntityStructure { ObjectCode = x.ObjectCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public IQueryable<ActualEntityStructure> GetDivisionsAreaActivity(string NoReg = null)
        {
            var set = UnitOfWork.GetRepository<ActualEntityStructure>();
            var roleRepo = UnitOfWork.GetRepository<RoleAreaActivitytModel>(); // Repository akses role

            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Division");

            // Jika NoReg diberikan, filter berdasarkan akses pengguna
            if (!string.IsNullOrEmpty(NoReg))
            {
                var userAccessDivisions = roleRepo.Fetch()
                    .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => a.DivisionCode)
                    .Distinct()
                    .ToList(); // Eksekusi untuk mendapatkan daftar DivisionCode yang diperbolehkan

                if (userAccessDivisions.Any())
                {
                    //div = div.Where(x => userAccessDivisions.Contains(x.ObjectCode));
                    var allowedAreaQueryable = userAccessDivisions.AsQueryable();

                    div = div.Join(
                        allowedAreaQueryable,
                        a => a.ObjectCode,
                        id => id,
                        (a, id) => a
                    );
                }
            }

            return div.Select(x => new ActualEntityStructure
            {
                ObjectCode = x.ObjectCode,
                ObjectText = x.ObjectText
            })
                .Distinct()
                .OrderBy(x => x.ObjectText);
        }

        public IQueryable<object> GetAreaDropDown(string divisionCode = null)
        {

            //var set = UnitOfWork.GetRepository<AreaTB>();
            //var div = set.Fetch().AsNoTracking();

            var areaRepo = UnitOfWork.GetRepository<AreaTB>();
            var divisiRepo = UnitOfWork.GetRepository<ActualEntityStructure>();
            var area = areaRepo.Fetch().AsNoTracking();
            var divisi = divisiRepo.Fetch().AsNoTracking();

            if (!string.IsNullOrEmpty(divisionCode))
            {
                Console.WriteLine($"Division: {divisionCode}");
                area = area.Where(e => e.DivisiCode == divisionCode);
            }

            var result = from a in area
                         join d in divisi on a.DivisiCode equals d.ObjectCode // Pastikan properti join sesuai dengan tabel Anda
                         where d.ObjectDescription == "Division"
                         select new
                         {
                             Id = a.Id.ToString(),     // Mengubah Id dari Guid ke string
                             NamaArea = a.NamaArea + "-" + d.ObjectText,// Properti dari tabel Divisi
                         };

            return result.Distinct().OrderBy(x => x.NamaArea);
            //return div
            //    .Select(x => new
            //    {
            //        Id = x.Id.ToString(),  // Mengubah Id dari Guid ke string
            //        NamaArea = x.NamaArea
            //    })
            //    .Distinct()
            //    .OrderBy(x => x.NamaArea);
        }

        public IQueryable<object> GetAreaDropDownAreaActivity(string divisionCode = null, string NoReg = null)
        {
            var areaRepo = UnitOfWork.GetRepository<AreaTB>();
            var divisiRepo = UnitOfWork.GetRepository<ActualEntityStructure>();
            var roleRepo = UnitOfWork.GetRepository<RoleAreaActivitytModel>(); // Repository untuk akses role

            var area = areaRepo.Fetch().AsNoTracking();
            var divisi = divisiRepo.Fetch().AsNoTracking();

            // Ambil daftar akses berdasarkan NoReg
            if (!string.IsNullOrEmpty(NoReg))
            {
                var userAccess = roleRepo.Fetch()
                    .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => a.AreaId)
                    .ToList(); // Eksekusi query untuk mendapatkan daftar AreaId yang diperbolehkan

                if (userAccess.Any())
                {
                    //area = area.Where(e => userAccess.Contains(e.Id));
                    var allowedAreaQueryable = userAccess.AsQueryable();

                    area = area.Join(
                        allowedAreaQueryable,
                        a => a.Id,
                        id => id,
                        (a, id) => a
                    );
                }
            }

            // Jika divisionCode diberikan, tambahkan filter berdasarkan DivisionCode
            if (!string.IsNullOrEmpty(divisionCode))
            {
                Console.WriteLine($"Filtering by DivisionCode: {divisionCode}");
                area = area.Where(e => e.DivisiCode == divisionCode);
            }

            var result = from a in area
                         join d in divisi on a.DivisiCode equals d.ObjectCode
                         where d.ObjectDescription == "Division"
                         select new
                         {
                             Id = a.Id.ToString(),  // Ubah Guid ke string
                             NamaArea = a.NamaArea + " - " + d.ObjectText, // Gabungkan nama area & divisi
                         };

            return result.Distinct().OrderBy(x => x.NamaArea);
        }

        public IQueryable<object> GetFilterAreaDropDown(string divisionCode = null, string NoReg = null)
        {
            var areaRepo = UnitOfWork.GetRepository<AreaTB>();
            var divisiRepo = UnitOfWork.GetRepository<ActualEntityStructure>();
            var roleRepo = UnitOfWork.GetRepository<RoleAreaActivitytModel>(); // Repository untuk akses role

            var area = areaRepo.Fetch().AsNoTracking();
            var divisi = divisiRepo.Fetch().AsNoTracking();

            // Ambil daftar akses berdasarkan NoReg
            if (!string.IsNullOrEmpty(NoReg))
            {
                var userAccess = roleRepo.Fetch()
                    .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => a.AreaId)
                    .ToList(); // Eksekusi query untuk mendapatkan daftar AreaId yang diperbolehkan

                if (userAccess.Any())
                {
                    area = area.Where(e => userAccess.Contains(e.Id));
                }
            }

            // Jika divisionCode diberikan, tambahkan filter berdasarkan DivisionCode
            if (!string.IsNullOrEmpty(divisionCode))
            {
                Console.WriteLine($"Filtering by DivisionCode: {divisionCode}");
                area = area.Where(e => e.DivisiCode == divisionCode);
            }

            var result = from a in area
                         join d in divisi on a.DivisiCode equals d.ObjectCode // Pastikan properti join sesuai dengan tabel Anda
                         where d.ObjectDescription == "Division"
                         select new
                         {
                             Value = a.Id.ToString(),     // Mengubah Id dari Guid ke string
                             Text = a.NamaArea + "-" + d.ObjectText,// Properti dari tabel Divisi
                         };

            return result.Distinct().OrderBy(x => x.Text);

        }
        public IQueryable<AreaTB> GetAreas()
        {
            // Ambil repository untuk entitas Area
            var repository = UnitOfWork.GetRepository<AreaTB>();

            // Query untuk mendapatkan data area
            return repository.Fetch()
                .AsNoTracking()
                .Select(x => new AreaTB
                {
                    Id = x.Id,
                    NamaArea = x.NamaArea
                })
                .Distinct()
                .OrderBy(x => x.NamaArea);
        }
        public void Upsert(AreaTB param)
        {
            AreaTBRepository.Upsert<Guid>(param, Properties);

            UnitOfWork.SaveChanges();
        }

        public void Delete(Guid id)
        {
            AreaTBRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
    }
}
