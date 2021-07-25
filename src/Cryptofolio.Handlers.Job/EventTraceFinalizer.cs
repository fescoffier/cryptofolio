using Cryptofolio.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Handlers.Job
{
    /// <summary>
    /// Provides an implementation of <see cref="IPipelineBehavior{IEvent, Unit}"/> to finalize <see cref="TEvent"/> handling in the pipeline.
    /// It ensures there is always a final middleware in the pipeline to avoid <see cref="MediatR.Mediator"/> exception.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    public class EventTraceFinalizer<TEvent> : IPipelineBehavior<TEvent, Unit> where TEvent : class, IEvent
    {
        private readonly ILogger<EventTraceFinalizer<TEvent>> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="EventTraceFinalizer{TEvent}"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public EventTraceFinalizer(ILogger<EventTraceFinalizer<TEvent>> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(TEvent request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Event {0} occurence handled succesfully.", request.Id);
            return Task.FromResult(Unit.Value);
        }
    }
}
