using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Interfaces;

namespace Catalogo.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação do repositório de Produtos em memória
    /// </summary>
    public class ProdutoMemoryRepository : MemoryRepository<Produto>, IProdutoRepository
    {
        public ProdutoMemoryRepository() : base()
        {
            // Adicionar alguns produtos de exemplo
            InicializarDadosExemplo();
        }

        public Task<Produto> ObterPorSkuAsync(string sku)
        {
            var produto = _entities.FirstOrDefault(p => 
                p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(produto);
        }

        public Task<IEnumerable<Produto>> ObterPorCategoriaAsync(string categoria)
        {
            var produtos = _entities.Where(p => 
                p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(produtos.AsEnumerable());
        }

        public Task<IEnumerable<Produto>> ObterProdutosAtivosAsync()
        {
            var produtos = _entities.Where(p => p.Ativo);
            return Task.FromResult(produtos.AsEnumerable());
        }

        public Task<IEnumerable<Produto>> ObterProdutosComEstoqueBaixoAsync(int quantidadeMinima)
        {
            var produtos = _entities.Where(p => p.QuantidadeEstoque < quantidadeMinima);
            return Task.FromResult(produtos.AsEnumerable());
        }

        public Task<bool> ExisteSkuAsync(string sku)
        {
            var existe = _entities.Any(p => 
                p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(existe);
        }

        private void InicializarDadosExemplo()
        {
            var produtos = new List<Produto>
            {
                new Produto(
                    "Notebook Dell Inspiron",
                    "Notebook com processador Intel Core i7, 16GB RAM, 512GB SSD",
                    3599.90m,
                    10,
                    "NOTE-DELL-001",
                    "Eletrônicos"
                ),
                new Produto(
                    "Mouse Logitech MX Master",
                    "Mouse sem fio com tecnologia Bluetooth e USB",
                    299.90m,
                    25,
                    "MOUSE-LOG-001",
                    "Periféricos"
                ),
                new Produto(
                    "Teclado Mecânico Razer",
                    "Teclado mecânico RGB com switches tácteis",
                    799.90m,
                    15,
                    "TECL-RAZ-001",
                    "Periféricos"
                ),
                new Produto(
                    "Monitor LG UltraWide",
                    "Monitor 29 polegadas UltraWide Full HD",
                    1499.90m,
                    5,
                    "MON-LG-001",
                    "Monitores"
                ),
                new Produto(
                    "Cadeira Gamer",
                    "Cadeira ergonômica para gamers com apoio lombar",
                    899.90m,
                    8,
                    "CAD-GAME-001",
                    "Móveis"
                )
            };

            // Adicionar imagens aos produtos
            produtos[0].DefinirImagem("https://example.com/notebook.jpg");
            produtos[1].DefinirImagem("https://example.com/mouse.jpg");
            produtos[2].DefinirImagem("https://example.com/teclado.jpg");
            produtos[3].DefinirImagem("https://example.com/monitor.jpg");
            produtos[4].DefinirImagem("https://example.com/cadeira.jpg");

            _entities.AddRange(produtos);
        }
    }
}