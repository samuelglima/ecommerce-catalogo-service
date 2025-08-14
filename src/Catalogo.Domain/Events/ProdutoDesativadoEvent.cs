using System;

namespace Catalogo.Domain.Events
{
    /// <summary>
    /// Evento disparado quando um produto é desativado
    /// </summary>
    public class ProdutoDesativadoEvent : DomainEvent
    {
        public Guid ProdutoId { get; set; }
        public string Sku { get; set; }
        public string Nome { get; set; }
        public string Motivo { get; set; }

        public ProdutoDesativadoEvent(Guid produtoId, string sku, string nome, string motivo = null)
        {
            ProdutoId = produtoId;
            Sku = sku;
            Nome = nome;
            Motivo = motivo ?? "Desativado pelo administrador";
        }
    }
}