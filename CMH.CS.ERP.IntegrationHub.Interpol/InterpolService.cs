using CMH.Common.Events.Interfaces;
using CMH.Common.Events.Models;
using CMH.Common.RabbitMQClient;
using CMH.Common.RabbitMQClient.Exceptions;
using CMH.Common.RabbitMQClient.Interfaces;
using CMH.Common.RabbitMQClient.Models;
using CMH.CS.ERP.IntegrationHub.Interpol.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Biz.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers;
using CMH.CS.ERP.IntegrationHub.Interpol.ServiceHosting;
using CMH.CSS.ERP.GlobalUtilities;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// The INTERPOL service business logic
    /// </summary>
    public class InterpolService : IServiceRunner
    {
        #region "static init"
        private static readonly Guid _instanceId;
        private static readonly ILogger<InterpolService> _logger;
        private static readonly ServiceCollection _services;
        private static readonly string _instanceKey;
        private static readonly System.Configuration.Configuration _assemblyConfig;
        private static readonly string _encryptionKey;
        private static readonly string _salt;
        private static readonly string _systemMessageExchange;

        /// <summary>
        /// Static initializer that sets up class-level logging and builds and configures services
        /// available through dependency injection
        /// </summary>
        static InterpolService()
        {
            Directory.SetCurrentDirectory(Environment.CurrentDirectory ?? ".");
            
            //The instance key is the second command line arg
            _instanceKey = Environment.GetCommandLineArgs()[1];

            //TODO: replace with proper technique for retrieving instance ID
            _instanceId = Guid.NewGuid();

            _assemblyConfig = typeof(InterpolService).GetAssemblyConfiguration();

            _encryptionKey = GetAssemblyConfigValue("RabbitMQClient:encryptionkey");
            _salt = GetAssemblyConfigValue("RabbitMQClient:salt");
            _systemMessageExchange = GetAssemblyConfigValue("RabbitMQClient:eventexchange");

            _services = new ServiceCollection();
            _services.AddLogging(_builder => _builder.AddNLog().SetMinimumLevel(LogLevel.Trace));

            var serviceProvider = _services.BuildServiceProvider();
            _logger = serviceProvider.GetService<ILogger<InterpolService>>();

            ConfigureServices(_services);
        }
        #endregion

        private static string GetAssemblyConfigValue(string key)
        {
            var value = _assemblyConfig?.AppSettings?.Settings?[key]?.Value;
            if (value is null)
            {
                _logger.LogWarning($"AppSettings key '{key}' not found in {_assemblyConfig?.FilePath}");
            }
            return value;
        }

        private readonly IScheduleProcessor _scheduler;
        private readonly CancellationTokenSource _tokenSource;
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly IEventConsumer _consumer;
        private readonly IQueueCreator _queueCreator;
        private readonly ISystemMessageFactory _systemMessageFactory;
        private readonly ITestController _testController;

        /// <summary>
        /// Ctor that uses the provided token source as a cancellation notifier,
        /// and creates a schedule processor using the DI system.
        /// </summary>
        /// <param name="connectionProvider">The provider for database connections</param>
        /// <param name="tokenSource">The provider for the cancellation token</param>
        public InterpolService(IDbConnectionProvider connectionProvider, CancellationTokenSource tokenSource)
        {
            _connectionProvider = connectionProvider;
            _tokenSource = tokenSource;
            _services.AddTransient((sp) => _connectionProvider.GetConnection());

            var serviceProvider = _services.BuildServiceProvider();

            _consumer = serviceProvider.GetService<IEventConsumer>();
            _scheduler = serviceProvider.GetService<IScheduleProcessor>();
            _queueCreator = serviceProvider.GetService<IQueueCreator>();
            _systemMessageFactory = serviceProvider.GetService<ISystemMessageFactory>();
            _testController = serviceProvider.GetService<ITestController>();
        }

        /// <summary>
        /// Configures the various services used by INTERPOL for use with DependencyInjection.
        /// </summary>
        /// <param name="services">The services registered in the DI system</param>
        private static void ConfigureServices(IServiceCollection services)
        {
            var config = typeof(InterpolService).GetAssemblyConfiguration();
            string GetAssemblyConfigValue(string key) {
                var value = config?.AppSettings?.Settings?[key]?.Value;
                if (value is null)
                {
                    _logger.LogWarning($"AppSettings key '{key}' not found in {config?.FilePath}");
                }
                return value;
            }

            services.AddConfiguration<CommunicationConfiguration>()
                .AddConfiguration<TestConfiguration>(c => c.StartInTestMode = bool.Parse(GetAssemblyConfigValue("StartInTestMode")))
                .AddSingleton<ConnectionConfiguration, ConnectionConfiguration>(serviceProvider => new ConnectionConfiguration()
                {
                    GenericSoapServiceEndpoint = GetAssemblyConfigValue("OracleERP:GenericSoapServiceEndpoint"),
                    IntegrationServiceEndpoint = GetAssemblyConfigValue("OracleERP:IntegrationServiceEndpoint"),
                    Password = GetAssemblyConfigValue("OracleERP:Password"),
                    ScheduleServiceEndpoint = GetAssemblyConfigValue("OracleERP:ScheduleServiceEndpoint"),
                    Username = GetAssemblyConfigValue("OracleERP:Username")
                })
                .AddSingleton<IRabbitMQConfiguration, RabbitMQConfiguration>(serviceProvider => new RabbitMQConfiguration()
                {
                    HostName = GetAssemblyConfigValue("RabbitMQClient:hostname"),
                    VirtualHost = GetAssemblyConfigValue("RabbitMQClient:vhost"),
                    UserName = GetAssemblyConfigValue("RabbitMQClient:username"),
                    Password = GetAssemblyConfigValue("RabbitMQClient:password"),
                    Port = int.Parse(GetAssemblyConfigValue("RabbitMQClient:port")),
                    SslEnabled = bool.Parse(GetAssemblyConfigValue("RabbitMQClient:sslenabled")),
                    NetworkRecoveryIntervalMilliseconds = int.Parse(GetAssemblyConfigValue("RabbitMQClient:recoverycheckinterval")),
                    ContinuationTimeoutMilliseconds = int.Parse(GetAssemblyConfigValue("RabbitMQClient:connectiontimeout")),
                    PrefetchSize = uint.Parse(GetAssemblyConfigValue("RabbitMQClient:prefetchsize")),
                    PrefetchCount = ushort.Parse(GetAssemblyConfigValue("RabbitMQClient:prefetchcount")),
                    MaxNumberOfRetries = int.Parse(GetAssemblyConfigValue("RabbitMQClient:maxnumberofretries")),
                    RecoveryWaitTimeMilliseconds = int.Parse(GetAssemblyConfigValue("RabbitMQClient:recoverywaittime")),
                    JsonSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }
                })
                .AddSingleton<IInterpolConfigurationProvider, InterpolConfigurationProvider>(
                    serviceProvider => new InterpolConfigurationProvider(
                        serviceProvider.GetService<ISqlProvider>(),
                        _instanceKey
                    )
                )
                .AddSingleton<IInterpolConfiguration, InterpolConfiguration>()
                .AddTransient<IRabbitMQConnection, RabbitMQConnection>()
                .AddTransient<IEventConsumer, RabbitMQConsumer>()
                .AddTransient<IEventProducer, RabbitMQProducer>()
                .AddTransient<IQueueCreator, QueueCreator>()
                .AddSingleton<ISystemMessageFactory, SystemMessageFactory>()
                .AddSingleton<ITestController, TestController>()
                .AddTransient<IFileExporter, FileExporter>()
                .AddSingleton<IScheduleProcessor, ScheduleProcessor>()
                .AddTransient<IMessageBusConnector, MessageBusConnector>(
                    serviceProvider => new MessageBusConnector(
                        serviceProvider.GetService<IEventProducer>(),
                        serviceProvider.GetService<ILogger<MessageBusConnector>>(),
                        bool.Parse(GetAssemblyConfigValue("RabbitMQClient:encryption")),
                        GetAssemblyConfigValue("RabbitMQClient:encryptionkey"),
                        GetAssemblyConfigValue("RabbitMQClient:salt")
                    )
                )
                .AddTransient<IDateTimeProvider, DateTimeProvider>()
                .AddTransient<IIDProvider, IDProvider>()
                .AddTransient<ITaskLogRepository<TaskLog>, TaskLogRepository>()
                .AddTransient<IBUTrackerRepository, BUTrackerRepository>()
                .AddTransient<ISqlProvider, SqlProvider>()
                .AddSingleton<IDataCache, DataCache>()
                .AddTransient<ISchedulerTaskFactory, SchedulerTaskFactory>()
                .AddTransient<IMessageProcessor, OracleBackflowMessageProcessor>()
                .AddTransient<IAggregateMessageProcessor, OracleBackflowAggregateMessageProcessor>()
                .AddTransient<IReportXmlExtractor, ReportXmlExtractor>()
                .AddSingleton<IScheduleReportNameProvider, ScheduleReportNameProvider>()
                .AddSingleton<IOracleServiceFactory, OracleServiceFactory>()
                .AddSingleton<ITestController, TestController>()
                .AddTransient<IInterpolOracleGateway, InterpolOracleGateway>()
                .AddTransient<IReportTaskDetailRepository, ReportTaskDetailRepository>()
                .AddTransient<IBUDataTypeLockRepository, BUDataTypeLockRepository>()
                .AddTransient<ISchedulerTask, SchedulerTask<AccountingHubStatusMessage>>()
                .AddTransient<ISchedulerTask, SchedulerTask<APInvoice>>()
                .AddTransient<ISchedulerTask, SchedulerTask<APInvoiceStatusMessage>>()
                .AddTransient<ISchedulerTask, SchedulerTask<APPayment>>()
                .AddTransient<ISchedulerTask, SchedulerTask<APPaymentRequest>>()
                .AddTransient<ISchedulerTask, SchedulerTask<APPaymentRequestStatusMessage>>()
                .AddTransient<ISchedulerTask, SchedulerTask<CashManagementStatusMessage>>()
                .AddTransient<ISchedulerTask, SchedulerTask<GLJournal>>()
                .AddTransient<ISchedulerTask, SchedulerTask<GLJournalStatusMessage>>()
                .AddTransient<ISchedulerTask, SchedulerTask<Supplier>>()
                .AddSingleton<IInstanceKeyProvider, InstanceKeyProvider>()
                .AddSingleton<IEnvironmentProvider, EnvironmentProvider>(serviceProvider => new EnvironmentProvider(GetAssemblyConfigValue("DeploymentEnvironment")))
                .AddSingleton<IJsonSettingsProvider, JsonSettingsProvider>()
                .AddSingleton<IMultistepOperationTimer, MultiStepOperationTimer>()
                .AddSingleton<IOracleBackflowProcessor<AccountingHubStatusMessage>>(
                    serviceProvider => new OracleBackflowProcessor<AccountingHubStatusMessage>(
                        serviceProvider.GetService<ILogger<OracleBackflowProcessor<AccountingHubStatusMessage>>>(),
                        "Header"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<AccountingHubStatusMessage>, OracleBackflowAccountingHubPostProcessor>()
                .AddSingleton<IOracleBackflowProcessor<APInvoice>>(
                    serviceProvider => new OracleBackflowProcessor<APInvoice>(
                        serviceProvider.GetService<ILogger<OracleBackflowProcessor<APInvoice>>>(),
                        "InvoiceHeaders"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<APInvoice>, OracleBackflowAPInvoicePostProcessor>()
                .AddSingleton<IOracleBackflowProcessor<APInvoiceStatusMessage>>(
                    serviceProvider => new OracleBackflowProcessor<APInvoiceStatusMessage>(
                        serviceProvider.GetService<ILogger<OracleBackflowProcessor<APInvoiceStatusMessage>>>(),
                        "REJECTIONS"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<APInvoiceStatusMessage>, OracleBackflowAPInvoiceStatusPostProcessor>()
                .AddSingleton<IOracleBackflowProcessor<APPayment>>(
                    serviceProvider => new OracleBackflowProcessor<APPayment>(
                        serviceProvider.GetService<ILogger<OracleBackflowProcessor<APPayment>>>(),
                        "Payments"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<APPayment>, OracleBackflowAPPaymentPostProcessor>()
                .AddSingleton<IOracleBackflowProcessor<APPaymentRequest>>(
                    serviceProvider => new OracleBackflowAPPaymentRequestProcessor(
                        serviceProvider.GetService<IOracleBackflowProcessor<APInvoice>>(),
                        serviceProvider.GetService<ILogger<OracleBackflowAPPaymentRequestProcessor>>(),
                        "InvoiceHeaders"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<APPaymentRequest>, OracleBackflowAPPaymentRequestPostProcessor>()
                .AddSingleton<IOracleBackflowProcessor<APPaymentRequestStatusMessage>>(
                    serviceProvider => new OracleBackflowProcessor<APPaymentRequestStatusMessage>(
                        serviceProvider.GetService<ILogger<OracleBackflowProcessor<APPaymentRequestStatusMessage>>>(),
                        "Rejections"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<APPaymentRequestStatusMessage>, OracleBackflowAPPaymentRequestStatusPostProcessor>()
                .AddSingleton<IOracleBackflowProcessor<CashManagementStatusMessage>>(
                    serviceProvider => new OracleBackflowProcessor<CashManagementStatusMessage>(
                        serviceProvider.GetService<ILogger<OracleBackflowProcessor<CashManagementStatusMessage>>>(),
                        "G_1"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<CashManagementStatusMessage>, OracleBackflowCashManagementStatusPostProcessor>()
                .AddSingleton<IOracleBackflowProcessor<GLJournal>>(
                    serviceProvider => new OracleBackflowProcessor<GLJournal>(
                        serviceProvider.GetService<ILogger<OracleBackflowProcessor<GLJournal>>>(),
                        "G_1"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<GLJournal>, OracleBackflowGLJournalPostProcessor>()
                .AddSingleton<IOracleBackflowProcessor<GLJournalStatusMessage>>(
                    serviceProvider => new OracleBackflowProcessor<GLJournalStatusMessage>(
                        serviceProvider.GetService<ILogger<OracleBackflowProcessor<GLJournalStatusMessage>>>(),
                        "G_1"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<GLJournalStatusMessage>, OracleBackflowGLJournalStatusMessagePostProcessor>()
                .AddSingleton<IOracleBackflowProcessor<Supplier>>(
                    serviceProvider => new OracleBackflowProcessor<Supplier>(
                        serviceProvider.GetService<ILogger<OracleBackflowProcessor<Supplier>>>(),
                        "Supplier"
                    )
                )
                .AddSingleton<IOracleBackflowPostProcessor<Supplier>, OracleBackflowSupplierPostProcessor>();
        }

        /// <summary>
        /// Event callback executed when a system message is received from the EMB
        /// </summary>
        /// <param name="message">The message received from the EMB</param>
        /// <param name="args">The delivery arguments provided by the EMB</param>
        /// <returns>The event result of the operation</returns>
        public EventResult OnSystemMessageReceived(IEventMessage<object> message, BasicDeliverEventArgs args)
        {
            try
            {
                var systemMessage = _systemMessageFactory.ParseMessage(message, _testController);
                systemMessage.ProcessMessage();
                return EventResult.Success;
            }
            catch (ArgumentException ae)
            {
                _logger.LogError(ae, "Malformed message");
                return EventResult.MessageFailure;
            }
            catch (NotSupportedException nse)
            {
                _logger.LogWarning($"Ignoring unsupported system event: {nse.Message}");
                return EventResult.MessageFailure;
            }
        }

        /// <summary>
        /// Runs the scheduler for the INTERPOL service
        /// </summary>
        public void Run()
        {
            try
            {
                var systemQueueName = _queueCreator.CreateQueue(
                    queueName: $"corporate.integrationhub.interpol.system.{_instanceId}",
                    isDurable: true,
                    isExclusive: false,
                    autoDelete: true
                );
                _queueCreator.BindToExchange(systemQueueName, _systemMessageExchange, string.Empty);
                _consumer.Register<object>("testing", OnSystemMessageReceived, encryptionKey: _encryptionKey, salt: _salt);
                _consumer.AttachToQueue(systemQueueName, exclusive: false);
            }
            catch (RabbitMQConnectionException rmqcex)
            {
                _logger.LogError(rmqcex, $"Unable to create and connect to system notification queue, InstanceID: {_instanceId}");
                throw rmqcex;
            }
            try
            {
                _logger.LogInformation($"{nameof(InterpolService)} starting to run");

                _scheduler.Start(_tokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // this occurs when the running task is cancelled from the ServiceBase.Stop() method
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error running {nameof(InterpolService)}");
                throw e;
            }
        }

        /// <summary>
        /// Instructs INTERPOL to stop processing.
        /// </summary>
        /// <param name="sender">The object that is requesting the stop</param>
        /// <param name="e">The event args</param>
        public void Stop(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
            _scheduler.Stop();
            _logger.LogInformation($"Stopped {nameof(InterpolService)}");
        }
    }
}