namespace Catalogo.Application.Queries
{
    /// <summary>
    /// Query para listar produtos com filtros opcionais
    /// </summary>
    public class ListarProdutosQuery
    {
        public string Categoria { get; set; }
        public bool? ApenasAtivos { get; set; }
        public bool? ApenasDisponiveis { get; set; }
        public int? Pagina { get; set; }
        public int? ItensPorPagina { get; set; }

        public ListarProdutosQuery()
        {
            Pagina = 1;
            ItensPorPagina = 10;
            ApenasAtivos = true;
        }
    }
}