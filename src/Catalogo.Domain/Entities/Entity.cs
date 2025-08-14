using System;

namespace Catalogo.Domain.Entities
{
    /// <summary>
    /// Classe base para todas as entidades do domínio
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Data de criação da entidade
        /// </summary>
        public DateTime DataCriacao { get; protected set; }

        /// <summary>
        /// Data da última atualização
        /// </summary>
        public DateTime? DataAtualizacao { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.UtcNow;
        }

        /// <summary>
        /// Verifica se duas entidades são iguais comparando seus IDs
        /// </summary>
        public override bool Equals(object obj)
        {
            var compareTo = obj as Entity;

            if (ReferenceEquals(this, compareTo)) return true;
            if (ReferenceEquals(null, compareTo)) return false;

            return Id.Equals(compareTo.Id);
        }

        /// <summary>
        /// Gera o HashCode baseado no ID
        /// </summary>
        public override int GetHashCode()
        {
            return (GetType().GetHashCode() * 907) + Id.GetHashCode();
        }

        /// <summary>
        /// Operador de igualdade
        /// </summary>
        public static bool operator ==(Entity a, Entity b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        /// <summary>
        /// Operador de desigualdade
        /// </summary>
        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Retorna string representando a entidade
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name} [Id={Id}]";
        }

        /// <summary>
        /// Marca a entidade como atualizada
        /// </summary>
        protected void MarcarComoAtualizado()
        {
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}