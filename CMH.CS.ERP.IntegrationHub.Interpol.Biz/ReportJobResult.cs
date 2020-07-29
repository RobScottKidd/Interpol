namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Job result statuses for Oracle report jobs
    /// </summary>
    public static class ReportJobResult
    {
        /// <summary>
        /// Job succeeded
        /// </summary>
        public static string Success { get; } = "Success";

        /// <summary>
        /// Job is running
        /// </summary>
        public static string Running { get; } = "Running";

        /// <summary>
        /// File contents are invalid
        /// </summary>
        public static string OutputHasError { get; } = "Output has Error";

        /// <summary>
        /// Job has been canceled
        /// </summary>
        public static string Canceled { get; } = "Canceled";
    }
}