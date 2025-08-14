using System;

namespace Catalogo.Domain.Events
{
    /// <summary>
    /// Classe base para eventos de domínio
    /// </summary>
    public abstract class DomainEvent : IEvent
    {
        public Guid EventId { get; protected set; }
        public DateTime OccurredOn { get; protected set; }
        public string EventType { get; protected set; }

        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            EventType = GetType().Name;
        }
    }
}