using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Options;
using System.Reflection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

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
        // Step 1: Bind RabbitMQ settings so messaging config can read them.
        builder.Services.AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Host.UseWolverine((opt) =>
        {
            // Step 2: Register handlers for Wolverine's discovery.
            opt.Discovery.IncludeAssembly(handlersAssembly);

            // Step 3: Enable transactional message handling and outbox persistence.
            opt.UseEntityFrameworkCoreTransactions();
            opt.PersistMessagesWithPostgresql(
                builder.Configuration.GetConnectionString("DefaultConnection")!);
            opt.Policies.UseDurableLocalQueues();
            opt.Policies.UseDurableOutboxOnAllSendingEndpoints();
            opt.Policies.AutoApplyTransactions();

            // Step 4: Load RabbitMQ connection options from configuration.
            var rabbitOptions = builder.Configuration
                .GetSection(RabbitMqOptions.SectionName)
                .Get<RabbitMqOptions>()!;

            // Step 5: Configure RabbitMQ transport and provision endpoints.
            opt.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = rabbitOptions.Host;
                rabbit.Port = rabbitOptions.Port;
                rabbit.UserName = rabbitOptions.Username;
                rabbit.Password = rabbitOptions.Password;
            }).AutoProvision();

            // Step 6: Apply app-specific publish/subscribe routing.
            configureEndpoints(builder, opt);
        });

        return builder;
    }
}