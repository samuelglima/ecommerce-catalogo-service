using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Catalogo.Infrastructure.MessageBus;
using Catalogo.Infrastructure.MessageBus.Consumers;

namespace Catalogo.API.Configurations
{
    /// <summary>
    /// Configuração do RabbitMQ e MassTransit
    /// </summary>
    public static class RabbitMQConfig
    {
        public static IServiceCollection AddRabbitMQConfiguration(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Configurar MassTransit
            services.AddMassTransit(x =>
            {
                // Registrar o Consumer
                x.AddConsumer<LogEventConsumer>();

                // Configurar RabbitMQ
                x.UsingRabbitMq((context, cfg) =>
                {
                    // Configurações do RabbitMQ
                    var rabbitMQHost = configuration["RabbitMQ:Host"] ?? "localhost";
                    var rabbitMQUser = configuration["RabbitMQ:Username"] ?? "admin";
                    var rabbitMQPass = configuration["RabbitMQ:Password"] ?? "admin123";
                    var rabbitMQVHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                    cfg.Host(rabbitMQHost, rabbitMQVHost, h =>
                    {
                        h.Username(rabbitMQUser);
                        h.Password(rabbitMQPass);
                    });



                    // Configurar Exchange para o Catálogo
                    cfg.Message<Domain.Events.ProdutoCriadoEvent>(x =>
                    {
                        x.SetEntityName("catalogo.produto-criado");
                    });

                    cfg.Message<Domain.Events.EstoqueAtualizadoEvent>(x =>
                    {
                        x.SetEntityName("catalogo.estoque-atualizado");
                    });

                    cfg.Message<Domain.Events.PrecoAlteradoEvent>(x =>
                    {
                        x.SetEntityName("catalogo.preco-alterado");
                    });

                    cfg.Message<Domain.Events.ProdutoDesativadoEvent>(x =>
                    {
                        x.SetEntityName("catalogo.produto-desativado");
                    });

                    // Configurar endpoint para o Consumer
                    cfg.ReceiveEndpoint("catalogo-log-queue", e =>
                    {
                        e.ConfigureConsumer<LogEventConsumer>(context);

                        // Configurar as bindings
                        e.Bind<Domain.Events.ProdutoCriadoEvent>();
                        e.Bind<Domain.Events.EstoqueAtualizadoEvent>();
                        e.Bind<Domain.Events.PrecoAlteradoEvent>();
                        e.Bind<Domain.Events.ProdutoDesativadoEvent>();
                    });

                    // Configurar serialização
                    cfg.ConfigureJsonSerializerOptions(options =>
                    {
                        options.WriteIndented = true;
                        return options;
                    });

                    // Configurar retry
                    cfg.UseMessageRetry(retry =>
                    {
                        retry.Interval(3, TimeSpan.FromSeconds(5));
                    });

                    

                    cfg.ConfigureEndpoints(context);
                });

                // Configurar timeout
                //x.SetRequestTimeout(TimeSpan.FromSeconds(30));
                
            });

           // No final do método, alterar de IPublishEndpoint para IBus
            // Registrar o Event Bus
            services.AddScoped<IEventBus, MassTransitEventBus>();

            // Adicionar isso para garantir que o IBus está disponível
            services.AddScoped<IBus>(provider => provider.GetRequiredService<IBusControl>());

            return services;
        }
    }
}