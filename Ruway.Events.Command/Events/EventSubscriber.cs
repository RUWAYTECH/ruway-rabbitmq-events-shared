using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ruway.Events.Command.Configuration;
using Ruway.Events.Command.Interfaces.Events;
using System.Collections.Concurrent;
using System.Text;

namespace Ruway.Events.Command.Events;

/// <summary>
/// Implementación del suscriptor de eventos para RabbitMQ
/// </summary>
public class EventSubscriber : IEventSubscriber
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<EventSubscriber> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private string? _queueName;
    private readonly object _lockObject = new();
    private bool _disposed = false;
    private bool _isListening = false;

    // Almacén de handlers registrados
    private readonly ConcurrentDictionary<string, List<Func<string, string, Task>>> _genericHandlers = new();
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _typedHandlers = new();

    public EventSubscriber(IOptions<RabbitMQSettings> settings, ILogger<EventSubscriber> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SubscribeAsync<TEvent>(
        Func<TEvent, Task> handler, 
        string routingKey = "*", 
        CancellationToken cancellationToken = default) 
        where TEvent : IDomainEvent
    {
        _logger.LogInformation("Subscribing to event type {EventType} with routing key {RoutingKey}", 
            typeof(TEvent).Name, routingKey);

        // Convertir el handler tipado a uno genérico
        Func<object, Task> genericHandler = async (eventObj) =>
        {
            if (eventObj is TEvent typedEvent)
            {
                await handler(typedEvent);
            }
            else
            {
                _logger.LogWarning("Received event of unexpected type {ActualType}, expected {ExpectedType}", 
                    eventObj.GetType().Name, typeof(TEvent).Name);
            }
        };

        // Registrar el handler
        var eventType = typeof(TEvent);
        _typedHandlers.AddOrUpdate(eventType, 
            new List<Func<object, Task>> { genericHandler },
            (key, existingHandlers) =>
            {
                existingHandlers.Add(genericHandler);
                return existingHandlers;
            });

        // Registrar también por routing key para filtrado
        _genericHandlers.AddOrUpdate(routingKey,
            new List<Func<string, string, Task>> 
            { 
                async (message, key) => await ProcessTypedMessage<TEvent>(message, key, eventType)
            },
            (key, existingHandlers) =>
            {
                existingHandlers.Add(async (message, key) => await ProcessTypedMessage<TEvent>(message, key, eventType));
                return existingHandlers;
            });

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task SubscribeAsync(
        Func<string, string, Task> handler, 
        string routingKey = "*", 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Subscribing to generic events with routing key {RoutingKey}", routingKey);

        _genericHandlers.AddOrUpdate(routingKey,
            new List<Func<string, string, Task>> { handler },
            (key, existingHandlers) =>
            {
                existingHandlers.Add(handler);
                return existingHandlers;
            });

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        if (_isListening)
        {
            _logger.LogWarning("Event subscriber is already listening");
            return;
        }

        try
        {
            _logger.LogInformation("Starting RabbitMQ event subscriber");

            EnsureConnectionAndChannel();
            
            // Crear una cola exclusiva y temporal para este suscriptor
            _queueName = _channel!.QueueDeclare(
                queue: "", 
                durable: false, 
                exclusive: true, 
                autoDelete: true, 
                arguments: null).QueueName;

            // Vincular la cola al exchange con todos los routing keys registrados
            foreach (var routingKey in _genericHandlers.Keys)
            {
                _channel.QueueBind(
                    queue: _queueName,
                    exchange: _settings.EventsExchange,
                    routingKey: routingKey == "*" ? "#" : routingKey);
                    
                _logger.LogDebug("Bound queue {QueueName} to exchange {Exchange} with routing key {RoutingKey}",
                    _queueName, _settings.EventsExchange, routingKey);
            }

            // Si no hay routing keys específicos, vincular a todos
            if (!_genericHandlers.Any())
            {
                _channel.QueueBind(
                    queue: _queueName,
                    exchange: _settings.EventsExchange,
                    routingKey: "#");
            }

            // Configurar el consumidor
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    await ProcessMessage(ea);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing received message");
                }
            };

            // Iniciar el consumo
            _channel.BasicConsume(
                queue: _queueName,
                autoAck: true, // Auto-acknowledge para simplificar
                consumer: consumer);

            _isListening = true;
            _logger.LogInformation("RabbitMQ event subscriber started successfully. Queue: {QueueName}", _queueName);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ event subscriber");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopListeningAsync(CancellationToken cancellationToken = default)
    {
        if (!_isListening)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Stopping RabbitMQ event subscriber");

            _channel?.Close();
            _connection?.Close();
            
            _isListening = false;
            _logger.LogInformation("RabbitMQ event subscriber stopped successfully");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping RabbitMQ event subscriber");
            throw;
        }
    }

    /// <summary>
    /// Procesa un mensaje recibido de RabbitMQ
    /// </summary>
    private async Task ProcessMessage(BasicDeliverEventArgs ea)
    {
        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
        var routingKey = ea.RoutingKey;

        _logger.LogDebug("Received message with routing key {RoutingKey}: {Message}", routingKey, message);

        // Procesar con handlers genéricos que coincidan con el routing key
        await ProcessGenericHandlers(message, routingKey);
    }

    /// <summary>
    /// Procesa handlers genéricos
    /// </summary>
    private async Task ProcessGenericHandlers(string message, string routingKey)
    {
        var tasks = new List<Task>();

        foreach (var kvp in _genericHandlers)
        {
            var pattern = kvp.Key;
            var handlers = kvp.Value;

            // Verificar si el routing key coincide con el pattern
            if (MatchesRoutingPattern(routingKey, pattern))
            {
                foreach (var handler in handlers)
                {
                    tasks.Add(handler(message, routingKey));
                }
            }
        }

        if (tasks.Any())
        {
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Procesa un mensaje tipado
    /// </summary>
    private async Task ProcessTypedMessage<TEvent>(string message, string routingKey, Type eventType) 
        where TEvent : IDomainEvent
    {
        try
        {
            var deserializedEvent = JsonConvert.DeserializeObject<TEvent>(message);
            if (deserializedEvent != null)
            {
                if (_typedHandlers.TryGetValue(eventType, out var handlers))
                {
                    var tasks = handlers.Select(h => h(deserializedEvent));
                    await Task.WhenAll(tasks);
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message as {EventType}: {Message}", typeof(TEvent).Name, message);
        }
    }

    /// <summary>
    /// Verifica si un routing key coincide con un pattern
    /// </summary>
    private bool MatchesRoutingPattern(string routingKey, string pattern)
    {
        if (pattern == "*" || pattern == "#")
            return true;

        if (pattern.Contains("*") || pattern.Contains("#"))
        {
            // Implementar lógica de wildcards básica
            var regex = pattern.Replace("*", "[^.]*").Replace("#", ".*");
            return System.Text.RegularExpressions.Regex.IsMatch(routingKey, $"^{regex}$");
        }

        return routingKey == pattern;
    }

    /// <summary>
    /// Asegura que la conexión y el canal estén disponibles
    /// </summary>
    private void EnsureConnectionAndChannel()
    {
        lock (_lockObject)
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
                return;

            try
            {
                // Crear conexión
                var factory = new ConnectionFactory()
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Asegurar que el exchange existe
                _channel.ExchangeDeclare(
                    exchange: _settings.EventsExchange,
                    type: ExchangeType.Topic,
                    durable: true);

                _logger.LogDebug("RabbitMQ connection and channel established for subscriber");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection for subscriber");
                throw;
            }
        }
    }

    /// <summary>
    /// Dispose pattern implementation
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            StopListeningAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during EventSubscriber disposal");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}