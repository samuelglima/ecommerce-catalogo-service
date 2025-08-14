using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Catalogo.Application.Commands;
using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Application.Queries;
using Microsoft.AspNetCore.Http;

namespace Catalogo.API.Controllers
{
    /// <summary>
    /// Controller responsável pelas operações de Produtos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutosController(IProdutoService produtoService)
        {
            _produtoService = produtoService ?? throw new ArgumentNullException(nameof(produtoService));
        }

        /// <summary>
        /// Obtém todos os produtos
        /// </summary>
        /// <param name="categoria">Filtrar por categoria (opcional)</param>
        /// <param name="apenasAtivos">Retornar apenas produtos ativos (padrão: true)</param>
        /// <param name="apenasDisponiveis">Retornar apenas produtos disponíveis</param>
        /// <param name="pagina">Número da página (padrão: 1)</param>
        /// <param name="itensPorPagina">Itens por página (padrão: 10)</param>
        /// <returns>Lista de produtos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProdutoResumoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProdutoResumoDto>>> ObterTodos(
            [FromQuery] string categoria = null,
            [FromQuery] bool? apenasAtivos = true,
            [FromQuery] bool? apenasDisponiveis = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 10)
        {
            var query = new ListarProdutosQuery
            {
                Categoria = categoria,
                ApenasAtivos = apenasAtivos,
                ApenasDisponiveis = apenasDisponiveis,
                Pagina = pagina,
                ItensPorPagina = itensPorPagina
            };

            var produtos = await _produtoService.ListarProdutosAsync(query);
            return Ok(produtos);
        }

        /// <summary>
        /// Obtém um produto por ID
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>Dados do produto</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProdutoDto>> ObterPorId(Guid id)
        {
            var query = new ObterProdutoPorIdQuery(id);
            var produto = await _produtoService.ObterPorIdAsync(query);

            if (produto == null)
            {
                return NotFound(new { mensagem = $"Produto com ID {id} não encontrado" });
            }

            return Ok(produto);
        }

        /// <summary>
        /// Obtém um produto por SKU
        /// </summary>
        /// <param name="sku">SKU do produto</param>
        /// <returns>Dados do produto</returns>
        [HttpGet("sku/{sku}")]
        [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProdutoDto>> ObterPorSku(string sku)
        {
            var produto = await _produtoService.ObterPorSkuAsync(sku);

            if (produto == null)
            {
                return NotFound(new { mensagem = $"Produto com SKU {sku} não encontrado" });
            }

            return Ok(produto);
        }

        /// <summary>
        /// Obtém produtos por categoria
        /// </summary>
        /// <param name="categoria">Nome da categoria</param>
        /// <returns>Lista de produtos da categoria</returns>
        [HttpGet("categoria/{categoria}")]
        [ProducesResponseType(typeof(IEnumerable<ProdutoResumoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProdutoResumoDto>>> ObterPorCategoria(string categoria)
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync(categoria);
            return Ok(produtos);
        }

        /// <summary>
        /// Cria um novo produto
        /// </summary>
        /// <param name="command">Dados do produto a ser criado</param>
        /// <returns>Produto criado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProdutoDto>> Criar([FromBody] CriarProdutoCommand command)
        {
            try
            {
                var produto = await _produtoService.CriarProdutoAsync(command);
                return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza um produto existente
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <param name="command">Dados para atualização</param>
        /// <returns>Produto atualizado</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProdutoDto>> Atualizar(Guid id, [FromBody] AtualizarProdutoCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { mensagem = "ID da rota não corresponde ao ID do comando" });
            }

            try
            {
                var produto = await _produtoService.AtualizarProdutoAsync(command);
                return Ok(produto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Altera o preço de um produto
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <param name="command">Novo preço</param>
        /// <returns>Produto com preço atualizado</returns>
        [HttpPatch("{id:guid}/preco")]
        [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProdutoDto>> AlterarPreco(Guid id, [FromBody] AlterarPrecoProdutoCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { mensagem = "ID da rota não corresponde ao ID do comando" });
            }

            try
            {
                var produto = await _produtoService.AlterarPrecoAsync(command);
                return Ok(produto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza o estoque de um produto
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <param name="command">Dados da operação de estoque</param>
        /// <returns>Produto com estoque atualizado</returns>
        [HttpPatch("{id:guid}/estoque")]
        [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProdutoDto>> AtualizarEstoque(Guid id, [FromBody] AtualizarEstoqueCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { mensagem = "ID da rota não corresponde ao ID do comando" });
            }

            try
            {
                var produto = await _produtoService.AtualizarEstoqueAsync(command);
                return Ok(produto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Ativa um produto
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>Status da operação</returns>
        [HttpPatch("{id:guid}/ativar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Ativar(Guid id)
        {
            var sucesso = await _produtoService.AtivarProdutoAsync(id);
            
            if (!sucesso)
            {
                return NotFound(new { mensagem = $"Produto com ID {id} não encontrado" });
            }

            return NoContent();
        }

        /// <summary>
        /// Desativa um produto
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>Status da operação</returns>
        [HttpPatch("{id:guid}/desativar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(Guid id)
        {
            var sucesso = await _produtoService.DesativarProdutoAsync(id);
            
            if (!sucesso)
            {
                return NotFound(new { mensagem = $"Produto com ID {id} não encontrado" });
            }

            return NoContent();
        }

        /// <summary>
        /// Deleta um produto
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>Status da operação</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deletar(Guid id)
        {
            var sucesso = await _produtoService.DeletarProdutoAsync(id);
            
            if (!sucesso)
            {
                return NotFound(new { mensagem = $"Produto com ID {id} não encontrado" });
            }

            return NoContent();
        }
    }
}