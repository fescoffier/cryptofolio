using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Infrastructure.IntegrationTests
{
    public class KafkaEventDispatcherTests : TestBase
    {
        private readonly KafkaEventDispatcher _dispatcher;
        private readonly KafkaConsumerWrapper<string, IEvent> _consumerWrapper;

        public KafkaEventDispatcherTests()
        {
            _dispatcher = ResolveServiceFromScope<KafkaEventDispatcher>();
            _consumerWrapper = ResolveServiceFromScope<KafkaConsumerWrapper<string, IEvent>>();
        }

        protected override void ConfigureTestServices(IServiceCollection services) => services
            .AddSingleton<KafkaEventDispatcher>()
            .AddProducer<IEvent>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(IEvent).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();

            })
            .AddConsumer<IEvent>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(IEvent).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });

        [Fact]
        public async Task DispathAsync_Test()
        {
            // Setup
            var @event = new FakeEvent
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.UtcNow,
                UserId = Guid.NewGuid().ToString(),
                Username = "test",
                Property1 = "value1",
                Property2 = "value2"
            };

            // Act
            await _dispatcher.DispatchAsync(@event);

            // Assert
            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var message = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            message.Message.Key.Should().Be(@event.Id);
            message.Message.Value.Should().BeEquivalentTo(@event);
        }
    }

    public class FakeEvent : IEvent
    {
        public string Id { get; init; }

        public DateTimeOffset Date { get; init; }

        public string UserId { get; init; }

        public string Username { get; init; }

        public string Category => "Test";

        public string Property1 { get; init; }

        public string Property2 { get; init; }
    }
}
