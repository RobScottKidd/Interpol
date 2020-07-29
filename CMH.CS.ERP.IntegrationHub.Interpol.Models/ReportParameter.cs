using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    /// <summary>
    /// Model of the ReportParameter in the IntegrationHub database
    /// </summary>
    public class ReportParameter : IReportParameter
    {
        /// <inheritdoc/>
        public Guid ReportParameterID { get; set; }

        /// <inheritdoc/>
        public string AttributeFormat { get; set; }

        /// <inheritdoc/>
        public string AttributeTemplate { get; set; }

        /// <inheritdoc/>
        public string DataType { get; set; }

        /// <inheritdoc/>
        public string AttributeLocale { get; set; }

        /// <inheritdoc/>
        public string WCCTitle { get; set; }

        /// <inheritdoc/>
        public string WCCFileName { get; set; }

        /// <inheritdoc/>
        public string WCCSecurityGroup { get; set; }

        /// <inheritdoc/>
        public string WCCServerName { get; set; }

        /// <inheritdoc/>
        public string ReportAbsolutePath { get; set; }

        /// <inheritdoc/>
        public string UserJobName { get; set; }

        /// <inheritdoc/>
        public string ReportParameters { get; set; }
    }
}