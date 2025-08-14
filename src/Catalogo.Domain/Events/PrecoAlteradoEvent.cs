using System;

namespace Catalogo.Domain.Events
{
    /// <summary>
    /// Evento disparado quando o preço é alterado
    /// </summary>
    public class PrecoAlteradoEvent : DomainEvent
    {
        public Guid ProdutoId { get; set; }
        public string Sku { get; set; }
        public decimal PrecoAnterior { get; set; }
        public decimal PrecoNovo { get; set; }
        public decimal PercentualAlteracao { get; set; }

        public PrecoAlteradoEvent(Guid produtoId, string sku, 
            decimal precoAnterior, decimal precoNovo)
        {
            ProdutoId = produtoId;
            Sku = sku;
            PrecoAnterior = precoAnterior;
            PrecoNovo = precoNovo;
            
            if (precoAnterior > 0)
            {
                PercentualAlteracao = ((precoNovo - precoAnterior) / precoAnterior) * 100;
            }
        }
    }
}