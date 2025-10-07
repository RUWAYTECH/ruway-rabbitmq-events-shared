using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rokys.Events.Command.Configuration;
using Rokys.Events.Command.Interfaces.Events;

namespace Rokys.Events.Command.Examples;

/// <summary>
/// Ejemplo de servicio de integración que escucha eventos para sincronizar con sistemas externos
/// </summary>
public class IntegrationService : BackgroundService
{
    private readonly IEventSubscriber _eventSubscriber;
    private readonly ILogger<IntegrationService> _logger;

    public IntegrationService(
        IEventSubscriber eventSubscriber,
        ILogger<IntegrationService> logger)
    {
        _eventSubscriber = eventSubscriber;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("🚀 Iniciando servicio de integración");
            
            await SetupEventSubscriptions();
            await _eventSubscriber.StartListeningAsync(stoppingToken);
            
            // Mantener el servicio ejecutándose
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🛑 Servicio de integración cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en servicio de integración");
            throw;
        }
    }

    private async Task SetupEventSubscriptions()
    {
        // Suscripción para eventos de empleados
        await _eventSubscriber.SubscribeAsync(
            HandleEmployeeEvents,
            "employee.events.*");
        
        // Suscripción para eventos de memos
        await _eventSubscriber.SubscribeAsync(
            HandleMemoEvents,
            "memo.events.*");
        
        // Suscripción genérica para auditoría
        await _eventSubscriber.SubscribeAsync(
            LogAllEvents,
            "#");

        _logger.LogInformation("✅ Suscripciones configuradas");
    }

    private async Task HandleEmployeeEvents(string message, string routingKey)
    {
        _logger.LogInformation("👤 Procesando evento de empleado: {RoutingKey}", routingKey);
        
        try
        {
            if (routingKey.EndsWith(".created"))
            {
                await ProcessEmployeeCreated(message);
            }
            else if (routingKey.EndsWith(".updated"))
            {
                await ProcessEmployeeUpdated(message);
            }
            else if (routingKey.EndsWith(".deleted"))
            {
                await ProcessEmployeeDeleted(message);
            }
            
            _logger.LogDebug("✅ Evento de empleado procesado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando evento de empleado: {RoutingKey}", routingKey);
        }
    }

    private async Task HandleMemoEvents(string message, string routingKey)
    {
        _logger.LogInformation("📄 Procesando evento de memo: {RoutingKey}", routingKey);
        
        try
        {
            // Procesar eventos de memos
            // Por ejemplo: sincronizar con sistema de documentos externo
            await Task.Delay(100); // Simular procesamiento
            
            _logger.LogDebug("✅ Evento de memo procesado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando evento de memo: {RoutingKey}", routingKey);
        }
    }

    private async Task LogAllEvents(string message, string routingKey)
    {
        // Solo loguear, no procesar
        _logger.LogTrace("📝 Auditoria - Evento: {RoutingKey}, Tamaño: {Size} bytes", 
            routingKey, message.Length);
        
        await Task.CompletedTask;
    }

    private async Task ProcessEmployeeCreated(string message)
    {
        _logger.LogInformation("➕ Procesando empleado creado");
        
        // Simular integraciones
        await SimulateExternalIntegration("Creando empleado en Active Directory");
        await SimulateExternalIntegration("Enviando email de bienvenida");
        await SimulateExternalIntegration("Creando cuenta en sistema de nómina");
        await SimulateExternalIntegration("Asignando equipos y licencias");
    }

    private async Task ProcessEmployeeUpdated(string message)
    {
        _logger.LogInformation("✏️ Procesando empleado actualizado");
        
        await SimulateExternalIntegration("Sincronizando cambios con Active Directory");
        await SimulateExternalIntegration("Actualizando sistema de nómina");
    }

    private async Task ProcessEmployeeDeleted(string message)
    {
        _logger.LogInformation("🗑️ Procesando empleado eliminado");
        
        await SimulateExternalIntegration("Deshabilitando cuenta en Active Directory");
        await SimulateExternalIntegration("Procesando baja en sistema de nómina");
        await SimulateExternalIntegration("Revocando accesos y licencias");
    }

    private async Task SimulateExternalIntegration(string action)
    {
        _logger.LogDebug("   🔄 {Action}...", action);
        await Task.Delay(Random.Shared.Next(50, 200)); // Simular tiempo de procesamiento
        _logger.LogDebug("   ✅ {Action} completado", action);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Deteniendo servicio de integración");
        
        try
        {
            await _eventSubscriber.StopListeningAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deteniendo suscriptor de eventos");
        }
        
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _eventSubscriber?.Dispose();
        base.Dispose();
    }
}

/// <summary>
/// Extensiones para configurar el servicio de integración en una aplicación ASP.NET Core
/// </summary>
public static class IntegrationServiceExtensions
{
    /// <summary>
    /// Agrega el servicio de integración para escuchar eventos
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <returns>Colección de servicios configurada</returns>
    public static IServiceCollection AddIntegrationService(this IServiceCollection services)
    {
        services.AddHostedService<IntegrationService>();
        return services;
    }
}