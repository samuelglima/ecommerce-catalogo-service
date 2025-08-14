using Microsoft.Extensions.DependencyInjection;
using Catalogo.Application.Interfaces;
using Catalogo.Application.Services;
using Catalogo.Domain.Interfaces;
using Catalogo.Infrastructure.Repositories;

namespace Catalogo.API.Configurations
{
    /// <summary>
    /// Configuração de Injeção de Dependência
    /// </summary>
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            // Registrar Repositories
            services.AddSingleton<IProdutoRepository, ProdutoMemoryRepository>();

            // Registrar Services
            services.AddScoped<IProdutoService, ProdutoService>();

            // O Event Bus já é registrado no RabbitMQConfig

            return services;
        }
    }
}