namespace Rokys.Events.Command.Configuration;

/// <summary>
/// Configuración para RabbitMQ
/// </summary>
public class RabbitMQSettings
{
    /// <summary>
    /// Host de RabbitMQ
    /// </summary>
    public string HostName { get; set; } = "172.16.10.12";

    /// <summary>
    /// Puerto de RabbitMQ
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Usuario de RabbitMQ
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Contraseña de RabbitMQ
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Virtual Host de RabbitMQ
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Exchange para eventos de dominio
    /// </summary>
    public string EventsExchange { get; set; } = "rokys.events";

    /// <summary>
    /// Nombre del microservicio (para routing keys automáticas)
    /// </summary>
    public string MicroserviceName { get; set; } = "unknown";

    /// <summary>
    /// Configuraciones específicas por entidad (opcional)
    /// </summary>
    public Dictionary<string, string> EntityRoutingKeys { get; set; } = new();

    /// <summary>
    /// Routing key para eventos de empleados (compatibilidad hacia atrás)
    /// </summary>
    public string EmployeeEventsRoutingKey { get; set; } = "employee.events";

    /// <summary>
    /// Timeout de conexión en millisegundos
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30000;

    /// <summary>
    /// Habilitar reintentos automáticos
    /// </summary>
    public bool EnableRetries { get; set; } = true;

    /// <summary>
    /// Número máximo de reintentos
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}