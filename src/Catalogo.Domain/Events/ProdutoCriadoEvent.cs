using System;

namespace Catalogo.Domain.Events
{
    /// <summary>
    /// Evento disparado quando um produto é criado
    /// </summary>
    public class ProdutoCriadoEvent : DomainEvent
    {
        public Guid ProdutoId { get; set; }
        public string Nome { get; set; }
        public string Sku { get; set; }
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
        public string Categoria { get; set; }

        public ProdutoCriadoEvent(Guid produtoId, string nome, string sku, 
            decimal preco, int quantidadeEstoque, string categoria)
        {
            ProdutoId = produtoId;
            Nome = nome;
            Sku = sku;
            Preco = preco;
            QuantidadeEstoque = quantidadeEstoque;
            Categoria = categoria;
        }
    }
}