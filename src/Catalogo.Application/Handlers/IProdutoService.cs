using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalogo.Application.Commands;
using Catalogo.Application.DTOs;
using Catalogo.Application.Queries;

namespace Catalogo.Application.Interfaces
{
    /// <summary>
    /// Interface do serviço de aplicação para Produtos
    /// </summary>
    public interface IProdutoService
    {
        // Commands
        Task<ProdutoDto> CriarProdutoAsync(CriarProdutoCommand command);
        Task<ProdutoDto> AtualizarProdutoAsync(AtualizarProdutoCommand command);
        Task<ProdutoDto> AlterarPrecoAsync(AlterarPrecoProdutoCommand command);
        Task<ProdutoDto> AtualizarEstoqueAsync(AtualizarEstoqueCommand command);
        Task<bool> DeletarProdutoAsync(Guid id);
        Task<bool> AtivarProdutoAsync(Guid id);
        Task<bool> DesativarProdutoAsync(Guid id);

        // Queries
        Task<ProdutoDto> ObterPorIdAsync(ObterProdutoPorIdQuery query);
        Task<IEnumerable<ProdutoResumoDto>> ListarProdutosAsync(ListarProdutosQuery query);
        Task<IEnumerable<ProdutoResumoDto>> ObterPorCategoriaAsync(string categoria);
        Task<ProdutoDto> ObterPorSkuAsync(string sku);
    }
}