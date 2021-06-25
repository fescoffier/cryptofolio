using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Cryptofolio.Infrastructure.IntegrationTests
{
    public class Config
    {
        public static IConfiguration BuildConfiguration(Action<IConfigurationBuilder> configureAction = null)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:Cryptofolio", $"Host=localhost;Database={Guid.NewGuid()};Username=cryptofolio;Password=Pass@word1;Port=55432;IncludeErrorDetails=true" },
                    { "Kafka:Topics:Cryptofolio.Infrastructure.IntegrationTests.TestMessage", Guid.NewGuid().ToString() },
                    { "Kafka:Producer:BootstrapServers", "localhost:9092" },
                    { "Kafka:Consumer:BootstrapServers", "localhost:9092" },
                    { "Kafka:Consumer:GroupId", "test" },
                    { "Kafka:Consumer:AutoOffsetReset", "Earliest" },
                    { "Kafka:Consumer:EnablePartitionEof", "true" },
                    { "Kafka:Consumer:EnableAutoCommit", "false" },
                    { "Kafka:Consumer:AllowAutoCreateTopics", "true" }
                });
            configureAction?.Invoke(configBuilder);
            return configBuilder.Build();
        }
    }
}
