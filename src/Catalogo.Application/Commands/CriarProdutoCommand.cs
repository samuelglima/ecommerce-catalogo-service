using System.ComponentModel.DataAnnotations;

namespace Catalogo.Application.Commands
{
    /// <summary>
    /// Command para criar um novo produto
    /// </summary>
    public class CriarProdutoCommand : Command
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MinLength(3, ErrorMessage = "O nome deve ter pelo menos 3 caracteres")]
        [MaxLength(200, ErrorMessage = "O nome não pode ter mais de 200 caracteres")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [MaxLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A quantidade em estoque é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade não pode ser negativa")]
        public int QuantidadeEstoque { get; set; }

        [Required(ErrorMessage = "O SKU é obrigatório")]
        [MinLength(3, ErrorMessage = "O SKU deve ter pelo menos 3 caracteres")]
        [MaxLength(50, ErrorMessage = "O SKU não pode ter mais de 50 caracteres")]
        public string Sku { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public string Categoria { get; set; }

        // ImagemUrl é OPCIONAL - sem Required
        public string ImagemUrl { get; set; }

        public CriarProdutoCommand()
        {
            // Construtor vazio para binding
        }

        public CriarProdutoCommand(string nome, string descricao, decimal preco, 
            int quantidadeEstoque, string sku, string categoria)
        {
            Nome = nome;
            Descricao = descricao;
            Preco = preco;
            QuantidadeEstoque = quantidadeEstoque;
            Sku = sku;
            Categoria = categoria;
        }
    }
}