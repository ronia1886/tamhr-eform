using Agit.Domain;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common.Attributes;
using Agit.Common;
using Microsoft.EntityFrameworkCore;

namespace TAMHR.ESS.Infrastructure.Web.Querying
{
    public class GenericDataQuery
    {
        private const string _tableAlias = "gdq";
        private const string _countQueryTemplate = "SELECT COUNT(*) FROM ({0}) AS {1} WHERE ({2})";
        private const string _queryTemplate = "SELECT TOP {0} * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY {4}) as row_number FROM (SELECT * FROM ({1}) AS {2}) AS {2} WHERE ({3})) AS {2} WHERE {2}.row_number >= {5}";
        private readonly Dictionary<FilterOperator, string> _operators = new Dictionary<FilterOperator, string>();
        private readonly UnitOfWork _unitOfWork;
        private readonly DataSourceRequest _dataSourceRequest;
        private readonly DynamicParameters _dynamicParameters = new DynamicParameters();

        public GenericDataQuery(UnitOfWork unitOfWork, DataSourceRequest dataSourceRequest)
        {
            _unitOfWork = unitOfWork;
            _dataSourceRequest = dataSourceRequest;

            _operators.Add(FilterOperator.Contains, "{0} LIKE '%' + {1} + '%'");
            _operators.Add(FilterOperator.DoesNotContain, "{0} NOT LIKE '%' + {1} + '%'");
            _operators.Add(FilterOperator.EndsWith, "{0} LIKE '%' + {1}");
            _operators.Add(FilterOperator.StartsWith, "{0} LIKE {1} + '%'");
            _operators.Add(FilterOperator.IsNull, "{0} IS NULL");
            _operators.Add(FilterOperator.IsNotNull, "{0} IS NOT NULL");
            _operators.Add(FilterOperator.IsNotEqualTo, "{0} <> {1}");
            _operators.Add(FilterOperator.IsNotEmpty, "{0} <> ''");
            _operators.Add(FilterOperator.IsLessThanOrEqualTo, "{0} <= {1}");
            _operators.Add(FilterOperator.IsLessThan, "{0} < {1}");
            _operators.Add(FilterOperator.IsGreaterThanOrEqualTo, "{0} >= {1}");
            _operators.Add(FilterOperator.IsGreaterThan, "{0} > {1}");
            _operators.Add(FilterOperator.IsEqualTo, "{0} = {1}");
            _operators.Add(FilterOperator.IsContainedIn, "{0} IN({1})");
        }

        public DataSourceResult GetFromTableValued<T>(Dictionary<string, object> parameters, Dictionary<string, object> conditionals = null) where T : class
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

            if (conditionals != null)
            {
                foreach (var key in conditionals.Keys)
                {
                    _dynamicParameters.Add(key, conditionals[key]);
                }

                whereClause = "WHERE " + string.Join(" AND ", conditionals.Select(x => x.Key + " = @" + x.Key));
            }

            return GetData<T>(string.Format("SELECT * FROM {0}({1}) {2}", attribute.Name, parameterString, whereClause));
        }

        public DataSourceResult GetFromQuery<T>(string query, Dictionary<string, object> parameters) where T : class
        {
            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    _dynamicParameters.Add(key, parameters[key]);
                }
            }

            return GetData<T>(query);
        }

        public DataSourceResult GetFromTable<T>(Dictionary<string, object> parameters) where T : class
        {
            var whereClause = string.Empty;

            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    _dynamicParameters.Add(key, parameters[key]);
                }

                whereClause = "WHERE " + string.Join(" AND ", parameters.Select(x => x.Key + " = @" + x.Key));
            }
            var type = typeof(T);
            var attribute = type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;

            return GetData<T>(string.Format("SELECT * FROM [{0}] {1}", attribute != null ? attribute.Name : type.Name, whereClause));
        }

        public DataSourceResult GetData<T>(string query) where T : class
        {
            var displayLength = _dataSourceRequest.PageSize == 0 ? int.MaxValue : _dataSourceRequest.PageSize;
            var displayStart = (_dataSourceRequest.Page - 1) * displayLength + 1;

            var filterQuery = _dataSourceRequest.Filters != null ? BuildFilter(_dataSourceRequest.Filters.ToList(), "AND") : "1=1";
            var sortQuery = BuildSort<T>();
            var sqlQuery = string.Format(_queryTemplate, displayLength, query, _tableAlias, filterQuery, sortQuery, displayStart);
            var totalCount = GetTotalCount(query, filterQuery);
            int? connectionTimeout = _unitOfWork.Database.GetCommandTimeout();

            var data = _unitOfWork.GetConnection().Query<T>(sqlQuery, _dynamicParameters.ParameterNames.Count() == 0 ? null : _dynamicParameters, buffered: true, commandTimeout: connectionTimeout);
            
            return new DataSourceResult { Data = data, Total = totalCount };
        }

        protected virtual int GetTotalCount(string query, string filterQuery)
        {
            var sqlString = string.Format(_countQueryTemplate, query, _tableAlias, string.IsNullOrEmpty(filterQuery) ? "1=1" : filterQuery);

            return _unitOfWork.GetConnection().ExecuteScalar<int>(sqlString, _dynamicParameters);
        }

        protected virtual string BuildFilter(List<IFilterDescriptor> searchables, string operand)
        {
            var queryBuilder = new List<string>();

            foreach (IFilterDescriptor filterDescriptor in searchables)
            {
                if (filterDescriptor is CompositeFilterDescriptor)
                {
                    var composite = filterDescriptor as CompositeFilterDescriptor;

                    queryBuilder.Add(BuildFilter(composite.FilterDescriptors.ToList(), composite.LogicalOperator.ToString()));
                }
                else
                {
                    var filter = filterDescriptor as FilterDescriptor;

                    if (string.IsNullOrEmpty((filter.Value ?? string.Empty).ToString())) continue;

                    var index = _dynamicParameters.ParameterNames.Count();
                    var paramName = "@" + filter.Member + index;
                    _dynamicParameters.Add(paramName, filter.Value);

                    queryBuilder.Add(string.Format(_operators[filter.Operator], filter.Member, paramName));
                }
            }

            return "(" + (queryBuilder.Count > 0 ? string.Join(string.Format(" {0} ", operand), queryBuilder) : "1 = 1") + ")";
        }

        protected virtual string BuildSort<T>()
        {
            var sortList = new List<string>();
            var sortables = _dataSourceRequest.Sorts;

            if (sortables != null && sortables.Count > 0)
            {
                foreach (var sort in sortables)
                {
                    sortList.Add(string.Format("{0} {1}", sort.Member, sort.SortDirection == ListSortDirection.Ascending ? "ASC" : "DESC"));
                }
            }
            else
            {
                var type = typeof(T);
                var propertyName = type.GetProperties().First().Name;

                sortList.Add(propertyName + " ASC");
            }

            return string.Join(", ", sortList);
        }
    }
}
