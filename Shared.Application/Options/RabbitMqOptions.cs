using System.ComponentModel.DataAnnotations;

namespace Shared.Application.Options;

/// <summary>
///     Represents the configuration settings for connecting to a RabbitMQ message broker.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>
    ///     The key name used in configuration files (e.g., appsettings.json) to bind these options.
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    ///     Gets or sets the hostname or IP address of the RabbitMQ server.
    /// </summary>
    /// <value>Defaults to "localhost".</value>
    [Required]
    public string Host { get; init; } = "localhost";

    /// <summary>
    ///     Gets or sets the port number on which the RabbitMQ service is listening.
    /// </summary>
    /// <value>Defaults to 5672.</value>
    [Range(1, 65535)]
    public int Port { get; init; } = 5672;

    /// <summary>
    ///     Gets or sets the username used for authentication with the broker.
    /// </summary>
    /// <value>Defaults to "guest".</value>
    [Required]
    public string Username { get; init; } = "guest";

    /// <summary>
    ///     Gets or sets the password used for authentication with the broker.
    /// </summary>
    /// <value>Defaults to "guest".</value>
    [Required]
    public string Password { get; init; } = "guest";
}