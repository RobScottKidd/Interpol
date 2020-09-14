using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;

using EventClass = CMH.Common.Events.Models.EventClass;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    /// <summary>
    /// Specifies a type that can be used to provide routing information for a canonical model message on the EMB
    /// </summary>
    /// <typeparam name="T">A canonical model type</typeparam>
    public interface IRoutableItem<T>
    {
        /// <summary>
        /// The datatype of the message
        /// </summary>
        public DataTypes DataType { get; set; }

        /// <summary>
        /// The string representation of the event type
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// The message type (usually Detail or Notice)
        /// </summary>
        public EventClass MessageType { get; set; }

        /// <summary>
        /// The canonical model
        /// </summary>
        public T Model { get; set; }

        /// <summary>
        /// The routing keys to use for the message when routing on the EMB
        /// </summary>
        public IEMBRoutingKeyInfo[] RoutingKeys { get; set; }

        /// <summary>
        /// The status to use when routing on the EMB
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The version of the model (this can be an optional field for certain datatypes)
        /// </summary>
        public string Version { get; set; }
    }
}