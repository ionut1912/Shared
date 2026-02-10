using JasperFx.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shared.Application.Options;
using System.Reflection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace Shared.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring Wolverine messaging within the application.
/// </summary>
public static class WolverineExtensions
{
    /// <summary>
    /// Configures Wolverine as the command bus and message broker, integrating RabbitMQ, PostgreSQL persistence, and EF Core transactions.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <param name="handlersAssembly">The assembly containing Wolverine message handlers for automatic discovery.</param>
    /// <param name="configureEndpoints">A delegate to define specific message routing, publishing, and subscription rules.</param>
    /// <returns>The configured <see cref="WebApplicationBuilder"/>.</returns>
    /// <remarks>
    /// This method automates:
    /// <list type="bullet">
    /// <item><description>Binding <see cref="RabbitMqOptions"/> from the "RabbitMQ" configuration section.</description></item>
    /// <item><description>Configuring Durable Outbox patterns with PostgreSQL.</description></item>
    /// <item><description>Automatic provisioning of RabbitMQ exchanges and queues.</description></item>
    /// </list>
    /// </remarks>
    public static WebApplicationBuilder AddWolverineMessaging(
        this WebApplicationBuilder builder,
        Assembly handlersAssembly,
        Action<WebApplicationBuilder, WolverineOptions> configureEndpoints)
    {
        builder.Services.AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Host.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(handlersAssembly);

            opts.UseFluentValidation();

            opts.UseEntityFrameworkCoreTransactions();
            opts.PersistMessagesWithPostgresql(
                builder.Configuration.GetConnectionString("DefaultConnection")!, "public");

            opts.Policies.UseDurableLocalQueues();
            opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
            opts.Policies.AutoApplyTransactions();

            opts.Policies.OnException<NpgsqlException>().ScheduleRetry(10.Seconds());
            opts.Policies.OnException<Exception>().RetryTimes(3);

            var rabbitOptions = builder.Configuration
                .GetSection(RabbitMqOptions.SectionName)
                .Get<RabbitMqOptions>();

            opts.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = rabbitOptions!.Host;
                rabbit.Port = rabbitOptions.Port;
                rabbit.UserName = rabbitOptions.Username;
                rabbit.Password = rabbitOptions.Password;
            }).AutoProvision();

            configureEndpoints(builder, opts);
        });

        return builder;
    }
}