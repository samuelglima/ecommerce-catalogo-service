using System;
using System.Collections.Generic;
using System.Linq;

namespace Catalogo.Domain.ValueObjects
{
    /// <summary>
    /// Classe base para Value Objects
    /// Value Objects são objetos que não têm identidade, são definidos apenas por seus valores
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        /// Retorna os componentes que definem a igualdade do Value Object
        /// </summary>
        protected abstract IEnumerable<object> GetEqualityComponents();

        /// <summary>
        /// Verifica se dois Value Objects são iguais
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <summary>
        /// Gera o HashCode baseado nos componentes
        /// </summary>
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        /// Operador de igualdade
        /// </summary>
        public static bool operator ==(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
                return false;

            return ReferenceEquals(left, null) || left.Equals(right);
        }

        /// <summary>
        /// Operador de desigualdade
        /// </summary>
        public static bool operator !=(ValueObject left, ValueObject right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Cria uma cópia do Value Object
        /// </summary>
        protected virtual T Clone<T>() where T : ValueObject
        {
            return (T)this.MemberwiseClone();
        }
    }
}