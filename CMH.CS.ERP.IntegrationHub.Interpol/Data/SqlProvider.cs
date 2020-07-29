using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Data
{
    /// <summary>
    /// Implementation of ISqlProvider
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SqlProvider : ISqlProvider
    {
        private readonly IDbConnection _connection;

        public SqlProvider(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Executes stored procedure
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure to execute</param>
        /// <param name="parameters">Parameters to be send to the stored procedure</param>
        /// <param name="treatDataTableAsTvp">Specifies if DataTable fields should be converted to TVP</param>
        /// <returns></returns>
        public int ExecuteStoredProcedure(string storedProcedureName, object parameters, bool treatDataTableAsTvp = false)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            var dynamicParams = new DynamicParameters();

            foreach (var property in parameters.GetType().GetProperties().Where(_prop => _prop.CanRead))
            {
                if (treatDataTableAsTvp && property.PropertyType == typeof(DataTable))
                {
                    var dataTable = (property.GetValue(parameters) as DataTable);
                    var tvpParam = dataTable.AsTableValuedParameter(dataTable.TableName);
                    dynamicParams.Add(property.Name, tvpParam);
                }
                else
                {
                    dynamicParams.Add(property.Name, property.GetValue(parameters));
                }
            }

            int result;
            try
            {
                result = _connection.Execute(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                _connection.Close();
            }

            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<T> QueryStoredProcedure<T>(string storedProcedureName, object parameters)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            IEnumerable<T> result;
            try
            {
                result = _connection.Query<T>(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                _connection.Close();
            }

            return result;
        }
    }
}