using System;

namespace Catalogo.Application.Commands
{
    /// <summary>
    /// Command para atualizar um produto existente
    /// </summary>
    public class AtualizarProdutoCommand : Command
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }

        public AtualizarProdutoCommand(Guid id, string nome, string descricao, string categoria)
        {
            Id = id;
            Nome = nome;
            Descricao = descricao;
            Categoria = categoria;
        }
    }
}