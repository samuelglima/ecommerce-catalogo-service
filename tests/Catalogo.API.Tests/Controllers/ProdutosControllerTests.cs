using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Catalogo.API.Controllers;
using Catalogo.Application.Interfaces;
using Catalogo.Application.Commands;
using Catalogo.Application.DTOs;
using Catalogo.Application.Queries;

namespace Catalogo.API.Tests.Controllers
{
    public class ProdutosControllerTests
    {
        private readonly Mock<IProdutoService> _mockService;
        private readonly ProdutosController _controller;

        public ProdutosControllerTests()
        {
            _mockService = new Mock<IProdutoService>();
            _controller = new ProdutosController(_mockService.Object);
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarOkComListaDeProdutos()
        {
            // Arrange
            var produtos = new List<ProdutoResumoDto>
            {
                new ProdutoResumoDto { Id = Guid.NewGuid(), Nome = "Produto 1" },
                new ProdutoResumoDto { Id = Guid.NewGuid(), Nome = "Produto 2" }
            };

            _mockService
                .Setup(x => x.ListarProdutosAsync(It.IsAny<ListarProdutosQuery>()))
                .ReturnsAsync(produtos);

            // Act
            var resultado = await _controller.ObterTodos();

            // Assert
            var okResult = resultado.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var retorno = okResult.Value as IEnumerable<ProdutoResumoDto>;
            retorno.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObterPorId_ComIdExistente_DeveRetornarOkComProduto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var produto = new ProdutoDto 
            { 
                Id = id, 
                Nome = "Produto Teste",
                Preco = 100m
            };

            _mockService
                .Setup(x => x.ObterPorIdAsync(It.IsAny<ObterProdutoPorIdQuery>()))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _controller.ObterPorId(id);

            // Assert
            var okResult = resultado.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var retorno = okResult.Value as ProdutoDto;
            retorno.Should().NotBeNull();
            retorno.Id.Should().Be(id);
        }

        [Fact]
        public async Task ObterPorId_ComIdInexistente_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockService
                .Setup(x => x.ObterPorIdAsync(It.IsAny<ObterProdutoPorIdQuery>()))
                .ReturnsAsync((ProdutoDto)null);

            // Act
            var resultado = await _controller.ObterPorId(id);

            // Assert
            var notFoundResult = resultado.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Criar_ComDadosValidos_DeveRetornarCreated()
        {
            // Arrange
            var command = new CriarProdutoCommand(
                "Novo Produto",
                "Descrição",
                100m,
                10,
                "SKU-001",
                "Categoria"
            );

            var produtoCriado = new ProdutoDto
            {
                Id = Guid.NewGuid(),
                Nome = command.Nome,
                Sku = command.Sku
            };

            _mockService
                .Setup(x => x.CriarProdutoAsync(It.IsAny<CriarProdutoCommand>()))
                .ReturnsAsync(produtoCriado);

            // Act
            var resultado = await _controller.Criar(command);

            // Assert
            var createdResult = resultado.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(ProdutosController.ObterPorId));
        }

        [Fact]
        public async Task Criar_ComDadosInvalidos_DeveRetornarBadRequest()
        {
            // Arrange
            var command = new CriarProdutoCommand(
                "Produto",
                "Descrição",
                100m,
                10,
                "SKU-EXISTE",
                "Categoria"
            );

            _mockService
                .Setup(x => x.CriarProdutoAsync(It.IsAny<CriarProdutoCommand>()))
                .ThrowsAsync(new InvalidOperationException("SKU já existe"));

            // Act
            var resultado = await _controller.Criar(command);

            // Assert
            var badRequestResult = resultado.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Atualizar_ComDadosValidos_DeveRetornarOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new AtualizarProdutoCommand(
                id,
                "Nome Atualizado",
                "Descrição Atualizada",
                "Categoria"
            );

            var produtoAtualizado = new ProdutoDto
            {
                Id = id,
                Nome = command.Nome
            };

            _mockService
                .Setup(x => x.AtualizarProdutoAsync(It.IsAny<AtualizarProdutoCommand>()))
                .ReturnsAsync(produtoAtualizado);

            // Act
            var resultado = await _controller.Atualizar(id, command);

            // Assert
            var okResult = resultado.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Atualizar_ComIdsDiferentes_DeveRetornarBadRequest()
        {
            // Arrange
            var idRota = Guid.NewGuid();
            var idCommand = Guid.NewGuid();
            var command = new AtualizarProdutoCommand(
                idCommand,
                "Nome",
                "Descrição",
                "Categoria"
            );

            // Act
            var resultado = await _controller.Atualizar(idRota, command);

            // Assert
            var badRequestResult = resultado.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task AlterarPreco_ComDadosValidos_DeveRetornarOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new AlterarPrecoProdutoCommand(id, 199.99m);

            var produtoAtualizado = new ProdutoDto
            {
                Id = id,
                Preco = command.NovoPreco
            };

            _mockService
                .Setup(x => x.AlterarPrecoAsync(It.IsAny<AlterarPrecoProdutoCommand>()))
                .ReturnsAsync(produtoAtualizado);

            // Act
            var resultado = await _controller.AlterarPreco(id, command);

            // Assert
            var okResult = resultado.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task AtualizarEstoque_Adicionar_DeveRetornarOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new AtualizarEstoqueCommand(id, 50, TipoOperacaoEstoque.Adicionar);

            var produtoAtualizado = new ProdutoDto
            {
                Id = id,
                QuantidadeEstoque = 150
            };

            _mockService
                .Setup(x => x.AtualizarEstoqueAsync(It.IsAny<AtualizarEstoqueCommand>()))
                .ReturnsAsync(produtoAtualizado);

            // Act
            var resultado = await _controller.AtualizarEstoque(id, command);

            // Assert
            var okResult = resultado.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Ativar_ComIdExistente_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockService
                .Setup(x => x.AtivarProdutoAsync(id))
                .ReturnsAsync(true);

            // Act
            var resultado = await _controller.Ativar(id);

            // Assert
            var noContentResult = resultado as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task Desativar_ComIdExistente_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockService
                .Setup(x => x.DesativarProdutoAsync(id))
                .ReturnsAsync(true);

            // Act
            var resultado = await _controller.Desativar(id);

            // Assert
            var noContentResult = resultado as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task Deletar_ComIdExistente_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockService
                .Setup(x => x.DeletarProdutoAsync(id))
                .ReturnsAsync(true);

            // Act
            var resultado = await _controller.Deletar(id);

            // Assert
            var noContentResult = resultado as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task Deletar_ComIdInexistente_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockService
                .Setup(x => x.DeletarProdutoAsync(id))
                .ReturnsAsync(false);

            // Act
            var resultado = await _controller.Deletar(id);

            // Assert
            var notFoundResult = resultado as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task ObterPorSku_ComSkuExistente_DeveRetornarOk()
        {
            // Arrange
            var sku = "SKU-TEST-001";
            var produto = new ProdutoDto
            {
                Id = Guid.NewGuid(),
                Sku = sku,
                Nome = "Produto Teste"
            };

            _mockService
                .Setup(x => x.ObterPorSkuAsync(sku))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _controller.ObterPorSku(sku);

            // Assert
            var okResult = resultado.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var retorno = okResult.Value as ProdutoDto;
            retorno.Should().NotBeNull();
            retorno.Sku.Should().Be(sku);
        }

        [Fact]
        public async Task ObterPorSku_ComSkuInexistente_DeveRetornarNotFound()
        {
            // Arrange
            var sku = "SKU-NAO-EXISTE";

            _mockService
                .Setup(x => x.ObterPorSkuAsync(sku))
                .ReturnsAsync((ProdutoDto)null);

            // Act
            var resultado = await _controller.ObterPorSku(sku);

            // Assert
            var notFoundResult = resultado.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task ObterPorCategoria_DeveRetornarOkComProdutos()
        {
            // Arrange
            var categoria = "Eletrônicos";
            var produtos = new List<ProdutoResumoDto>
            {
                new ProdutoResumoDto { Id = Guid.NewGuid(), Nome = "Produto 1", Categoria = categoria },
                new ProdutoResumoDto { Id = Guid.NewGuid(), Nome = "Produto 2", Categoria = categoria }
            };

            _mockService
                .Setup(x => x.ObterPorCategoriaAsync(categoria))
                .ReturnsAsync(produtos);

            // Act
            var resultado = await _controller.ObterPorCategoria(categoria);

            // Assert
            var okResult = resultado.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            var retorno = okResult.Value as IEnumerable<ProdutoResumoDto>;
            retorno.Should().HaveCount(2);
            retorno.Should().OnlyContain(p => p.Categoria == categoria);
        }
    }
}