# 📡 Ejemplos de Suscripciones - Casos de Uso Específicos

## 🎯 **Casos de Uso Implementados**

### **Caso 1: Audit escucha creación de empleados en Memos**
### **Caso 2: Memos y Audit escuchan creación de usuarios en Security**

---

## 🏗️ **Configuraciones Actualizadas**

### **📊 MS Audit - Configuración**
```json
{
  "Subscriptions": [
    {
      "RoutingKey": "memos.employee.created",
      "QueueName": "audit.employee_created_handler",
      "Description": "Audita cuando se crean empleados en Memos"
    },
    {
      "RoutingKey": "security.user.created",
      "QueueName": "audit.user_created_handler", 
      "Description": "Audita cuando Security crea usuarios"
    },
    {
      "RoutingKey": "#",
      "QueueName": "audit.all_events_handler",
      "Description": "Captura todos los eventos del sistema para auditoría completa"
    }
  ]
}
```

### **📝 MS Memos - Configuración**
```json
{
  "Subscriptions": [
    {
      "RoutingKey": "security.user.created",
      "QueueName": "memos.user_created_handler",
      "Description": "Escucha cuando Security crea un usuario para sincronizar datos"
    },
    {
      "RoutingKey": "security.user.updated", 
      "QueueName": "memos.user_updated_handler",
      "Description": "Escucha actualizaciones de usuarios desde Security"
    }
  ]
}
```

### **🔐 MS Security - Configuración**
```json
{
  "Subscriptions": []
  // Security solo publica, no necesita suscribirse a otros eventos por ahora
}
```

---

## 💻 **Implementación de Handlers**

### **🔍 MS Audit - Handlers**

#### **Handler para Empleados Creados**
```csharp
public class EmployeeCreatedAuditHandler : INotificationHandler<EmployeeCreatedEvent>
{
    private readonly IAuditService _auditService;
    
    public EmployeeCreatedAuditHandler(IAuditService auditService)
    {
        _auditService = auditService;
    }
    
    public async Task Handle(EmployeeCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _auditService.LogEventAsync(new AuditEntry
        {
            EventType = "EmployeeCreated",
            MicroserviceSource = "memos",
            EntityId = notification.Id,
            EntityType = "Employee",
            Details = JsonConvert.SerializeObject(notification),
            Timestamp = DateTime.UtcNow,
            Action = "CREATE"
        });
    }
}
```

#### **Handler para Usuarios Creados**
```csharp
public class UserCreatedAuditHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IAuditService _auditService;
    
    public UserCreatedAuditHandler(IAuditService auditService)
    {
        _auditService = auditService;
    }
    
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _auditService.LogEventAsync(new AuditEntry
        {
            EventType = "UserCreated",
            MicroserviceSource = "security",
            EntityId = notification.UserId,
            EntityType = "User",
            Details = JsonConvert.SerializeObject(notification),
            Timestamp = DateTime.UtcNow,
            Action = "CREATE",
            CriticalLevel = "HIGH" // Usuarios son críticos para seguridad
        });
    }
}
```

### **📝 MS Memos - Handlers**

#### **Handler para Sincronización de Usuarios**
```csharp
public class UserCreatedSyncHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<UserCreatedSyncHandler> _logger;
    
    public UserCreatedSyncHandler(IEmployeeService employeeService, ILogger<UserCreatedSyncHandler> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }
    
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar si existe un empleado con el mismo email
            var employee = await _employeeService.GetByEmailAsync(notification.Email);
            
            if (employee != null)
            {
                // Vincular usuario con empleado existente
                employee.UserId = notification.UserId;
                employee.IsActiveUser = true;
                await _employeeService.UpdateAsync(employee);
                
                _logger.LogInformation($"Usuario {notification.UserId} vinculado con empleado {employee.Id}");
            }
            else
            {
                _logger.LogWarning($"No se encontró empleado con email {notification.Email} para vincular con usuario {notification.UserId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al sincronizar usuario creado: {notification.UserId}");
        }
    }
}
```

---

## 🚀 **Registro de Suscripciones en Startup**

### **MS Audit - Startup.cs**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configurar EventSubscriber
    services.AddScoped<IEventSubscriber, EventSubscriber>();
    services.AddHostedService<EventSubscriberHostedService>();
    
    // Registrar handlers de auditoría
    services.AddScoped<INotificationHandler<EmployeeCreatedEvent>, EmployeeCreatedAuditHandler>();
    services.AddScoped<INotificationHandler<UserCreatedEvent>, UserCreatedAuditHandler>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Las suscripciones se configuran automáticamente mediante EventSubscriberHostedService
    // que lee la configuración de appsettings.audit.json
}
```

### **MS Memos - Startup.cs**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configurar EventSubscriber
    services.AddScoped<IEventSubscriber, EventSubscriber>();
    services.AddHostedService<EventSubscriberHostedService>();
    
    // Registrar handlers de sincronización
    services.AddScoped<INotificationHandler<UserCreatedEvent>, UserCreatedSyncHandler>();
    services.AddScoped<INotificationHandler<UserUpdatedEvent>, UserUpdatedSyncHandler>();
}
```

---

## 📊 **Flujo de Eventos**

### **Flujo 1: Creación de Empleado**
```
1. MS Memos: Crea empleado
   ↓
2. MS Memos: Publica EmployeeCreatedEvent
   Routing: "memos.employee.created"
   ↓
3. MS Audit: Recibe evento
   Queue: "audit.employee_created_handler"
   ↓
4. MS Audit: Registra en log de auditoría
```

### **Flujo 2: Creación de Usuario**
```
1. MS Security: Crea usuario
   ↓ 
2. MS Security: Publica UserCreatedEvent
   Routing: "security.user.created"
   ↓
3a. MS Audit: Recibe evento
    Queue: "audit.user_created_handler"
    ↓
    Registra en log de auditoría

3b. MS Memos: Recibe evento  
    Queue: "memos.user_created_handler"
    ↓
    Busca empleado con mismo email
    ↓
    Vincula usuario con empleado si existe
```

---

## 🎯 **Ventajas de Esta Configuración**

### ✅ **Auditoría Completa**
- MS Audit captura **TODOS** los eventos con pattern `#`
- Registro específico para eventos críticos (empleados, usuarios)
- Trazabilidad completa del sistema

### ✅ **Sincronización Automática**
- MS Memos se sincroniza automáticamente con usuarios de Security
- Vinculación automática usuario-empleado
- Consistencia de datos entre microservicios

### ✅ **Flexibilidad**
- Cada microservicio define sus propias suscripciones
- Fácil agregar/remover handlers
- Configuración declarativa en JSON

### ✅ **Escalabilidad**
- Colas independientes por handler
- Procesamiento asíncrono
- Resistente a fallos

---

## 🚀 **¡El sistema está listo para los casos de uso específicos!**