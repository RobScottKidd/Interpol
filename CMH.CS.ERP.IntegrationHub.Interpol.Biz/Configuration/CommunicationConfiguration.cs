using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.Configuration
{
    /// <summary>
    /// Defines the communication configuration for retry and delay information for Oracle
    /// </summary>
    public class CommunicationConfiguration
    {
        /// Specifies how long the write operation has to complete before timing out
        /// </summary>
        public TimeSpan SendTimeout { get; set; }

        /// <summary>
        /// Specifies how long the application has to receive a message before timing out
        /// </summary>
        public TimeSpan ReceiveTimeout { get; set; }

        /// <summary>
        /// Specifies the number of times to try to cancel a scheduled job
        /// </summary>
        public int CancelScheduleJobRetryCount { get; set; }

        /// <summary>
        /// The point at which this step will set a critical alert
        /// </summary>
        public string CancelReportJobCriticalAlertPoint { get; set; }

        /// <summary>
        /// Specifies the number of times to try to create the Oracle report
        /// </summary>
        public int CreateReportRequestRetryCount { get; set; }

        /// <summary>
        /// Specifies how many milliseconds to wait between each attempt to send a report request to Oracle
        /// </summary>
        public int CreateReportRequestDelay { get; set; }

        /// <summary>
        /// The point at which this step will set a critical alert
        /// </summary>
        public string CreateReportCriticalAlertPoint { get; set; }

        /// <summary>
        /// Specifies the number of times to try to get the created Oracle report job instance
        /// </summary>
        public int ReportJobInstanceRequestRetryCount { get; set; }

        /// <summary>
        /// Specifies how many milliseconds to wait before querying Oracle
        /// on the status of the newly created report job
        /// </summary>
        public int ReportJobInstanceRequestDelay { get; set; }

        /// <summary>
        /// The point at which this step will set a critical alert
        /// </summary>
        public string ReportJobInstanceCriticalAlertPoint { get; set; }

        /// <summary>
        /// Specifies the number of times to get information for a report job from Oracle
        /// </summary>
        public int ReportJobInfoRequestRetryCount { get; set; }

        /// <summary>
        /// Specifies the number of milliseconds to wait between each attempt to get the Oracle report job status
        /// </summary>
        public int ReportJobInfoRequestDelay { get; set; }

        /// <summary>
        /// The point at which this step will set a critical alert
        /// </summary>
        public string ReportJobInfoCriticalAlertPoint { get; set; }

        /// <summary>
        /// Specifies the expected result message of a successful job
        /// </summary>
        public string ReportJobSuccessMessage { get; set; } = "Success";

        /// <summary>
        /// Specifies the name of the encoding used when parsing report files
        /// </summary>
        public string ReportEncodingName { get; set; } = "utf-8";

        /// <summary>
        /// Specifies the number of times to attempt to retrieve the generated report document ID
        /// </summary>
        public int ReportDocumentQueryRequestRetryCount { get; set; }

        /// <summary>
        /// The point at which this step will set a critical alert
        /// </summary>
        public string ReportDocumentQueryCriticalAlertPoint { get; set; }

        /// <summary>
        /// Specifies the number of milliseconds to wait between each attempt to retrieve generated report document ID
        /// </summary>
        public int ReportDocumentQueryRequestDelay { get; set; }

        /// <summary>
        /// Specifies the number of attempts to retrieve a generated report document
        /// </summary>
        public int ReportDocumentRequestRetryCount { get; set; }

        /// <summary>
        /// Specifies the number of milliseconds to wait between each attempt to retrieve a generated report document
        /// </summary>
        public int ReportDocumentRequestDelay { get; set; }

        /// <summary>
        /// The point at which this step will set a critical alert
        /// </summary>
        public string ReportDocumentRequestCriticalAlertPoint { get; set; }

        /// <summary>
        /// Specifies the number of attempts to load and import data to Oracle
        /// </summary>
        public int LoadDataToOracleRetryCount { get; set; }

        /// <summary>
        /// Specifies the number of milliseconds to wait between each attempt to load and import data to Oracle
        /// </summary>
        public int LoadDataToOracleRetryDelay { get; set; }

        /// <summary>
        /// Specifies the number of attempts to import data to Oracle
        /// </summary>
        public int ImportDataJobRequestRetryCount { get; set; }

        /// <summary>
        /// Specifies the number of milliseconds to wait between each attempt to import data to Oracle
        /// </summary>
        public int ImportDataJobRequestRetryDelay { get; set; }

        /// <summary>
        /// Unique ledger Id for AP Invoices
        /// </summary>
        public long APInvoiceLedgerID { get; set; }

        /// <summary>
        /// Unique ledger Id for AP Payment Requests
        /// </summary>
        public long APPaymentRequestLedgerID { get; set; }

        /// <summary>
        /// Specifies the phrases/substrings of a scheduler service timeout
        /// contained in the status details
        /// </summary>
        public string[] SchedulerServiceTimeoutIndicators { get; set; }

        /// <summary>
        /// Specifies the terminal states of a scheduler service job
        /// </summary>
        public string[] SchedulerServiceTerminalStatuses { get; set; }

        /// <summary>
        /// Specifies whether or not to log the communication times with Oracle
        /// </summary>
        public bool LogOracleCallTimes { get; set; }
    }
}