using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Represents a type that can be used to provide the name of a scheduled report to send to Oracle.
    /// </summary>
    public interface IScheduleReportNameProvider
    {
        /// <summary>
        /// Generates the report job name based on the provided report parameter configuration.
        /// </summary>
        /// <param name="reportParameters">The report parameter setup</param>
        /// <param name="businessUnit">The business unit</param>
        /// <returns>The job name to use</returns>
        string GetUserJobName(IReportParameter reportParameters, string businessUnit);

        /// <summary>
        /// Generates the WCC filename based on the provided report parameter configuration.
        /// </summary>
        /// <param name="reportParameters">The report parameter setup</param>
        /// <param name="businessUnit">The business unit</param>
        /// <returns>The filename to use</returns>
        string GetWCCFileName(IReportParameter reportParameters, string businessUnit);
    }
}