using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cryptofolio.Infrastructure.IntegrationTests
{
    public class KafkaMessageHandlerTests : TestBase
    {
        private readonly KafkaMessageHandler<TestMessage> _service;
        private readonly KafkaProducerWrapper<string, TestMessage> _producerWrapper;
        private readonly Mock<IRequestHandler<TestMessage, Unit>> _handlerMock = new();

        public KafkaMessageHandlerTests()
        {
            _service = ResolveServiceFromScope<KafkaMessageHandler<TestMessage>>();
            _producerWrapper = ResolveServiceFromScope<KafkaProducerWrapper<string, TestMessage>>();
        }

        protected override void ConfigureTestServices(IServiceCollection services) => services
            .AddProducer<TestMessage>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(TestMessage).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();

            })
            .AddConsumer<TestMessage>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(TestMessage).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            })
            .AddSingleton<KafkaMessageHandler<TestMessage>>()
            .AddScoped<IRequestHandler<TestMessage, Unit>>(_ => _handlerMock.Object);

        [Fact]
        public async Task Test()
        {
            // Setup
            var message = new TestMessage
            {
                Id = "871aed5f-2d5f-4090-9f54-752f23cef2a0",
                Message = "Lorem ipsum dolor sit amet"
            };
            var topic = _producerWrapper.Options.Topic;
            var handled = false;
            _handlerMock
                .Setup(m => m.Handle(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
                .Callback<TestMessage, CancellationToken>((m, cancellationToken) =>
                {
                    if (m.Id == message.Id && m.Message == message.Message)
                    {
                        handled = true;
                    }
                })
                .ReturnsAsync(Unit.Value);

            // Act
            await _service.StartAsync(default);
            await _producerWrapper.Producer.ProduceAsync(topic, new() { Key = Guid.NewGuid().ToString(), Value = message });
            while (!handled) ;
            await _service.StopAsync(default);

            // Assert
            _handlerMock
                .Verify(m => m
                    .Handle(
                        It.Is<TestMessage>(m => m.Id == message.Id && m.Message == message.Message),
                        It.IsAny<CancellationToken>()
                    ),
                    Times.Once()
                );
        }
    }
}
