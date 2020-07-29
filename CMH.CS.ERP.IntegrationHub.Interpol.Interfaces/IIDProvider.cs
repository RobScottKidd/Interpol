using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    public interface IIDProvider
    {
        /// <summary>
        /// returns a GUID of the provided string or a random guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Guid GetGuid(string guid = "");
    }
}