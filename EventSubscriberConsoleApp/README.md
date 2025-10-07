# EventSubscriberConsoleApp

Aplicación de consola de ejemplo que demuestra la publicación y suscripción de eventos de creación de empleados usando RabbitMQ.

## Funcionalidades

### 🔥 Publicación de Eventos
- Genera empleados de prueba con datos aleatorios
- Publica eventos `EmployeeCreatedEvent` a RabbitMQ
- Utiliza routing keys específicas para eventos de empleados

### 📨 Suscripción de Eventos  
- Se suscribe a eventos de creación de empleados (`memos.employee.created`)
- Procesa eventos de empleados de forma asíncrona
- Simula flujos de trabajo reales (emails, notificaciones, actualizaciones de sistemas)

## Configuración

La aplicación se conecta a RabbitMQ con la siguiente configuración:

```csharp
Host: 172.16.10.12
Port: 5672
Usuario: owner
Exchange: ruway.events
```

## Uso

1. **Ejecutar la aplicación:**
   ```bash
   dotnet run --project EventSubscriberConsoleApp
   ```

2. **Comandos disponibles:**
   - `create` - Crea y publica un evento de empleado de prueba
   - `help` - Muestra la ayuda de comandos
   - `exit` - Sale de la aplicación

## Estructura del Evento

El evento `EmployeeCreatedEvent` contiene:

- `EmployeeId`: ID único del empleado
- `FirstName`: Nombre del empleado
- `LastName`: Apellido del empleado  
- `DocumentNumber`: Número de documento
- `Email`: Correo electrónico
- `Phone`: Teléfono de contacto

## Flujo de Procesamiento

Cuando se recibe un evento de empleado creado, la aplicación:

1. ✅ Envía email de bienvenida
2. ✅ Crea cuenta de usuario
3. ✅ Notifica a Recursos Humanos
4. ✅ Actualiza directorio corporativo

## Routing Keys

- **Publicación**: `memos.employee.created`
- **Suscripción**: 
  - `memos.employee.created` (eventos tipados)
  - `employee.events.*` (logging genérico)