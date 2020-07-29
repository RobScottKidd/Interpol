using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.ServiceHosting
{
    /// <summary>
    /// Represents a service that can run for an extended time performing processing tasks and can be stopped at any point.
    /// Isolates the business logic of the service from the implementation of the host (Windows Service or a .NET Core console application).
    /// </summary>
    public interface IServiceRunner
    {
        /// <summary>
        /// Instructs the service to start processing.
        /// </summary>
        void Run();

        /// <summary>
        /// Instructs the service to stop processing in a graceful manner.
        /// </summary>
        /// <param name="sender">The object that triggered the event (may be null in a static context)</param>
        /// <param name="e">The event arguments</param>
        void Stop(object sender, EventArgs e);
    }
}