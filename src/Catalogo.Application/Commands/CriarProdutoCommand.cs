namespace Catalogo.Application.Commands
{
    /// <summary>
    /// Command para criar um novo produto
    /// </summary>
    public class CriarProdutoCommand : Command
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
        public string Sku { get; set; }
        public string Categoria { get; set; }
        public string ImagemUrl { get; set; }

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