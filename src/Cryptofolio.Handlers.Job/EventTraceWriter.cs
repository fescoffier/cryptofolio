using Cryptofolio.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Nest;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Handlers.Job
{
    /// <summary>
    /// Provides an implementation of <see cref="IPipelineBehavior{IEvent, Unit}"/> to handle <see cref="IEvent"/> occurence.
    /// </summary>
    public class EventTraceWriter : IPipelineBehavior<IEvent, Unit>
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<EventTraceWriter> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="EventTraceWriter"/>.
        /// </summary>
        /// <param name="elasticClient">The elastic client.</param>
        /// <param name="logger">The logger.</param>
        public EventTraceWriter(IElasticClient elasticClient, ILogger<EventTraceWriter> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(IEvent @event, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            // TODO: It could be a good idea to use a different response type, and then check the handling result here.

            _logger.LogInformation("Storing the {0} event occurence of category {1} and type {2}.", @event.Id, @event.Category, @event.GetType().FullName);
            _logger.LogTraceObject("Event", @event);

            var result = await _elasticClient.IndexDocumentAsync(@event, cancellationToken);
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
