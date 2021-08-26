using Cryptofolio.Infrastructure;
using MediatR;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Handlers.Job
{
    /// <summary>
    /// Provides an implementation of <see cref="IPipelineBehavior{TEvent, Unit}"/> to handle <see cref="TEvent"/> occurence.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    public class EventTraceWriter<TEvent> : IPipelineBehavior<TEvent, Unit> where TEvent : class, IEvent
    {
        private readonly IElasticClient _elasticClient;
        private readonly ISystemClock _systemClock;
        private readonly IOptionsSnapshot<ElasticsearchOptions> _optionsSnapshot;
        private readonly ILogger<EventTraceWriter<TEvent>> _logger;

        private ElasticsearchOptions Options => _optionsSnapshot.Value;

        /// <summary>
        /// Creates a new instance of <see cref="EventTraceWriter"/>.
        /// </summary>
        /// <param name="elasticClient">The elastic client.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="optionsSnapshot">The options snapshot.</param>
        /// <param name="logger">The logger.</param>
        public EventTraceWriter(
            IElasticClient elasticClient,
            ISystemClock systemClock,
            IOptionsSnapshot<ElasticsearchOptions> optionsSnapshot,
            ILogger<EventTraceWriter<TEvent>> logger)
        {
            _elasticClient = elasticClient;
            _systemClock = systemClock;
            _optionsSnapshot = optionsSnapshot;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(TEvent @event, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Storing the {0} event occurence of category {1} and type {2}.", @event.Id, @event.Category, typeof(TEvent).FullName);
            _logger.LogTraceObject("Event", @event);

            var result = await _elasticClient.IndexAsync<IEvent>(
                @event,
                d => d.Index(string.Format(Options.Indices[typeof(IEvent).FullName], _systemClock.UtcNow)),
                cancellationToken
            );
            if (result.IsValid)
            {
                _logger.LogInformation("Event {0} occurence stored successfully.", @event.Id);
                return await next();
            }
            else
            {
                _logger.LogError(result.OriginalException, "An error has occured while storing the event {0} occurence.", @event.Id);
                _logger.LogDebug(result.DebugInformation);
            }

            _logger.LogWarning("The event {0} pipeline will be short circuited.", @event.Id);
            return Unit.Value;
        }
    }
}
