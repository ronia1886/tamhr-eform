using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle printout matrix master data.
    /// </summary>
    public class PrintoutMatrixService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Printout matrix repository object.
        /// </summary>
        protected IRepository<PrintoutMatrix> PrintoutMatrixRepository => UnitOfWork.GetRepository<PrintoutMatrix>();

        /// <summary>
        /// Printout matrix readonly repository object.
        /// </summary>
        protected IReadonlyRepository<PrintoutMatrixView> PrintoutMatrixReadonlyRepository => UnitOfWork.GetRepository<PrintoutMatrixView>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated.
        /// </summary>
        private readonly string[] _properties = new[] {
            "FormId",
            "SubType",
            "ApproverLocation",
            "ApproverType",
            "Approver"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public PrintoutMatrixService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get printout matrix query
        /// </summary>
        /// <returns>Printout Matrix Query</returns>
        public IQueryable<PrintoutMatrixView> GetQuery()
        {
            return PrintoutMatrixReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Get all printout matrices
        /// </summary>
        /// <returns>List of Printout Matrices</returns>
        public IEnumerable<PrintoutMatrixView> Gets()
        {
            return GetQuery().ToList();
        }

        /// <summary>
        /// Get printout matrix by id
        /// </summary>
        /// <param name="id">Printout Matrix Id</param>
        /// <returns>Printout Matrix Object</returns>
        public PrintoutMatrixView Get(Guid id)
        {
            return PrintoutMatrixReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update or insert printout matrix
        /// </summary>
        /// <param name="printOutMatrix">Printout Matrix Object</param>
        public void Upsert(PrintoutMatrix printOutMatrix)
        {
            PrintoutMatrixRepository.Upsert<Guid>(printOutMatrix, _properties);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete printout matrix by id and its dependencies if any
        /// </summary>
        /// <param name="id">Printout Matrix Id</param>
        public void SoftDelete(Guid id)
        {
            var PrintOut = PrintoutMatrixRepository.FindById(id);

            PrintOut.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete printout matrix by id and its dependencies if any
        /// </summary>
        /// <param name="id">Printout Matrix Id</param>
        public void Delete(Guid id)
        {
            PrintoutMatrixRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        } 
        #endregion
    }
}
