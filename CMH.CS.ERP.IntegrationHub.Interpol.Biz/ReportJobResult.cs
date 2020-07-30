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
        /// Job is ready
        /// </summary>
        public static string Ready { get; } = "Ready";

        /// <summary>
        /// Job is running
        /// </summary>
        public static string Running { get; } = "Running";

        /// <summary>
        /// Job is in wait
        /// </summary>
        public static string Wait { get; } = "Wait";

        /// <summary>
        /// Job is paused
        /// </summary>
        public static string Paused { get; } = "Paused";

        /// <summary>
        /// File contents are invalid
        /// </summary>
        public static string OutputHasError { get; } = "Output has Error";

        /// <summary>
        /// Error during delivery
        /// </summary>
        public static string DeliveryHasError { get; } = "Delivery has Error";

        /// <summary>
        /// Job has been canceled
        /// </summary>
        public static string Canceled { get; } = "Canceled";
    }
}