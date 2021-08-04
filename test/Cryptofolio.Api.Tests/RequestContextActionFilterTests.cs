using Cryptofolio.Api.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Cryptofolio.Api.Tests
{
    public class RequestContextActionFilterTests
    {
        [Fact]
        public void OnActionExecuting_Test()
        {
            // Setup
            var command = new FakeCommand();
            var context = new ActionExecutingContext(
                new()
                {
                    ActionDescriptor = new(),
                    RouteData = new(),
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = new ServiceCollection()
                            .AddSingleton(TestContext.Instance.RequestContext)
                            .BuildServiceProvider()
                    }
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>
                {
                    { "command", command }
                },
                null
            );
            var actionFilter = new RequestContextActionFilter();

            // Act
            actionFilter.OnActionExecuting(context);

            // Assert
            command.RequestContext.Should().BeSameAs(TestContext.Instance.RequestContext);
        }

        [Fact]
        public void OnActionExecuting_InvalidOperationException_Test()
        {
            // Setup
            var command = new FakeCommand();
            var context = new ActionExecutingContext(
                new()
                {
                    ActionDescriptor = new(),
                    RouteData = new(),
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = new ServiceCollection().BuildServiceProvider()
                    }
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>
                {
                    { "command", command }
                },
                null
            );
            var actionFilter = new RequestContextActionFilter();

            // Act & Assert
            actionFilter.Invoking(f => f.OnActionExecuting(context)).Should().Throw<InvalidOperationException>();
        }

        private class FakeCommand : CommandBase
        {
        }
    }
}
