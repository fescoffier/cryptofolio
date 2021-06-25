using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptofolio.Infrastructure
{
    /// <summary>
    /// Provides an handler to handle Kafka message as a background service.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class KafkaMessageHandler<TMessage> : IHostedService, IAsyncDisposable
    {
        private bool _stopCalled;
        private CancellationTokenSource _cancellation;
        private Task _runningTask;

        private readonly IServiceProvider _provider;
        private readonly KafkaConsumerWrapper<string, TMessage> _consumerWrapper;
        private readonly ILogger<KafkaMessageHandler<TMessage>> _logger;

        private IConsumer<string, TMessage> Consumer => _consumerWrapper.Consumer;

        private KafkaConsumerOptions<TMessage> ConsumerOptions => _consumerWrapper.Options;

        /// <summary>
        /// Creates a new instance of <see cref="KafkaMessageHandler{TMessage}"/>.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <param name="consumerWrapper">The consumer wrapper.</param>
        /// <param name="logger">The logger.</param>
        public KafkaMessageHandler(
            IServiceProvider provider,
            KafkaConsumerWrapper<string, TMessage> consumerWrapper,
            ILogger<KafkaMessageHandler<TMessage>> logger)
        {
            _provider = provider;
            _consumerWrapper = consumerWrapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (!_stopCalled)
            {
                await StopAsync(CancellationToken.None);
            }

            _cancellation?.Dispose();
            _runningTask?.Dispose();
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ConsumerOptions.Topic))
            {
                throw new InvalidOperationException($"Missing topic name for the message type : {typeof(TMessage).FullName}");
            }

            _logger.LogInformation("Subscribing to topic {0}.", ConsumerOptions.Topic);
            Consumer.Subscribe(ConsumerOptions.Topic);
            _stopCalled = false;
            _cancellation = new();
            _runningTask = ExecuteAsync(_cancellation.Token);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_runningTask == null)
                {
                    return;
                }

                _cancellation.Cancel();
                await _runningTask;

                _logger.LogInformation("Closing consumer.");
                Consumer.Close();
            }
            finally
            {
                _stopCalled = true;
            }
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = await Consume(stoppingToken);
                    if (cr.IsPartitionEOF)
                    {
                        _logger.LogInformation("End of partition {0} in the topic {1}.", cr.Partition.Value, cr.Topic);
                    }
                    else
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            break;
                        }

                        _logger.LogInformation("Consumed message at offset {0} in the partition {1}.", cr.Offset, cr.Partition.Value);
                        _logger.LogTraceObject("Consume result", cr);

                        using var scope = _provider.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        await mediator.Send(cr.Message.Value, stoppingToken);

                        _logger.LogDebug("Committing offset {0}.", cr.Offset);
                        Consumer.Commit(cr);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("The message consumption has been cancelled.");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error has occured while consuming a message from the topic {0}.", ConsumerOptions.Topic);
                }
            }
        }

        private Task<ConsumeResult<string, TMessage>> Consume(CancellationToken stoppingToken) => Task.Run(() => Consumer.Consume(stoppingToken));
    }
}
