namespace Catalogo.Domain.Interfaces
{
    /// <summary>
    /// Marca uma entidade como Aggregate Root
    /// Aggregate Root é o ponto de entrada para um agregado no DDD
    /// </summary>
    public interface IAggregateRoot
    {
        // Interface marcadora (Marker Interface)
        // Não tem métodos, apenas identifica que a entidade é um Aggregate Root
    }
}