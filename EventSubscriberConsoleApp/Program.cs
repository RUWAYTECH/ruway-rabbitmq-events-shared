using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rokys.Events.Command.Configuration;
using Rokys.Events.Command.Interfaces.Events;

namespace EventSubscriberConsoleApp;

/// <summary>
/// Ejemplo de uso del suscriptor de eventos en una aplicación consola
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Configurar servicios
        var services = new ServiceCollection();
        
        // Configurar logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Configurar eventos con RabbitMQ
        services.AddEventPublisher(new RabbitMQSettings
        {
            HostName = "172.16.10.12",
            Port = 5672,
            UserName = "owner",
            Password = "P4ss@78_#%a9",
            EventsExchange = "rokys.memo.events"
        });
        
        var serviceProvider = services.BuildServiceProvider();
        var subscriber = serviceProvider.GetRequiredService<IEventSubscriber>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("=== Iniciando Suscriptor de Eventos ===");
            
            // Ejemplo 1: Suscripción genérica para logging
            await subscriber.SubscribeAsync(async (message, routingKey) =>
            {
                logger.LogInformation("📨 Evento recibido [{RoutingKey}]: {Message}", 
                    routingKey, message);
                await Task.CompletedTask;
            }, "#"); // Escuchar todos los eventos
            
            // Ejemplo 2: Suscripción específica para eventos de empleados
            await subscriber.SubscribeAsync(async (message, routingKey) =>
            {
                logger.LogInformation("👤 Evento de empleado [{RoutingKey}]: {Message}", 
                    routingKey, message);
                    
                // Aquí podrías procesar el evento específicamente
                // Por ejemplo: enviar email, actualizar cache, etc.
                await ProcessEmployeeEvent(message, routingKey);
            }, "employee.events.*");
            
            logger.LogInformation("🔄 Iniciando escucha de eventos...");
            await subscriber.StartListeningAsync();
            
            logger.LogInformation("✅ Suscriptor iniciado. Presiona Ctrl+C para salir");
            
            // Mantener la aplicación ejecutándose
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            
            try
            {
                await Task.Delay(-1, cts.Token);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("🛑 Deteniendo suscriptor...");
            }
            
            await subscriber.StopListeningAsync();
            logger.LogInformation("✅ Suscriptor detenido correctamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error en el suscriptor de eventos");
        }
        finally
        {
            subscriber.Dispose();
        }
    }
    
    private static async Task ProcessEmployeeEvent(string message, string routingKey)
    {
        // Simular procesamiento
        await Task.Delay(100);
        Console.WriteLine("   -> Procesando evento de empleado...");
        Console.WriteLine("   -> Enviando notificación...");
        Console.WriteLine("   -> Actualizando sistemas externos...");
    }
}
