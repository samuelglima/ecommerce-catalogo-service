using System;

namespace Catalogo.Application.Commands
{
    /// <summary>
    /// Command para alterar o preço de um produto
    /// </summary>
    public class AlterarPrecoProdutoCommand : Command
    {
        public Guid Id { get; set; }
        public decimal NovoPreco { get; set; }

        public AlterarPrecoProdutoCommand(Guid id, decimal novoPreco)
        {
            Id = id;
            NovoPreco = novoPreco;
        }
    }
}