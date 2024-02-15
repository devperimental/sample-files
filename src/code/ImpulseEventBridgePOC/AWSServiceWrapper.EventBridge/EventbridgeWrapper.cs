using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using AWSServiceWrapper.Shared.Behaviours;
using AWSServiceWrapper.Shared.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWSServiceWrapper.EventBridge
{
    public class EventbridgeWrapper : IEventbridgeWrapper
    {
        private readonly IAmazonEventBridge _amazonEventBridge;
        private readonly ILogger<EventbridgeWrapper> _logger;

        /// <summary>
        /// Constructor for the EventBridge wrapper.
        /// </summary>
        /// <param name="amazonEventBridge">The injected EventBridge client.</param>
        /// <param name="logger">The injected logger for the wrapper.</param>
        public EventbridgeWrapper(IAmazonEventBridge amazonEventBridge, ILogger<EventbridgeWrapper> logger)

        {
            _amazonEventBridge = amazonEventBridge;
            _logger = logger;
        }
        /// <summary>
        /// Add an event to the event bus.
        /// </summary>
        /// <param name="eventBusEntry">The eventBusEntry to use in the PutEventsRequestEntry call.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> PutCustomEvent(EventBusEntry eventBusEntry)
        {
            bool success;
            try
            {
                var response = await _amazonEventBridge.PutEventsAsync(
                new PutEventsRequest()
                {
                    Entries = new List<PutEventsRequestEntry>()
                    {
                        new PutEventsRequestEntry()
                        {
                            Source = eventBusEntry.Source,
                            Detail = eventBusEntry.Detail,
                            EventBusName = eventBusEntry.EventBusName,
                            DetailType = eventBusEntry.DetailType
                        }
                    }
                });

                success = response.FailedEntryCount == 0;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw ex;
            }

            return success;
        }
    }
}
