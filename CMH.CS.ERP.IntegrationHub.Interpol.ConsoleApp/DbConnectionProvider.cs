using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using NLog;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.ConsoleApp
{
    /// <summary>
    /// A class that uses AWS Secrets Manager to retrieve connection string information for building database connections.
    /// </summary>
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly string _connectionString;
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the DbConnectionProvider with the provided AWS region and secret name.
        /// </summary>
        /// <param name="awsRegion">The AWS region of the current execution context</param>
        /// <param name="awsSecretName">The name of the secret to retrieve</param>
        public DbConnectionProvider(string awsRegion, string awsSecretName)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Trace($"awsRegion={awsRegion}, awsSecretName={awsSecretName}");

            if (string.IsNullOrWhiteSpace(awsRegion)) throw new ArgumentNullException(nameof(awsRegion));
            if (string.IsNullOrWhiteSpace(awsSecretName)) throw new ArgumentNullException(nameof(awsSecretName));

            var region = RegionEndpoint.GetBySystemName(awsRegion);
            if (region is null) throw new ArgumentException($"Not a valid AWS region identifier: {awsRegion}", nameof(awsRegion));

            _connectionString = CreateConnectionString(region, awsSecretName);
        }

        /// <summary>
        /// Connects to the AWS region and retrieves the provided secret, which is then used to construct the final connection string.
        /// </summary>
        /// <param name="region">The AWS region of the current execution context</param>
        /// <param name="secretName">The name of the secret to retrieve</param>
        /// <returns>The connection string to use for database connections</returns>
        private string CreateConnectionString(RegionEndpoint region, string secretName)
        {
            using (var client = new AmazonSecretsManagerClient(region))
            {
                var getSecretValueRequest = new GetSecretValueRequest() { SecretId = secretName };

                GetSecretValueResponse getSecretValueResponse = null;
                try
                {
                    getSecretValueResponse = client.GetSecretValueAsync(getSecretValueRequest).Result;
                }
                catch (AggregateException ae)
                {
                    _logger.Error("One or more exceptions were triggered while attempting to retrieve the secret");
                    ae.InnerExceptions.ToList().ForEach(ex =>
                    {
                        if (ex is DecryptionFailureException) _logger.Error("Secrets Manager can't decrypt the protected secret text using the provided KMS key");
                        else if (ex is InternalServiceErrorException) _logger.Error("An error occurred on the AWS server side");
                        else if (ex is InvalidParameterException) _logger.Error("An invalid value for a parameter was provided");
                        else if (ex is InvalidRequestException) _logger.Error("A parameter value was provided that is not valid for the current state of the resource");
                        else if (ex is ResourceNotFoundException) _logger.Error("The requested resource was not found");
                        else _logger.Error(ex.Message);
                    });
                    throw;
                }

                var secretString = getSecretValueResponse.SecretString;

                var connectionInfo = JsonConvert.DeserializeObject<ConnectionInfo>(secretString);
                var connectionStringBuilder = new DbConnectionStringBuilder
                {
                    { "Server", $"{connectionInfo.Host}\\CORP1" },
                    { "Database", connectionInfo.DBName },
                    { "UID", connectionInfo.Username },
                    { "PWD", connectionInfo.Password }
                };

                var connectionString = connectionStringBuilder.ToString();
                return connectionString;
            }
        }

        public IDbConnection GetConnection() => new SqlConnection(_connectionString);
    }
}