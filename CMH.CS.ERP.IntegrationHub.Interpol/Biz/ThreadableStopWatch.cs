using System;
using System.Threading;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Handles creating and tracking stop watches in multi-threaded scenarios. 
    /// </summary>
    /// <remarks>MUST BE SINGLTON SCOPE</remarks>
    public class ThreadableStopWatch
    {
        public Guid Id { get; set; }

        public Guid ThreadID {get; set; }

        public string DataType { get; set; }

        public string BusinessUnit { get; set; }

        public string TaskName { get; set; }

        public Timer Timer { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public TimeSpan CriticalAlertThreshold { get; set; }
    }
}