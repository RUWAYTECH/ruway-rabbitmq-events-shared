using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Rokys.Events.Command.Configuration;
using Rokys.Events.Command.Interfaces.Events;
using System.Text;

namespace Rokys.Events.Command.Events;

/// <summary>
/// Implementación del servicio de RabbitMQ
/// </summary>
public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQService> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lockObject = new();

    public RabbitMQService(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        InitializeConnection();
    }

    /// <summary>
    /// Inicializa la conexión con RabbitMQ
    /// </summary>
    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(_settings.ConnectionTimeout),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection("Rokys.Memo.EventPublisher");
            _channel = _connection.CreateModel();

            // Declarar el exchange para eventos
            _channel.ExchangeDeclare(
                exchange: _settings.EventsExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation("RabbitMQ connection established successfully to {HostName}:{Port}", 
                _settings.HostName, _settings.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection to {HostName}:{Port}", 
                _settings.HostName, _settings.Port);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task PublishEventAsync(IDomainEvent domainEvent, string routingKey, CancellationToken cancellationToken = default)
    {
        if (_channel == null || _connection == null)
        {
            _logger.LogWarning("RabbitMQ connection is not available. Attempting to reconnect...");
            InitializeConnection();
        }

        var retryCount = 0;
        var maxRetries = _settings.EnableRetries ? _settings.MaxRetries : 0;

        while (retryCount <= maxRetries)
        {
            try
            {
                var eventData = new
                {
                    EventId = domainEvent.EventId,
                    EventName = domainEvent.EventName,
                    Version = domainEvent.Version,
                    OccurredOn = domainEvent.OccurredOn,
                    Data = domainEvent
                };

                var message = JsonConvert.SerializeObject(eventData, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.None
                });

                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel!.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = domainEvent.EventId.ToString();
                properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)domainEvent.OccurredOn).ToUnixTimeSeconds());
                properties.Type = domainEvent.EventName;
                properties.ContentType = "application/json";
                properties.ContentEncoding = "utf-8";

                lock (_lockObject)
                {
                    _channel.BasicPublish(
                        exchange: _settings.EventsExchange,
                        routingKey: routingKey,
                        basicProperties: properties,
                        body: body);
                }

                _logger.LogInformation("Successfully published event {EventName} with ID {EventId} to RabbitMQ",
                    domainEvent.EventName, domainEvent.EventId);

                break; // Éxito, salir del bucle de reintentos
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to publish event {EventName} with ID {EventId}. Retry {RetryCount}/{MaxRetries}",
                    domainEvent.EventName, domainEvent.EventId, retryCount, maxRetries);

                // Esperar antes del siguiente intento
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), cancellationToken);

                // Intentar reconectar
                try
                {
                    InitializeConnection();
                }
                catch (Exception reconnectEx)
                {
                    _logger.LogWarning(reconnectEx, "Failed to reconnect to RabbitMQ during retry {RetryCount}", retryCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event {EventName} with ID {EventId} to RabbitMQ after {RetryCount} retries",
                    domainEvent.EventName, domainEvent.EventId, retryCount);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            return _connection?.IsOpen == true && _channel?.IsOpen == true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ health check failed");
            return false;
        }
    }

    /// <summary>
    /// Libera los recursos de RabbitMQ
    /// </summary>
    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ connection disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing RabbitMQ connection");
        }
    }
}