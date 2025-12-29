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

    public class KategoriPenyakitService : DomainServiceBase
    {
        #region Domain Repository
        protected IRepository<Kategori_PenyakitVIEW> Kategori_PenyakitViewRepository => UnitOfWork.GetRepository<Kategori_PenyakitVIEW>();
        protected IRepository<Kategori_Penyakit> Kategori_PenyakitRepository => UnitOfWork.GetRepository<Kategori_Penyakit>();
        protected IReadonlyRepository<Kategori_Penyakit> Kategori_PenyakitReadonlyRepository => UnitOfWork.GetRepository<Kategori_Penyakit>();
        protected IReadonlyRepository<Kategori_PenyakitVIEW> Kategori_PenyakitViewReadonlyRepository => UnitOfWork.GetRepository<Kategori_PenyakitVIEW>();
        #endregion
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for bank entity.
        /// </summary>
        private readonly string[] Properties = new[] {
            //"Id",
            "IdTingkatSakit",
            "KategoriPenyakit",
            "Penyakit"

        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public KategoriPenyakitService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
        public IQueryable<Kategori_PenyakitVIEW> GetQuery()
        {
            return Kategori_PenyakitViewReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        public IEnumerable<Kategori_PenyakitVIEW> Gets()
        {
            return GetQuery().ToList();
        }
        public Kategori_PenyakitVIEW Get(Guid id)
        {
            return Kategori_PenyakitViewReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public IEnumerable<Kategori_PenyakitVIEW> getkategoriPenyakit()
        {
            return Kategori_PenyakitViewRepository.Fetch()
                .AsNoTracking().ToList();
        }
        public IQueryable<Kategori_PenyakitVIEW> GetPopUpKategoriPenyakit()
        {
            return Kategori_PenyakitViewReadonlyRepository.Fetch()
                .AsNoTracking();
        }
        public Kategori_PenyakitVIEW GetPopUpKategoriPenyakit(Guid id)
        {
            return GetPopUpKategoriPenyakit().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }
        public IEnumerable<Absence> GetAbsenceByCodePresensi(IEnumerable<int?> ids)
        {
            var item = UnitOfWork.GetRepository<Absence>();
            var idQuery = ids
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .AsQueryable();

            var dbitems = item.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus)
                .Join(
                    idQuery,                     // daftar ID yang dicari
                    x => (x.CodePresensi), // kolom di tabel (handle null)
                    id => id,                    // ID dari list
                    (x, id) => x                 // hasil join
                )
                .ToList();

            return dbitems;
        }

        public void Upsert(Kategori_Penyakit param)
        {
            Kategori_PenyakitRepository.Upsert<Guid>(param,Properties);

            UnitOfWork.SaveChanges();
        }

        public void Delete(Guid id)
        {
            Kategori_PenyakitRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
    }
}
