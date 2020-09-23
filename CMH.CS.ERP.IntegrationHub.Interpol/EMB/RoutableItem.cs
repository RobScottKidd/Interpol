using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;

using EventClass = CMH.Common.Events.Models.EventClass;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Implementation of IRoutableItem
    /// </summary>
    /// <typeparam name="T">A canonical model type</typeparam>
    public class RoutableItem<T> : IRoutableItem<T>
    {
        /// <inheritdoc/>
        public DataTypes DataType { get; set; }

        /// <inheritdoc/>
        public string EventType { get; set; }

        /// <inheritdoc/>
        public EventClass MessageType { get; set; }

        /// <inheritdoc/>
        public T Model { get; set; }

        /// <inheritdoc/>
        public IEMBRoutingKeyInfo[] RoutingKeys { get; set; }

        /// <inheritdoc/>
        public string Status { get; set; }

        /// <inheritdoc/>
        public string Version { get; set; }
    }
}