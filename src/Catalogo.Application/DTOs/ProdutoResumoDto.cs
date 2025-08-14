using System;

namespace Catalogo.Application.DTOs
{
    /// <summary>
    /// DTO resumido para listagens de produtos
    /// </summary>
    public class ProdutoResumoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public string Categoria { get; set; }
        public string ImagemUrl { get; set; }
        public bool Disponivel { get; set; }
        public string Sku { get; set; }
    }
}