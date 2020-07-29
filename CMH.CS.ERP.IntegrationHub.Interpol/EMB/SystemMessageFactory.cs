using CMH.Common.Events.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Implementation of ISystemMessageFactory.
    /// </summary>
    public class SystemMessageFactory : ISystemMessageFactory
    {
        private const string TESTING_EVENT_TYPE = "testing";

        /// <summary>
        /// Helper method that creates a test-oriented system message.
        /// </summary>
        /// <typeparam name="T">The type of the message payload</typeparam>
        /// <param name="message">The system event message</param>
        /// <param name="action">The event subtype or action to perform</param>
        /// <param name="testController">The test controller</param>
        /// <returns></returns>
        private ISystemMessage CreateTestingMessage<T>(IEventMessage<T> message, string action, ITestController testController)
        {
            if (action.Equals("Oracle start", StringComparison.CurrentCultureIgnoreCase))
            {
                return new TestingStartMessage(testController);
            }
            else if (action.Equals("Oracle end", StringComparison.CurrentCultureIgnoreCase))
            {
                return new TestingEndMessage(testController);
            }

            throw new NotSupportedException($"Unsupported action '{action}' for system event '{TESTING_EVENT_TYPE}'");
        }

        /// <inheritdoc/>
        public ISystemMessage ParseMessage<T>(IEventMessage<T> message, ITestController testController)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            var eventType = message.EventType?.ToLower();
            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new ArgumentException("Missing required field 'EventType'");
            }

            var action = message.EventSubType;
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Missing required field 'EventSubType'");
            }

            if (eventType.Equals(TESTING_EVENT_TYPE, StringComparison.CurrentCultureIgnoreCase))
            {
                return CreateTestingMessage(message, action, testController);
            }

            throw new NotSupportedException($"Unsupported system event '{eventType}'");
        }
    }
}