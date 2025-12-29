using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle absence master data.
    /// </summary>
    public class AbsenceService : GenericDomainServiceBase<Absence>
    {
        #region Domain Repositories
        /// <summary>
        /// Absence summary repository object.
        /// </summary>
        protected IRepository<AbsenceSummary> AbsenceSummaryRepository => UnitOfWork.GetRepository<AbsenceSummary>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for absence entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "Name",
            "SubmitApplicationStart",
            "SubmitApplicationEnd",
            "MaxAbsentDays",
            "MandatoryAttachment",
            "ActiveValidation",
            "AbsenceType",
            "Planning",
            "Unplanning",
            "CodePresensi"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public AbsenceService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of absences that has code presensi.
        /// </summary>
        /// <returns>This list of <see cref="Absence"/> objects.</returns>
        public IQueryable<Absence> GetDefaultCategories()
        {
            // Return list of absences that has code presensi.
            //return GetQuery().Where(x => x.CodePresensi.HasValue);

            // 20241031 | Saefudin | Display one of the duplicate data entries for CodePresensi and Name.
            var query = GetQuery().Where(x => x.CodePresensi.HasValue);

            //var grouped = query
            //    .GroupBy(x => new { x.CodePresensi, x.Name })
            //    .SelectMany(g => g.Count() > 1
            //        ? g.Where(x => x.Code.StartsWith("up-")) // 20241031 | Saefudin | Select the Code starting with "up-" if there are duplicate data entries.
            //        : g); // 20241031 | Saefudin | If there are no duplicates, retrieve all data.

            //return grouped;

            var grouped = query
                .GroupBy(x => new { x.CodePresensi, x.Name })
                .AsEnumerable() // pindah ke LINQ to Objects
                .SelectMany(g => g.Count() > 1
                    ? g.Where(x => x.Code.StartsWith("up-"))
                    : g);

            return grouped.AsQueryable();
        }

        /// <summary>
        /// Get absence summary by category.
        /// </summary>
        /// <param name="category">This absence category.</param>
        /// <returns>This dictionary of code presensi as key and absence name as value.</returns>
        public Dictionary<int, string> GetAbsencesBySummaryCategory(string category)
        {
            var presenceCodes = AbsenceSummaryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.SummaryCategoryCode == category && x.RowStatus==true)
                .Select(x => x.PresenceCode)
                .ToList();

            var idQuery = presenceCodes.AsQueryable();

            var outputData = CommonRepository.Fetch()
                .AsNoTracking();

            if(idQuery.Any())
            {
                outputData = outputData.Join(idQuery, // Gunakan IQueryable dari daftar ID
                  dtjoin => (dtjoin.CodePresensi ?? -1),
                  idItem => idItem,
                  (dtjoin, idItem) => dtjoin);
            }

            var output = outputData//.Where(x => presenceCodes.Contains(x.CodePresensi ?? -1))
                .Select(x => new { x.CodePresensi.Value, x.Name })
                .OrderBy(x => x.Value)
                .Distinct()
                .ToDictionary(x => x.Value, x => x.Name);

            return output;
        }

        /// <summary>
        /// Check whether absence data with specified id exist or not.
        /// </summary>
        /// <param name="id">This absence id.</param>
        /// <returns>True if exist, false otherwise.</returns>
        public bool Contains(Guid id)
        {
            // Determine whether absence with specified id exist or not.
            return GetQuery().Any(x => x.Id == id);
        }
        #endregion
    }
}
