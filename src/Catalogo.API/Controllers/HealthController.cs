using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Catalogo.Infrastructure.MessageBus;
using Catalogo.Domain.Events;

namespace Catalogo.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IEventBus _eventBus;

        public HealthController(ILogger<HealthController> logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        [HttpPost("test-event")]
        public async Task<IActionResult> TestEvent()
        {
            try
            {
                _logger.LogInformation("Iniciando teste de evento...");

                var testEvent = new ProdutoCriadoEvent(
                    Guid.NewGuid(),
                    "Produto Teste",
                    "TEST-001",
                    99.99m,
                    10,
                    "Teste"
                );

                await _eventBus.PublishAsync(testEvent);

                _logger.LogInformation("Evento de teste publicado com sucesso!");

                return Ok(new { 
                    message = "Evento publicado com sucesso", 
                    eventId = testEvent.EventId,
                    timestamp = testEvent.OccurredOn 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento de teste");
                return StatusCode(500, new { 
                    error = "Erro ao publicar evento", 
                    details = ex.Message 
                });
            }
        }

        [HttpGet("rabbitmq-status")]
        public IActionResult GetRabbitMQStatus()
        {
            return Ok(new
            {
                status = "Verificar RabbitMQ Management",
                managementUrl = "http://localhost:15672",
                credentials = "admin / admin123"
            });
        }
    }
}