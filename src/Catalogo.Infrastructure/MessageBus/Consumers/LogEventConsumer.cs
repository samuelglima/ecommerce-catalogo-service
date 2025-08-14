using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Catalogo.Domain.Events;

namespace Catalogo.Infrastructure.MessageBus.Consumers
{
    /// <summary>
    /// Consumer para fazer log de eventos (exemplo)
    /// </summary>
    public class LogEventConsumer : 
        IConsumer<ProdutoCriadoEvent>,
        IConsumer<EstoqueAtualizadoEvent>,
        IConsumer<PrecoAlteradoEvent>,
        IConsumer<ProdutoDesativadoEvent>
    {
        private readonly ILogger<LogEventConsumer> _logger;

        public LogEventConsumer(ILogger<LogEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<ProdutoCriadoEvent> context)
        {
            var evento = context.Message;
            _logger.LogInformation(
                "📦 EVENTO RECEBIDO - Produto Criado: {Nome} (SKU: {Sku}) - Preço: R$ {Preco} - Estoque: {Estoque}",
                evento.Nome, evento.Sku, evento.Preco, evento.QuantidadeEstoque);
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<EstoqueAtualizadoEvent> context)
        {
            var evento = context.Message;
            _logger.LogInformation(
                "📊 EVENTO RECEBIDO - Estoque Atualizado: SKU {Sku} - De {Anterior} para {Atual} ({Operacao})",
                evento.Sku, evento.QuantidadeAnterior, evento.QuantidadeAtual, evento.TipoOperacao);
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<PrecoAlteradoEvent> context)
        {
            var evento = context.Message;
            _logger.LogInformation(
                "💰 EVENTO RECEBIDO - Preço Alterado: SKU {Sku} - De R$ {Anterior} para R$ {Novo} ({Percentual:F2}%)",
                evento.Sku, evento.PrecoAnterior, evento.PrecoNovo, evento.PercentualAlteracao);
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<ProdutoDesativadoEvent> context)
        {
            var evento = context.Message;
            _logger.LogInformation(
                "🚫 EVENTO RECEBIDO - Produto Desativado: {Nome} (SKU: {Sku}) - Motivo: {Motivo}",
                evento.Nome, evento.Sku, evento.Motivo);
            
            return Task.CompletedTask;
        }
    }
}