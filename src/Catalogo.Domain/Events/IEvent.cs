using System;

namespace Catalogo.Domain.Events
{
    /// <summary>
    /// Interface base para todos os eventos
    /// </summary>
    public interface IEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }
}