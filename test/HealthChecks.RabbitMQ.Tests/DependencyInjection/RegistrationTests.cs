﻿using FluentAssertions;
using HealthChecks.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Xunit;

namespace HealthChecks.RabbitMQ.Tests.DependencyInjection
{
    public class rabbitmq_registration_should
    {
        private string _fakeConnectionString = "amqp://server";
        private string _defaultCheckName = "rabbitmq";

        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddRabbitMQ(rabbitConnectionString: _fakeConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(_defaultCheckName);
            check.GetType().Should().Be(typeof(RabbitMQHealthCheck));

            ((RabbitMQHealthCheck)check).Dispose();
            var result = check.CheckHealthAsync(new HealthCheckContext { Registration = new HealthCheckRegistration("", check, null, null) }).Result;
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Exception.GetType().Should().Be(typeof(ObjectDisposedException));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            var customCheckName = "my-" + _defaultCheckName;

            services.AddHealthChecks()
                .AddRabbitMQ(_fakeConnectionString, name: customCheckName);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(customCheckName);
            check.GetType().Should().Be(typeof(RabbitMQHealthCheck));

            ((RabbitMQHealthCheck)check).Dispose();
            var result = check.CheckHealthAsync(new HealthCheckContext { Registration = new HealthCheckRegistration("", check, null, null) }).Result;
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Exception.GetType().Should().Be(typeof(ObjectDisposedException));
        }
    }
}
