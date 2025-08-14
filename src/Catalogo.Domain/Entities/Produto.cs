using System;
using Catalogo.Domain.Interfaces;
using Catalogo.Domain.ValueObjects;

namespace Catalogo.Domain.Entities
{
    /// <summary>
    /// Entidade Produto - Aggregate Root do contexto de Catálogo
    /// </summary>
    public class Produto : Entity, IAggregateRoot
    {
        /// <summary>
        /// Nome do produto
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Descrição detalhada do produto
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// Preço do produto
        /// </summary>
        public Dinheiro Preco { get; private set; }

        /// <summary>
        /// Quantidade em estoque
        /// </summary>
        public int QuantidadeEstoque { get; private set; }

        /// <summary>
        /// Indica se o produto está ativo
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// SKU (Stock Keeping Unit) - Código único do produto
        /// </summary>
        public string Sku { get; private set; }

        /// <summary>
        /// Categoria do produto
        /// </summary>
        public string Categoria { get; private set; }

        /// <summary>
        /// URL da imagem do produto
        /// </summary>
        public string ImagemUrl { get; private set; }

        // Construtor protegido para o EF Core
        protected Produto() { }

        /// <summary>
        /// Cria um novo produto
        /// </summary>
        public Produto(string nome, string descricao, decimal preco, int quantidadeEstoque, string sku, string categoria)
        {
            ValidarNome(nome);
            ValidarDescricao(descricao);
            ValidarSku(sku);
            ValidarCategoria(categoria);
            ValidarQuantidadeEstoque(quantidadeEstoque);

            Nome = nome;
            Descricao = descricao;
            Preco = new Dinheiro(preco);
            QuantidadeEstoque = quantidadeEstoque;
            Sku = sku.ToUpperInvariant();
            Categoria = categoria;
            Ativo = true;
        }

        /// <summary>
        /// Atualiza as informações básicas do produto
        /// </summary>
        public void AtualizarInformacoes(string nome, string descricao, string categoria)
        {
            ValidarNome(nome);
            ValidarDescricao(descricao);
            ValidarCategoria(categoria);

            Nome = nome;
            Descricao = descricao;
            Categoria = categoria;

            MarcarComoAtualizado();
        }

        /// <summary>
        /// Altera o preço do produto
        /// </summary>
        public void AlterarPreco(decimal novoPreco)
        {
            if (novoPreco <= 0)
                throw new ArgumentException("O preço deve ser maior que zero", nameof(novoPreco));

            Preco = new Dinheiro(novoPreco);
            MarcarComoAtualizado();
        }

        /// <summary>
        /// Adiciona quantidade ao estoque
        /// </summary>
        public void AdicionarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade deve ser maior que zero", nameof(quantidade));

            QuantidadeEstoque += quantidade;
            MarcarComoAtualizado();
        }

        /// <summary>
        /// Remove quantidade do estoque
        /// </summary>
        public void RemoverEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade deve ser maior que zero", nameof(quantidade));

            if (QuantidadeEstoque < quantidade)
                throw new InvalidOperationException($"Estoque insuficiente. Disponível: {QuantidadeEstoque}, Solicitado: {quantidade}");

            QuantidadeEstoque -= quantidade;
            MarcarComoAtualizado();
        }

        /// <summary>
        /// Ativa o produto
        /// </summary>
        public void Ativar()
        {
            Ativo = true;
            MarcarComoAtualizado();
        }

        /// <summary>
        /// Desativa o produto
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
            MarcarComoAtualizado();
        }

        /// <summary>
        /// Define a URL da imagem do produto
        /// </summary>
        public void DefinirImagem(string imagemUrl)
        {
            if (string.IsNullOrWhiteSpace(imagemUrl))
                throw new ArgumentException("A URL da imagem não pode ser vazia", nameof(imagemUrl));

            ImagemUrl = imagemUrl;
            MarcarComoAtualizado();
        }

        /// <summary>
        /// Verifica se o produto está disponível para venda
        /// </summary>
        public bool EstaDisponivel()
        {
            return Ativo && QuantidadeEstoque > 0;
        }

        // Métodos de validação privados
        private void ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do produto não pode ser vazio", nameof(nome));

            if (nome.Length < 3)
                throw new ArgumentException("O nome do produto deve ter pelo menos 3 caracteres", nameof(nome));

            if (nome.Length > 200)
                throw new ArgumentException("O nome do produto não pode ter mais de 200 caracteres", nameof(nome));
        }

        private void ValidarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("A descrição do produto não pode ser vazia", nameof(descricao));

            if (descricao.Length > 1000)
                throw new ArgumentException("A descrição não pode ter mais de 1000 caracteres", nameof(descricao));
        }

        private void ValidarSku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("O SKU não pode ser vazio", nameof(sku));

            if (sku.Length < 3 || sku.Length > 50)
                throw new ArgumentException("O SKU deve ter entre 3 e 50 caracteres", nameof(sku));
        }

        private void ValidarCategoria(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                throw new ArgumentException("A categoria não pode ser vazia", nameof(categoria));
        }

        private void ValidarQuantidadeEstoque(int quantidade)
        {
            if (quantidade < 0)
                throw new ArgumentException("A quantidade em estoque não pode ser negativa", nameof(quantidade));
        }
    }
}