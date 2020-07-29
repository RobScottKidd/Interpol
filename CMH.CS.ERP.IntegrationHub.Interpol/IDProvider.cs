using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    public class IDProvider : IIDProvider
    {
        public Guid GetGuid(string guid = "") => string.IsNullOrEmpty(guid) ? Guid.NewGuid() : new Guid(guid);
    }
}