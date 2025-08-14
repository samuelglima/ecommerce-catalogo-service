using System.Collections.Generic;
using System.Threading.Tasks;
using Catalogo.Domain.Entities;

namespace Catalogo.Domain.Interfaces
{
    /// <summary>
    /// Repositório específico para Produtos
    /// </summary>
    public interface IProdutoRepository : IRepository<Produto>
    {
        /// <summary>
        /// Busca um produto pelo SKU
        /// </summary>
        Task<Produto> ObterPorSkuAsync(string sku);

        /// <summary>
        /// Busca produtos por categoria
        /// </summary>
        Task<IEnumerable<Produto>> ObterPorCategoriaAsync(string categoria);

        /// <summary>
        /// Busca produtos ativos
        /// </summary>
        Task<IEnumerable<Produto>> ObterProdutosAtivosAsync();

        /// <summary>
        /// Busca produtos com estoque baixo
        /// </summary>
        Task<IEnumerable<Produto>> ObterProdutosComEstoqueBaixoAsync(int quantidadeMinima);

        /// <summary>
        /// Verifica se existe um produto com o SKU informado
        /// </summary>
        Task<bool> ExisteSkuAsync(string sku);
    }
}