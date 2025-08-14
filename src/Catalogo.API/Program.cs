using Catalogo.API.Configurations;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Configurar serviços
builder.Services.AddControllers(options =>
    {
        // Desabilitar validação automática de nullable reference types
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        // Configurar serialização JSON
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        
        // Adicionar conversor para enums
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Adicionar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

// Adicionar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Adicionar RabbitMQ e MassTransit
builder.Services.AddRabbitMQConfiguration(builder.Configuration);

// Adicionar Injeção de Dependência
builder.Services.AddDependencyInjection();

// Adicionar HealthChecks
builder.Services.AddHealthChecks()
    .AddRabbitMQ(   sp => {
            var factory = new ConnectionFactory() { Uri = new Uri("amqp://admin:admin123@localhost:5672/") };
            return factory.CreateConnectionAsync();
        },
        name: "rabbitmq",
        tags: new[] { "message-broker" });

var app = builder.Build();

// Configurar pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Usar Swagger
app.UseSwaggerConfiguration();

// Usar CORS
app.UseCors("AllowAll");

// Usar HTTPS Redirection
app.UseHttpsRedirection();

// Usar Authorization
app.UseAuthorization();

// Mapear controllers
app.MapControllers();

// Mapear HealthChecks
app.MapHealthChecks("/health");

// Adicionar rota de boas-vindas
app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

// Logging de inicialização
app.Logger.LogInformation("🚀 Catálogo API iniciada com sucesso!");
app.Logger.LogInformation($"📍 Ambiente: {app.Environment.EnvironmentName}");
app.Logger.LogInformation("📚 Documentação disponível em: /swagger");
app.Logger.LogInformation("💚 Health check disponível em: /health");
app.Logger.LogInformation("🐰 RabbitMQ configurado e conectado");

app.Run();