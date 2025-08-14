using System;

namespace Catalogo.Application.Queries
{
    /// <summary>
    /// Query para obter um produto por ID
    /// </summary>
    public class ObterProdutoPorIdQuery
    {
        public Guid Id { get; set; }

        public ObterProdutoPorIdQuery(Guid id)
        {
            Id = id;
        }
    }
}