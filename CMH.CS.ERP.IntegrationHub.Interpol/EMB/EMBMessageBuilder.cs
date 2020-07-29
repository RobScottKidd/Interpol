using CMH.Common.Events.Interfaces;
using CMH.Common.Events.Models;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    public static class EMBMessageBuilder
    {
        /// <summary>
        /// Creates an EMBEvent message from the given parameters
        /// </summary>
        /// <typeparam name="T">Type of the payload</typeparam>
        /// <param name="item">Payload data</param>
        /// <param name="eventClass"></param>
        /// <param name="source"></param>
        /// <param name="eventType"></param>
        /// <param name="eventSubType"></param>
        /// <param name="processId"></param>
        /// <param name="idProvider"></param>
        /// <param name="dateTimeProvider"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static IEMBEvent<T> BuildMessage<T>(T item, EventClass eventClass, string source, string eventType, string eventSubType, string processId,
            IIDProvider idProvider, IDateTimeProvider dateTimeProvider, string version)
        {
            return new EMBEvent<T>()
            {
                EventID = idProvider.GetGuid(),
                EventDate = dateTimeProvider.CurrentTime,
                EventVersion = version,
                EventClass = eventClass,
                EventType = eventType,
                EventSubType = eventSubType,
                Source = string.IsNullOrEmpty(source) ? "unknown" : source.ToUpper(),
                ProcessInitiator = string.Empty,
                ProcessId = string.Empty,
                Reporter = string.IsNullOrEmpty(source) ? "unknown" : source.ToUpper(),
                Payload = item
            };
        }
    }
}