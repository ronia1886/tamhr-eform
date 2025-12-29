using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle JSON category master data.
    /// </summary>
    public class JsonCategoryService : GenericDomainServiceBase<JsonCategory>
    {
        #region Variables & Properties
        protected IRepository<JsonCategory> JsonRepository => UnitOfWork.GetRepository<JsonCategory>();
        /// <summary>
        /// Field that hold properties that can be updated.
        /// </summary>
        protected override string[] Properties => new[] {
            "Category",
            "Title",
            "Description",
            "JsonValues",
            "OrderIndex"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public JsonCategoryService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public IQueryable<JsonCategory> GetJsonCategoriesQuery()
        {
            return JsonRepository.Fetch()
                .AsNoTracking();
        }
    }
}
