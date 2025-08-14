using System;

namespace Catalogo.Domain.Events
{
    /// <summary>
    /// Evento disparado quando o estoque é atualizado
    /// </summary>
    public class EstoqueAtualizadoEvent : DomainEvent
    {
        public Guid ProdutoId { get; set; }
        public string Sku { get; set; }
        public int QuantidadeAnterior { get; set; }
        public int QuantidadeAtual { get; set; }
        public string TipoOperacao { get; set; }

        public EstoqueAtualizadoEvent(Guid produtoId, string sku, 
            int quantidadeAnterior, int quantidadeAtual, string tipoOperacao)
        {
            ProdutoId = produtoId;
            Sku = sku;
            QuantidadeAnterior = quantidadeAnterior;
            QuantidadeAtual = quantidadeAtual;
            TipoOperacao = tipoOperacao;
        }
    }
}