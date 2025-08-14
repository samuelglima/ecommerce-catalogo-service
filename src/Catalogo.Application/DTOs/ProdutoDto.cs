using System;

namespace Catalogo.Application.DTOs
{
    /// <summary>
    /// DTO para representar um Produto nas respostas da API
    /// </summary>
    public class ProdutoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public string Moeda { get; set; }
        public int QuantidadeEstoque { get; set; }
        public bool Ativo { get; set; }
        public string Sku { get; set; }
        public string Categoria { get; set; }
        public string ImagemUrl { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public bool Disponivel { get; set; }
    }
}