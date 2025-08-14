using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Catalogo.Domain.Events;

namespace Catalogo.Infrastructure.MessageBus
{
    /// <summary>
    /// Implementação do Event Bus usando MassTransit
    /// </summary>
    public class MassTransitEventBus : IEventBus
    {
        private readonly IBus _bus;
        private readonly ILogger<MassTransitEventBus> _logger;

        public MassTransitEventBus(IBus bus, ILogger<MassTransitEventBus> logger)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync<T>(T @event) where T : class, IEvent
        {
            try
            {
                _logger.LogInformation("Publicando evento {EventType} com ID {EventId}", 
                    @event.GetType().Name, @event.EventId);

                // Publicar usando o Bus em vez do PublishEndpoint
                await _bus.Publish(@event, @event.GetType());

                _logger.LogInformation("Evento {EventType} publicado com sucesso", 
                    @event.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento {EventType}", @event.GetType().Name);
                throw;
            }
        }
    }
}