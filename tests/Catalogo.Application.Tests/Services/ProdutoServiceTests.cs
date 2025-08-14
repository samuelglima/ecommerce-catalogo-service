using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Catalogo.Application.Services;
using Catalogo.Application.Commands;
using Catalogo.Application.Queries;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Interfaces;
using Catalogo.Infrastructure.MessageBus;
using Catalogo.Domain.Events;
using Bogus;

namespace Catalogo.Application.Tests.Services
{
    public class ProdutoServiceTests
    {
        private readonly Mock<IProdutoRepository> _mockRepository;
        private readonly Mock<IEventBus> _mockEventBus;
        private readonly Mock<ILogger<ProdutoService>> _mockLogger;
        private readonly ProdutoService _service;
        private readonly Faker _faker;

        public ProdutoServiceTests()
        {
            _mockRepository = new Mock<IProdutoRepository>();
            _mockEventBus = new Mock<IEventBus>();
            _mockLogger = new Mock<ILogger<ProdutoService>>();
            _service = new ProdutoService(
                _mockRepository.Object,
                _mockEventBus.Object,
                _mockLogger.Object
            );
            _faker = new Faker("pt_BR");
        }

        [Fact]
        public async Task CriarProduto_ComDadosValidos_DeveCriarComSucesso()
        {
            // Arrange
            var command = new CriarProdutoCommand(
                _faker.Commerce.ProductName(),
                _faker.Commerce.ProductDescription(),
                _faker.Random.Decimal(10, 1000),
                _faker.Random.Int(1, 100),
                _faker.Random.AlphaNumeric(10).ToUpper(),
                _faker.Commerce.Categories(1)[0]
            );

            _mockRepository
                .Setup(x => x.ExisteSkuAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockRepository
                .Setup(x => x.AdicionarAsync(It.IsAny<Produto>()))
                .ReturnsAsync((Produto p) => p);

            // Act
            var resultado = await _service.CriarProdutoAsync(command);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be(command.Nome);
            resultado.Sku.Should().Be(command.Sku);
            resultado.Preco.Should().Be(command.Preco);

            _mockRepository.Verify(x => x.ExisteSkuAsync(command.Sku), Times.Once);
            _mockRepository.Verify(x => x.AdicionarAsync(It.IsAny<Produto>()), Times.Once);
            
            // CORREÇÃO: Verificar IEvent em vez do tipo específico
            _mockEventBus.Verify(x => x.PublishAsync(
                It.Is<IEvent>(e => e is ProdutoCriadoEvent)), 
                Times.Once);
        }

        [Fact]
        public async Task CriarProduto_ComSkuDuplicado_DeveLancarExcecao()
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

            _mockRepository
                .Setup(x => x.ExisteSkuAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _service.CriarProdutoAsync(command);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{command.Sku}*");

            _mockRepository.Verify(x => x.AdicionarAsync(It.IsAny<Produto>()), Times.Never);
            _mockEventBus.Verify(x => x.PublishAsync(It.IsAny<IEvent>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarProduto_ComProdutoExistente_DeveAtualizarComSucesso()
        {
            // Arrange
            var produto = CriarProdutoValido();
            var command = new AtualizarProdutoCommand(
                produto.Id,
                "Nome Atualizado",
                "Descrição Atualizada",
                "Categoria Atualizada"
            );

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(produto.Id))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _service.AtualizarProdutoAsync(command);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be(command.Nome);
            resultado.Descricao.Should().Be(command.Descricao);
            resultado.Categoria.Should().Be(command.Categoria);

            _mockRepository.Verify(x => x.AtualizarAsync(It.IsAny<Produto>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarProduto_ComProdutoInexistente_DeveLancarExcecao()
        {
            // Arrange
            var command = new AtualizarProdutoCommand(
                Guid.NewGuid(),
                "Nome",
                "Descrição",
                "Categoria"
            );

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Produto)null);

            // Act
            Func<Task> act = async () => await _service.AtualizarProdutoAsync(command);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{command.Id}*");

            _mockRepository.Verify(x => x.AtualizarAsync(It.IsAny<Produto>()), Times.Never);
        }

        [Fact]
        public async Task AlterarPreco_ComProdutoExistente_DeveAlterarComSucesso()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.ClearDomainEvents(); // Limpar evento de criação
            var precoAnterior = produto.Preco.Valor;
            var command = new AlterarPrecoProdutoCommand(produto.Id, 299.99m);

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(produto.Id))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _service.AlterarPrecoAsync(command);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Preco.Should().Be(command.NovoPreco);

            _mockRepository.Verify(x => x.AtualizarAsync(It.IsAny<Produto>()), Times.Once);
            
            // CORREÇÃO: Verificar IEvent que é PrecoAlteradoEvent
            _mockEventBus.Verify(x => x.PublishAsync(
                It.Is<IEvent>(e => e is PrecoAlteradoEvent)), 
                Times.Once);
        }

        [Fact]
        public async Task AtualizarEstoque_Adicionar_DeveIncrementarEstoque()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.ClearDomainEvents(); // Limpar evento de criação
            var estoqueInicial = produto.QuantidadeEstoque;
            var command = new AtualizarEstoqueCommand(
                produto.Id, 
                50, 
                TipoOperacaoEstoque.Adicionar
            );

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(produto.Id))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _service.AtualizarEstoqueAsync(command);

            // Assert
            resultado.Should().NotBeNull();
            resultado.QuantidadeEstoque.Should().Be(estoqueInicial + 50);

            _mockRepository.Verify(x => x.AtualizarAsync(It.IsAny<Produto>()), Times.Once);
            
            // CORREÇÃO: Verificar IEvent que é EstoqueAtualizadoEvent
            _mockEventBus.Verify(x => x.PublishAsync(
                It.Is<IEvent>(e => e is EstoqueAtualizadoEvent)), 
                Times.Once);
        }

        [Fact]
        public async Task AtualizarEstoque_Remover_DeveDecrementarEstoque()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.AdicionarEstoque(100); // Garantir estoque suficiente
            produto.ClearDomainEvents(); // Limpar eventos anteriores
            var estoqueInicial = produto.QuantidadeEstoque;
            var command = new AtualizarEstoqueCommand(
                produto.Id, 
                30, 
                TipoOperacaoEstoque.Remover
            );

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(produto.Id))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _service.AtualizarEstoqueAsync(command);

            // Assert
            resultado.Should().NotBeNull();
            resultado.QuantidadeEstoque.Should().Be(estoqueInicial - 30);

            _mockRepository.Verify(x => x.AtualizarAsync(It.IsAny<Produto>()), Times.Once);
            
            // CORREÇÃO: Verificar IEvent que é EstoqueAtualizadoEvent
            _mockEventBus.Verify(x => x.PublishAsync(
                It.Is<IEvent>(e => e is EstoqueAtualizadoEvent)), 
                Times.Once);
        }

        [Fact]
        public async Task AtualizarEstoque_RemoverMaisQueDisponivel_DeveLancarExcecao()
        {
            // Arrange
            var produto = CriarProdutoValido();
            var command = new AtualizarEstoqueCommand(
                produto.Id, 
                produto.QuantidadeEstoque + 100, 
                TipoOperacaoEstoque.Remover
            );

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(produto.Id))
                .ReturnsAsync(produto);

            // Act
            Func<Task> act = async () => await _service.AtualizarEstoqueAsync(command);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Estoque insuficiente*");

            _mockRepository.Verify(x => x.AtualizarAsync(It.IsAny<Produto>()), Times.Never);
        }

        [Fact]
        public async Task DeletarProduto_ComProdutoExistente_DeveDeletarComSucesso()
        {
            // Arrange
            var produto = CriarProdutoValido();

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(produto.Id))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _service.DeletarProdutoAsync(produto.Id);

            // Assert
            resultado.Should().BeTrue();
            _mockRepository.Verify(x => x.RemoverAsync(produto), Times.Once);
        }

        [Fact]
        public async Task DeletarProduto_ComProdutoInexistente_DeveRetornarFalse()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(id))
                .ReturnsAsync((Produto)null);

            // Act
            var resultado = await _service.DeletarProdutoAsync(id);

            // Assert
            resultado.Should().BeFalse();
            _mockRepository.Verify(x => x.RemoverAsync(It.IsAny<Produto>()), Times.Never);
        }

        [Fact]
        public async Task DesativarProduto_ComProdutoExistente_DeveDesativarComSucesso()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.ClearDomainEvents(); // Limpar evento de criação

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(produto.Id))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _service.DesativarProdutoAsync(produto.Id);

            // Assert
            resultado.Should().BeTrue();
            produto.Ativo.Should().BeFalse();

            _mockRepository.Verify(x => x.AtualizarAsync(It.IsAny<Produto>()), Times.Once);
            
            // CORREÇÃO: Verificar IEvent que é ProdutoDesativadoEvent
            _mockEventBus.Verify(x => x.PublishAsync(
                It.Is<IEvent>(e => e is ProdutoDesativadoEvent)), 
                Times.Once);
        }

        [Fact]
        public async Task ObterPorId_ComProdutoExistente_DeveRetornarProduto()
        {
            // Arrange
            var produto = CriarProdutoValido();
            var query = new ObterProdutoPorIdQuery(produto.Id);

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(produto.Id))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _service.ObterPorIdAsync(query);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(produto.Id);
            resultado.Nome.Should().Be(produto.Nome);
        }

        [Fact]
        public async Task ObterPorId_ComProdutoInexistente_DeveRetornarNull()
        {
            // Arrange
            var query = new ObterProdutoPorIdQuery(Guid.NewGuid());

            _mockRepository
                .Setup(x => x.ObterPorIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Produto)null);

            // Act
            var resultado = await _service.ObterPorIdAsync(query);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ListarProdutos_ComFiltroCategoria_DeveRetornarProdutosFiltrados()
        {
            // Arrange
            var produtos = new List<Produto>
            {
                CriarProdutoComCategoria("Eletrônicos"),
                CriarProdutoComCategoria("Eletrônicos"),
                CriarProdutoComCategoria("Livros"),
                CriarProdutoComCategoria("Roupas")
            };

            var query = new ListarProdutosQuery
            {
                Categoria = "Eletrônicos",
                ApenasAtivos = true
            };

            _mockRepository
                .Setup(x => x.ObterProdutosAtivosAsync())
                .ReturnsAsync(produtos);

            // Act
            var resultado = await _service.ListarProdutosAsync(query);

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().OnlyContain(p => p.Categoria == "Eletrônicos");
        }

        [Fact]
        public async Task ListarProdutos_ComPaginacao_DeveRetornarPaginaCorreta()
        {
            // Arrange
            var produtos = Enumerable.Range(1, 25)
                .Select(i => CriarProdutoValido())
                .ToList();

            var query = new ListarProdutosQuery
            {
                Pagina = 2,
                ItensPorPagina = 10,
                ApenasAtivos = false
            };

            _mockRepository
                .Setup(x => x.ObterTodosAsync())
                .ReturnsAsync(produtos);

            // Act
            var resultado = await _service.ListarProdutosAsync(query);

            // Assert
            resultado.Should().HaveCount(10);
        }

        [Fact]
        public async Task ObterPorSku_ComSkuExistente_DeveRetornarProduto()
        {
            // Arrange
            var produto = CriarProdutoValido();
            var sku = produto.Sku;

            _mockRepository
                .Setup(x => x.ObterPorSkuAsync(sku))
                .ReturnsAsync(produto);

            // Act
            var resultado = await _service.ObterPorSkuAsync(sku);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Sku.Should().Be(sku);
        }

        [Fact]
        public async Task ObterPorCategoria_DeveRetornarProdutosDaCategoria()
        {
            // Arrange
            var categoria = "Eletrônicos";
            var produtos = new List<Produto>
            {
                CriarProdutoComCategoria(categoria),
                CriarProdutoComCategoria(categoria)
            };

            _mockRepository
                .Setup(x => x.ObterPorCategoriaAsync(categoria))
                .ReturnsAsync(produtos);

            // Act
            var resultado = await _service.ObterPorCategoriaAsync(categoria);

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().OnlyContain(p => p.Categoria == categoria);
        }

        // NOVO TESTE: Verificar múltiplos eventos
        [Fact]
        public async Task CriarProdutoCompleto_DevePublicarTodosEventos()
        {
            // Arrange
            var command = new CriarProdutoCommand(
                "Produto Completo",
                "Descrição",
                100m,
                10,
                "SKU-COMPLETO",
                "Categoria"
            );

            _mockRepository
                .Setup(x => x.ExisteSkuAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockRepository
                .Setup(x => x.AdicionarAsync(It.IsAny<Produto>()))
                .ReturnsAsync((Produto p) => p);

            // Setup genérico para aceitar qualquer IEvent
            _mockEventBus
                .Setup(x => x.PublishAsync(It.IsAny<IEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.CriarProdutoAsync(command);

            // Assert
            resultado.Should().NotBeNull();
            
            // Verificar que PublishAsync foi chamado pelo menos uma vez
            _mockEventBus.Verify(x => x.PublishAsync(It.IsAny<IEvent>()), Times.AtLeastOnce);
        }

        private Produto CriarProdutoValido()
        {
            return new Produto(
                _faker.Commerce.ProductName(),
                _faker.Commerce.ProductDescription(),
                _faker.Random.Decimal(10, 1000),
                _faker.Random.Int(1, 100),
                _faker.Random.AlphaNumeric(10).ToUpper(),
                _faker.Commerce.Categories(1)[0]
            );
        }

        private Produto CriarProdutoComCategoria(string categoria)
        {
            return new Produto(
                _faker.Commerce.ProductName(),
                _faker.Commerce.ProductDescription(),
                _faker.Random.Decimal(10, 1000),
                _faker.Random.Int(1, 100),
                _faker.Random.AlphaNumeric(10).ToUpper(),
                categoria
            );
        }
    }
}