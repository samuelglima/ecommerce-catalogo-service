using System.Threading.Tasks;
using Catalogo.Domain.Events;

namespace Catalogo.Infrastructure.MessageBus
{
    /// <summary>
    /// Interface para publicação de eventos
    /// </summary>
    public interface IEventBus
    {
        Task PublishAsync<T>(T @event) where T : class, IEvent;
    }
}