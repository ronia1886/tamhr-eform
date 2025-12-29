using Agit.Common;
using Agit.Common.Attributes;
using Dapper;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TAMHR.ESS.Infrastructure.Web.Querying
{
    public class GenericSummaryBuilder
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly DynamicParameters _dynamicParameters = new DynamicParameters();

        public GenericSummaryBuilder(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<dynamic> BuildSummary<T>(string field, string category, Dictionary<string, object> parameters, string filter = null) where T : class
        {
            var type = typeof(T);
            var attribute = type.GetCustomAttributes(typeof(DatabaseObjectAttribute), true).FirstOrDefault() as DatabaseObjectAttribute;

            Assert.ThrowIf(attribute.DatabaseObjectType != DatabaseObjectType.TableValued, $"{attribute.Name} is not Table Valued Function");

            var whereClause = string.Empty;

            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    _dynamicParameters.Add(key, parameters[key]);
                }
            }

            var parameterString = parameters != null ? string.Join(", ", _dynamicParameters.ParameterNames.Select(x => "@" + x)) : string.Empty;

            var filterQuery = string.IsNullOrEmpty(filter) ? "1=1" : filter;

            var innerQuery = string.Format(@"SELECT * FROM {0}({1}) WHERE ({2})", attribute.Name, parameterString, filterQuery);
            var outerQuery = string.Format(@"SELECT * FROM dbo.TB_M_GENERAL_CATEGORY WHERE Category = '{0}'", category);
            var bodyQuery = string.Format(@"SELECT cat.Code, cat.Name, ISNULL(COUNT(src.{2}), 0) AS Total FROM ({0}) cat LEFT JOIN ({1}) src ON src.{2} = cat.Code GROUP BY cat.Code, cat.Name", outerQuery, innerQuery, field);
            DbContextOptions<UnitOfWork> options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<UnitOfWork>(), _unitOfWork.GetConnection(), sqlServerOptions => sqlServerOptions.CommandTimeout(_unitOfWork.GetConnection().ConnectionTimeout)).Options;
            UnitOfWork unitOfWork = new UnitOfWork(options);
            int? connectionTimeout = unitOfWork.Database.GetCommandTimeout();
            return _unitOfWork.GetConnection().Query(bodyQuery, _dynamicParameters.ParameterNames.Count() == 0 ? null : _dynamicParameters,null, buffered: true, connectionTimeout.Value);
        }
    }
}
