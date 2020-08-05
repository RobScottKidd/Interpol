using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Configuration
{
    public class InterpolScheduleDbModel : InterpolDbModelWithBusinessUnits
    {
        public Guid InstanceKey { get; set; }

        public int PollingIntervalMilliseconds { get; set; }

        public string DaysOfWeek{ get; set; }

        public string DataTypes { get; set; }

        public int MaximumReportInterval { get; set; }
    }
}