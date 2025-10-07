using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ruway.Events.Command.Events;
using Ruway.Events.Command.Interfaces.Events;
using System.Reflection;

namespace Ruway.Events.Command.Configuration;

/// <summary>
/// Extensiones para configurar la inyección de dependencias del publicador de eventos
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura los servicios del publicador y suscriptor de eventos con RabbitMQ
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <returns>Colección de servicios configurada</returns>
    public static IServiceCollection AddEventPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar RabbitMQ settings
        var rabbitMQSection = configuration.GetSection("RabbitMQ");
        services.Configure<RabbitMQSettings>(rabbitMQSection);

        // Registrar MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Registrar servicios
        services.AddSingleton<IRabbitMQService, RabbitMQService>();
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddSingleton<IEventSubscriber, EventSubscriber>();

        return services;
    }

    /// <summary>
    /// Configura los servicios del publicador y suscriptor de eventos con configuración personalizada de RabbitMQ
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="rabbitMQSettings">Configuración personalizada de RabbitMQ</param>
    /// <returns>Colección de servicios configurada</returns>
    public static IServiceCollection AddEventPublisher(this IServiceCollection services, RabbitMQSettings rabbitMQSettings)
    {
        // Configurar RabbitMQ settings
        services.Configure<RabbitMQSettings>(options =>
        {
            options.HostName = rabbitMQSettings.HostName;
            options.Port = rabbitMQSettings.Port;
            options.UserName = rabbitMQSettings.UserName;
            options.Password = rabbitMQSettings.Password;
            options.VirtualHost = rabbitMQSettings.VirtualHost;
            options.EventsExchange = rabbitMQSettings.EventsExchange;
            options.EmployeeEventsRoutingKey = rabbitMQSettings.EmployeeEventsRoutingKey;
            options.ConnectionTimeout = rabbitMQSettings.ConnectionTimeout;
            options.EnableRetries = rabbitMQSettings.EnableRetries;
            options.MaxRetries = rabbitMQSettings.MaxRetries;
        });

        // Registrar MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Registrar servicios
        services.AddSingleton<IRabbitMQService, RabbitMQService>();
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddSingleton<IEventSubscriber, EventSubscriber>();

        return services;
    }

    /// <summary>
    /// Configura el suscriptor de eventos como un servicio en segundo plano
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <returns>Colección de servicios configurada</returns>
    public static IServiceCollection AddEventSubscriberHostedService(this IServiceCollection services)
    {
        services.AddHostedService<EventSubscriberHostedService>();
        return services;
    }
}