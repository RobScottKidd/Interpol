using System;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Data
{
    public interface IBUTrackerRepository
    {
        Dictionary<Guid, IBusinessUnit> GetBusinessUnits(List<Guid> allGuids);
    }
}