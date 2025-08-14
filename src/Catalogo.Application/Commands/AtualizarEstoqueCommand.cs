using System;

namespace Catalogo.Application.Commands
{
    /// <summary>
    /// Command para atualizar o estoque de um produto
    /// </summary>
    public class AtualizarEstoqueCommand : Command
    {
        public Guid Id { get; set; }
        public int Quantidade { get; set; }
        public TipoOperacaoEstoque TipoOperacao { get; set; }

        public AtualizarEstoqueCommand(Guid id, int quantidade, TipoOperacaoEstoque tipoOperacao)
        {
            Id = id;
            Quantidade = quantidade;
            TipoOperacao = tipoOperacao;
        }
    }

    public enum TipoOperacaoEstoque
    {
        Adicionar,
        Remover
    }
}