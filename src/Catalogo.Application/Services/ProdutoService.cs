using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalogo.Application.Commands;
using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Application.Queries;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Interfaces;

namespace Catalogo.Application.Services
{
    /// <summary>
    /// Implementação do serviço de aplicação para Produtos
    /// </summary>
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoService(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
        }

        // Commands
        public async Task<ProdutoDto> CriarProdutoAsync(CriarProdutoCommand command)
        {
            // Verificar se já existe um produto com o mesmo SKU
            if (await _produtoRepository.ExisteSkuAsync(command.Sku))
            {
                throw new InvalidOperationException($"Já existe um produto com o SKU {command.Sku}");
            }

            // Criar a entidade
            var produto = new Produto(
                command.Nome,
                command.Descricao,
                command.Preco,
                command.QuantidadeEstoque,
                command.Sku,
                command.Categoria
            );

            // Adicionar imagem se fornecida
            if (!string.IsNullOrWhiteSpace(command.ImagemUrl))
            {
                produto.DefinirImagem(command.ImagemUrl);
            }

            // Salvar no repositório
            var produtoSalvo = await _produtoRepository.AdicionarAsync(produto);

            // Retornar DTO
            return MapearParaDto(produtoSalvo);
        }

        public async Task<ProdutoDto> AtualizarProdutoAsync(AtualizarProdutoCommand command)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(command.Id);
            
            if (produto == null)
            {
                throw new InvalidOperationException($"Produto com ID {command.Id} não encontrado");
            }

            // Atualizar informações
            produto.AtualizarInformacoes(command.Nome, command.Descricao, command.Categoria);

            // Salvar alterações
            await _produtoRepository.AtualizarAsync(produto);

            return MapearParaDto(produto);
        }

        public async Task<ProdutoDto> AlterarPrecoAsync(AlterarPrecoProdutoCommand command)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(command.Id);
            
            if (produto == null)
            {
                throw new InvalidOperationException($"Produto com ID {command.Id} não encontrado");
            }

            produto.AlterarPreco(command.NovoPreco);
            await _produtoRepository.AtualizarAsync(produto);

            return MapearParaDto(produto);
        }

        public async Task<ProdutoDto> AtualizarEstoqueAsync(AtualizarEstoqueCommand command)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(command.Id);
            
            if (produto == null)
            {
                throw new InvalidOperationException($"Produto com ID {command.Id} não encontrado");
            }

            if (command.TipoOperacao == TipoOperacaoEstoque.Adicionar)
            {
                produto.AdicionarEstoque(command.Quantidade);
            }
            else
            {
                produto.RemoverEstoque(command.Quantidade);
            }

            await _produtoRepository.AtualizarAsync(produto);

            return MapearParaDto(produto);
        }

        public async Task<bool> DeletarProdutoAsync(Guid id)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(id);
            
            if (produto == null)
            {
                return false;
            }

            await _produtoRepository.RemoverAsync(produto);
            return true;
        }

        public async Task<bool> AtivarProdutoAsync(Guid id)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(id);
            
            if (produto == null)
            {
                return false;
            }

            produto.Ativar();
            await _produtoRepository.AtualizarAsync(produto);
            return true;
        }

        public async Task<bool> DesativarProdutoAsync(Guid id)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(id);
            
            if (produto == null)
            {
                return false;
            }

            produto.Desativar();
            await _produtoRepository.AtualizarAsync(produto);
            return true;
        }

        // Queries
        public async Task<ProdutoDto> ObterPorIdAsync(ObterProdutoPorIdQuery query)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(query.Id);
            
            if (produto == null)
            {
                return null;
            }

            return MapearParaDto(produto);
        }

        public async Task<IEnumerable<ProdutoResumoDto>> ListarProdutosAsync(ListarProdutosQuery query)
        {
            IEnumerable<Produto> produtos;

            if (query.ApenasAtivos.HasValue && query.ApenasAtivos.Value)
            {
                produtos = await _produtoRepository.ObterProdutosAtivosAsync();
            }
            else
            {
                produtos = await _produtoRepository.ObterTodosAsync();
            }

            // Filtrar por categoria se especificado
            if (!string.IsNullOrWhiteSpace(query.Categoria))
            {
                produtos = produtos.Where(p => p.Categoria.Equals(query.Categoria, StringComparison.OrdinalIgnoreCase));
            }

            // Filtrar por disponibilidade se especificado
            if (query.ApenasDisponiveis.HasValue && query.ApenasDisponiveis.Value)
            {
                produtos = produtos.Where(p => p.EstaDisponivel());
            }

            // Aplicar paginação
            var produtosList = produtos.ToList();
            var skip = ((query.Pagina ?? 1) - 1) * (query.ItensPorPagina ?? 10);
            var take = query.ItensPorPagina ?? 10;

            var produtosPaginados = produtosList.Skip(skip).Take(take);

            return produtosPaginados.Select(MapearParaResumoDto);
        }

        public async Task<IEnumerable<ProdutoResumoDto>> ObterPorCategoriaAsync(string categoria)
        {
            var produtos = await _produtoRepository.ObterPorCategoriaAsync(categoria);
            return produtos.Select(MapearParaResumoDto);
        }

        public async Task<ProdutoDto> ObterPorSkuAsync(string sku)
        {
            var produto = await _produtoRepository.ObterPorSkuAsync(sku);
            
            if (produto == null)
            {
                return null;
            }

            return MapearParaDto(produto);
        }

        // Métodos auxiliares de mapeamento
        private ProdutoDto MapearParaDto(Produto produto)
        {
            if (produto == null) return null;

            return new ProdutoDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                Preco = produto.Preco.Valor,
                Moeda = produto.Preco.Moeda,
                QuantidadeEstoque = produto.QuantidadeEstoque,
                Ativo = produto.Ativo,
                Sku = produto.Sku,
                Categoria = produto.Categoria,
                ImagemUrl = produto.ImagemUrl,
                DataCriacao = produto.DataCriacao,
                DataAtualizacao = produto.DataAtualizacao,
                Disponivel = produto.EstaDisponivel()
            };
        }

        private ProdutoResumoDto MapearParaResumoDto(Produto produto)
        {
            if (produto == null) return null;

            return new ProdutoResumoDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Preco = produto.Preco.Valor,
                Categoria = produto.Categoria,
                ImagemUrl = produto.ImagemUrl,
                Disponivel = produto.EstaDisponivel(),
                Sku = produto.Sku
            };
        }
    }
}