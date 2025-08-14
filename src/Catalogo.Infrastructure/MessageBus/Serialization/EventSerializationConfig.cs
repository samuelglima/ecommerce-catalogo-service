using System.Text.Json;
using System.Text.Json.Serialization;

namespace Catalogo.Infrastructure.MessageBus.Serialization
{
    public static class EventSerializationConfig
    {
        public static JsonSerializerOptions GetJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());
            
            return options;
        }
    }
}