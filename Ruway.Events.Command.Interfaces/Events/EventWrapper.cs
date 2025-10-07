using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruway.Events.Command.Interfaces.Events
{
    public class EventWrapper
    {
       
        public Guid EventId { get; set; }

   
        public string EventName { get; set; } = string.Empty;

        public int Version { get; set; }


        public DateTime OccurredOn { get; set; }

      
        public IDomainEvent Data { get; set; } = null!;
    }

    /// <summary>
    /// Wrapper genérico para eventos tipados
    /// </summary>
    /// <typeparam name="T">Tipo del evento</typeparam>
    public class EventWrapper<T> where T : IDomainEvent
    {
      
        public Guid EventId { get; set; }


        public string EventName { get; set; } = string.Empty;


        public int Version { get; set; }


        public DateTime OccurredOn { get; set; }

        
        public T Data { get; set; } = default!;
    }
}