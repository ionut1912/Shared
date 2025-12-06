
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Rabbit.Repositories;
using Shared.Rabbit.Settings;
using System.Text;


namespace Shared.Rabbit.Services;

public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _chanel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly RabbitMqEventBusOptions _rabbitMqEventBusOptions;
    private readonly Dictionary<string, List<Type>> _handlers = new();

    public RabbitMqEventBus(IConnection connection,IServiceProvider serviceProvider,ILogger<RabbitMqEventBus> logger,RabbitMqEventBusOptions options)
    {
        
       _connection=connection;
        _serviceProvider=serviceProvider;
        _logger=logger;
        _rabbitMqEventBusOptions=options;
        _chanel = connection.CreateModel();

        DeclareExchanges();
    }

    private void DeclareExchanges()
    {
        foreach(var exchange in _rabbitMqEventBusOptions.ExchangeConfigurations)
        {
            _chanel.ExchangeDeclare(exchange.Name, exchange.Type, exchange.Durable, exchange.AutoDelete, null);

            _logger.LogInformation("Declared Exchange : {ExchangeName} (Type:{ExchangeType})", exchange.Name, exchange.Type);
        }
    }

    public void Dispose()
    {
        _chanel?.Dispose();
        _connection?.Dispose();
    }

    public async Task PublishAsync<T>(T @event) where T : IEvent
    {
        var eventName = typeof(T).Name;
        var routingKey=GetRoutingKey(eventName);
        var body=Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
        var exchange = GetExchange(eventName);
        _chanel.BasicPublish(exchange, routingKey, null, body);
        _logger.LogInformation("Published event {EventNam}e to exchange {Exchange} with routing key {RoutingKey}", eventName, exchange, routingKey);
    }

    public void Subscribe<T, TH>() where T : IEvent
        where TH : class, IEventHandler<T>
    {
       var eventName= typeof(T).Name;
       var handlerType = typeof(TH);
        if (!_handlers.ContainsKey(eventName))
        {
            _handlers[eventName] = new List<Type>();
        }
        _handlers[eventName].Add(handlerType);
        var queueName = GetQueueName(eventName, handlerType.Name);
        var routingQey = GetRoutingKey(eventName);
        var exchange=GetExchange(eventName);
        _chanel.QueueDeclare(queueName,true, false, false, null);
        _chanel.QueueBind(queueName, exchange, routingQey);
        var consumer = new EventingBasicConsumer(_chanel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var @event = JsonConvert.DeserializeObject<T>(message);

                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TH>();
                await handler.Handle(@event);

                _chanel.BasicAck(ea.DeliveryTag, false);
                _logger.LogInformation("Handled event {EventName}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event {EventName}", eventName);
                _chanel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _chanel.BasicConsume(queueName, false, consumer);
        _logger.LogInformation("Subscribed to event {EventName} with handler {handlerName}",eventName,handlerType.Name);
    }

    public void Unsubscribe<T, TH>() where T : IEvent
        where TH : class, IEventHandler<T>
    {
        var eventName = typeof(T).Name;
        var handlerType = typeof(TH);
        if (_handlers.ContainsKey(eventName))
        {
            _handlers[eventName].Remove(handlerType);
        }
    }

    private string GetRoutingKey(string eventName)
    {
        if (_rabbitMqEventBusOptions.RoutingKeyResolver != null)
        {
            return _rabbitMqEventBusOptions.RoutingKeyResolver(eventName);
        }

        return eventName.ToLowerInvariant().Replace("event", "");
    }

    private string GetExchange(string eventName)
    {
        if (_rabbitMqEventBusOptions.ExchangeResolver != null)
        {
            return _rabbitMqEventBusOptions.ExchangeResolver(eventName);
        }

        if (_rabbitMqEventBusOptions.ExchangeConfigurations.Count == 0)
        {
            throw new InvalidOperationException("No exchanges configured");
        }

        return _rabbitMqEventBusOptions.ExchangeConfigurations[0].Name;
    }

    private static string GetQueueName(string eventName,string handlerName)
    {
         return $"{eventName.ToLowerInvariant()}.{handlerName.ToLowerInvariant()}";
    }
}
