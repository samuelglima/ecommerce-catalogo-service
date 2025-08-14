using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Events;
using Bogus;

namespace Catalogo.Domain.Tests.Entities
{
    public class ProdutoTests
    {
        private readonly Faker _faker = new Faker("pt_BR");

        [Fact]
        public void Produto_CriarComDadosValidos_DeveCriarCorretamente()
        {
            // Arrange
            var nome = _faker.Commerce.ProductName();
            var descricao = _faker.Commerce.ProductDescription();
            var preco = _faker.Random.Decimal(10, 1000);
            var quantidade = _faker.Random.Int(1, 100);
            var sku = _faker.Random.AlphaNumeric(10).ToUpper();
            var categoria = _faker.Commerce.Categories(1)[0];

            // Act
            var produto = new Produto(nome, descricao, preco, quantidade, sku, categoria);

            // Assert
            produto.Should().NotBeNull();
            produto.Nome.Should().Be(nome);
            produto.Descricao.Should().Be(descricao);
            produto.Preco.Valor.Should().Be(preco);
            produto.QuantidadeEstoque.Should().Be(quantidade);
            produto.Sku.Should().Be(sku.ToUpperInvariant());
            produto.Categoria.Should().Be(categoria);
            produto.Ativo.Should().BeTrue();
            produto.Id.Should().NotBeEmpty();
            produto.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Produto_CriarNovoProduto_DeveGerarEventoProdutoCriado()
        {
            // Arrange & Act
            var produto = new Produto(
                "Produto Teste",
                "Descrição Teste",
                100m,
                10,
                "SKU-001",
                "Categoria Teste"
            );

            // Assert
            produto.DomainEvents.Should().NotBeNull();
            produto.DomainEvents.Should().HaveCount(1);
            
            var evento = produto.DomainEvents.First() as ProdutoCriadoEvent;
            evento.Should().NotBeNull();
            evento.ProdutoId.Should().Be(produto.Id);
            evento.Nome.Should().Be(produto.Nome);
            evento.Sku.Should().Be(produto.Sku);
            evento.Preco.Should().Be(produto.Preco.Valor);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("AB")] // Menos de 3 caracteres
        public void Produto_CriarComNomeInvalido_DeveLancarExcecao(string nome)
        {
            // Arrange & Act
            Action act = () => new Produto(nome, "Descrição", 100m, 10, "SKU001", "Categoria");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Produto_CriarComNomeMuitoLongo_DeveLancarExcecao()
        {
            // Arrange
            var nomeLongo = new string('A', 201);

            // Act
            Action act = () => new Produto(nomeLongo, "Descrição", 100m, 10, "SKU001", "Categoria");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*não pode ter mais de 200 caracteres*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Produto_CriarComDescricaoInvalida_DeveLancarExcecao(string descricao)
        {
            // Arrange & Act
            Action act = () => new Produto("Produto", descricao, 100m, 10, "SKU001", "Categoria");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Produto_CriarComPrecoNegativo_DeveLancarExcecao()
        {
            // Arrange & Act
            Action act = () => new Produto("Produto", "Descrição", -10m, 10, "SKU001", "Categoria");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Produto_CriarComEstoqueNegativo_DeveLancarExcecao()
        {
            // Arrange & Act
            Action act = () => new Produto("Produto", "Descrição", 100m, -5, "SKU001", "Categoria");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*não pode ser negativa*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("AB")] // Menos de 3 caracteres
        public void Produto_CriarComSkuInvalido_DeveLancarExcecao(string sku)
        {
            // Arrange & Act
            Action act = () => new Produto("Produto", "Descrição", 100m, 10, sku, "Categoria");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Produto_AlterarPreco_DeveAtualizarCorretamente()
        {
            // Arrange
            var produto = CriarProdutoValido();
            var novoPreco = 200m;
            var precoAnterior = produto.Preco.Valor;

            // Act
            produto.AlterarPreco(novoPreco);

            // Assert
            produto.Preco.Valor.Should().Be(novoPreco);
            produto.DataAtualizacao.Should().NotBeNull();
            produto.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Produto_AlterarPreco_DeveGerarEventoPrecoAlterado()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.ClearDomainEvents(); // Limpar evento de criação
            var precoAnterior = produto.Preco.Valor;
            var novoPreco = 200m;

            // Act
            produto.AlterarPreco(novoPreco);

            // Assert
            produto.DomainEvents.Should().HaveCount(1);
            var evento = produto.DomainEvents.First() as PrecoAlteradoEvent;
            evento.Should().NotBeNull();
            evento.PrecoAnterior.Should().Be(precoAnterior);
            evento.PrecoNovo.Should().Be(novoPreco);
        }

        [Fact]
        public void Produto_AlterarPrecoParaZeroOuNegativo_DeveLancarExcecao()
        {
            // Arrange
            var produto = CriarProdutoValido();

            // Act
            Action act1 = () => produto.AlterarPreco(0);
            Action act2 = () => produto.AlterarPreco(-10);

            // Assert
            act1.Should().Throw<ArgumentException>();
            act2.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Produto_AdicionarEstoque_DeveIncrementarCorretamente()
        {
            // Arrange
            var produto = CriarProdutoValido();
            var estoqueInicial = produto.QuantidadeEstoque;
            var quantidadeAdicionar = 50;

            // Act
            produto.AdicionarEstoque(quantidadeAdicionar);

            // Assert
            produto.QuantidadeEstoque.Should().Be(estoqueInicial + quantidadeAdicionar);
        }

        [Fact]
        public void Produto_AdicionarEstoque_DeveGerarEventoEstoqueAtualizado()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.ClearDomainEvents();
            var estoqueAnterior = produto.QuantidadeEstoque;

            // Act
            produto.AdicionarEstoque(10);

            // Assert
            produto.DomainEvents.Should().HaveCount(1);
            var evento = produto.DomainEvents.First() as EstoqueAtualizadoEvent;
            evento.Should().NotBeNull();
            evento.QuantidadeAnterior.Should().Be(estoqueAnterior);
            evento.QuantidadeAtual.Should().Be(estoqueAnterior + 10);
            evento.TipoOperacao.Should().Be("Adicao");
        }

        [Fact]
        public void Produto_RemoverEstoque_DeveDecrementarCorretamente()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.AdicionarEstoque(100);
            var estoqueInicial = produto.QuantidadeEstoque;
            var quantidadeRemover = 30;

            // Act
            produto.RemoverEstoque(quantidadeRemover);

            // Assert
            produto.QuantidadeEstoque.Should().Be(estoqueInicial - quantidadeRemover);
        }

        [Fact]
        public void Produto_RemoverEstoqueAcimaDoDisponivel_DeveLancarExcecao()
        {
            // Arrange
            var produto = CriarProdutoValido();
            var quantidadeRemover = produto.QuantidadeEstoque + 1;

            // Act
            Action act = () => produto.RemoverEstoque(quantidadeRemover);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Estoque insuficiente*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Produto_AdicionarOuRemoverQuantidadeInvalida_DeveLancarExcecao(int quantidade)
        {
            // Arrange
            var produto = CriarProdutoValido();

            // Act
            Action actAdicionar = () => produto.AdicionarEstoque(quantidade);
            Action actRemover = () => produto.RemoverEstoque(quantidade);

            // Assert
            actAdicionar.Should().Throw<ArgumentException>();
            actRemover.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Produto_Desativar_DeveMarcarComoInativo()
        {
            // Arrange
            var produto = CriarProdutoValido();

            // Act
            produto.Desativar();

            // Assert
            produto.Ativo.Should().BeFalse();
            produto.DataAtualizacao.Should().NotBeNull();
        }

        [Fact]
        public void Produto_Desativar_DeveGerarEventoProdutoDesativado()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.ClearDomainEvents();

            // Act
            produto.Desativar();

            // Assert
            produto.DomainEvents.Should().HaveCount(1);
            var evento = produto.DomainEvents.First() as ProdutoDesativadoEvent;
            evento.Should().NotBeNull();
            evento.ProdutoId.Should().Be(produto.Id);
            evento.Sku.Should().Be(produto.Sku);
        }

        [Fact]
        public void Produto_Ativar_DeveMarcarComoAtivo()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.Desativar();

            // Act
            produto.Ativar();

            // Assert
            produto.Ativo.Should().BeTrue();
        }

        [Fact]
        public void Produto_EstaDisponivel_DeveRetornarTrueQuandoAtivoComEstoque()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.AdicionarEstoque(10);

            // Act
            var disponivel = produto.EstaDisponivel();

            // Assert
            disponivel.Should().BeTrue();
        }

        [Fact]
        public void Produto_EstaDisponivel_DeveRetornarFalseQuandoInativo()
        {
            // Arrange
            var produto = CriarProdutoValido();
            produto.AdicionarEstoque(10);
            produto.Desativar();

            // Act
            var disponivel = produto.EstaDisponivel();

            // Assert
            disponivel.Should().BeFalse();
        }

        [Fact]
        public void Produto_EstaDisponivel_DeveRetornarFalseQuandoSemEstoque()
        {
            // Arrange
            var produto = new Produto("Produto", "Descrição", 100m, 0, "SKU001", "Categoria");

            // Act
            var disponivel = produto.EstaDisponivel();

            // Assert
            disponivel.Should().BeFalse();
        }

        [Fact]
        public void Produto_AtualizarInformacoes_DeveAtualizarCorretamente()
        {
            // Arrange
            var produto = CriarProdutoValido();
            var novoNome = "Novo Nome";
            var novaDescricao = "Nova Descrição";
            var novaCategoria = "Nova Categoria";

            // Act
            produto.AtualizarInformacoes(novoNome, novaDescricao, novaCategoria);

            // Assert
            produto.Nome.Should().Be(novoNome);
            produto.Descricao.Should().Be(novaDescricao);
            produto.Categoria.Should().Be(novaCategoria);
            produto.DataAtualizacao.Should().NotBeNull();
        }

        [Fact]
        public void Produto_DefinirImagem_DeveAtualizarUrl()
        {
            // Arrange
            var produto = CriarProdutoValido();
            var imagemUrl = "https://exemplo.com/imagem.jpg";

            // Act
            produto.DefinirImagem(imagemUrl);

            // Assert
            produto.ImagemUrl.Should().Be(imagemUrl);
            produto.DataAtualizacao.Should().NotBeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Produto_DefinirImagemInvalida_DeveLancarExcecao(string imagemUrl)
        {
            // Arrange
            var produto = CriarProdutoValido();

            // Act
            Action act = () => produto.DefinirImagem(imagemUrl);

            // Assert
            act.Should().Throw<ArgumentException>();
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
    }
}