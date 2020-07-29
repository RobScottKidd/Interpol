using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Model of the BusinessUnit is IntegrationHub database
    /// </summary>
    public interface IReportParameter
    {
        /// <summary>
        /// Unique identifier of the Report Parameter 
        /// </summary>
        Guid ReportParameterID { get; set; }

        /// <summary>
        /// Report Parameter Attribute Format
        /// </summary>
        string AttributeFormat { get; set; }

        /// <summary>
        /// Report Parameter Attribute Template
        /// </summary>
        string AttributeTemplate { get; set; }

        /// <summary>
        /// Report Parameter Data Type 
        /// </summary>
        string DataType { get; set; }

        /// <summary>
        /// Report Parameter Attribute Locale
        /// </summary>
        string AttributeLocale { get; set; }

        /// <summary>
        /// Report Parameter WCC Title
        /// </summary>
        string WCCTitle { get; set; }

        /// <summary>
        /// Report Parameter WCC FileName
        /// </summary>
        string WCCFileName { get; set; }

        /// <summary>
        /// Report Parameter WCC Security Group
        /// </summary>
        string WCCSecurityGroup { get; set; }

        /// <summary>
        /// Report Parameter WCC Server Name
        /// </summary>
        string WCCServerName { get; set; }

        /// <summary>
        /// Report Parameter Report Absolute Path
        /// </summary>
        string ReportAbsolutePath { get; set; }

        /// <summary>
        /// Report Parameter User Job Name
        /// </summary>
        string UserJobName { get; set; }

        /// <summary>
        /// Unique Report Parameters
        /// </summary>
        string ReportParameters { get; set; }
    }
}