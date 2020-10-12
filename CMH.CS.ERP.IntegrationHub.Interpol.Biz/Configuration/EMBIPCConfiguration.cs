using System;
using System.Collections.Generic;
using System.Text;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.Configuration
{
    /// <summary>
    /// Special class for handling IPC exchange and queue configuration
    /// </summary>
    public class EMBIPCConfiguration
    {
        /// <summary>
        /// The endpoint URL to use for schedule services
        /// </summary>
        public string IPCExchange { get; set; }

        /// <summary>
        /// The service account username
        /// </summary>
        public string IPCRoutingKey { get; set; }
    }
}
