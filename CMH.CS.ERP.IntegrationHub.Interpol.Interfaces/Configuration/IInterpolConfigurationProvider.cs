using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration
{
    public interface IInterpolConfigurationProvider
    {
        IInterpolConfigurationDbModel GetConfiguration();

        IExclusion[] GetExclusions();

        Dictionary<string, IScheduleConfiguration[]> GetSchedules();
    }
}