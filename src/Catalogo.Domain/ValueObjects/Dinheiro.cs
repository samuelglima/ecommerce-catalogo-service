using System;
using System.Collections.Generic;

namespace Catalogo.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um valor monetário
    /// </summary>
    public class Dinheiro : ValueObject
    {
        public decimal Valor { get; private set; }
        public string Moeda { get; private set; }

        public Dinheiro(decimal valor, string moeda = "BRL")
        {
            if (valor < 0)
                throw new ArgumentException("O valor não pode ser negativo", nameof(valor));

            if (string.IsNullOrWhiteSpace(moeda))
                throw new ArgumentException("A moeda deve ser informada", nameof(moeda));

            Valor = valor;
            Moeda = moeda.ToUpperInvariant();
        }

        /// <summary>
        /// Soma dois valores monetários (devem ser da mesma moeda)
        /// </summary>
        public Dinheiro Somar(Dinheiro outro)
        {
            if (outro == null)
                throw new ArgumentNullException(nameof(outro));

            if (Moeda != outro.Moeda)
                throw new InvalidOperationException($"Não é possível somar valores de moedas diferentes: {Moeda} e {outro.Moeda}");

            return new Dinheiro(Valor + outro.Valor, Moeda);
        }

        /// <summary>
        /// Subtrai dois valores monetários (devem ser da mesma moeda)
        /// </summary>
        public Dinheiro Subtrair(Dinheiro outro)
        {
            if (outro == null)
                throw new ArgumentNullException(nameof(outro));

            if (Moeda != outro.Moeda)
                throw new InvalidOperationException($"Não é possível subtrair valores de moedas diferentes: {Moeda} e {outro.Moeda}");

            return new Dinheiro(Valor - outro.Valor, Moeda);
        }

        /// <summary>
        /// Multiplica o valor por um número
        /// </summary>
        public Dinheiro Multiplicar(decimal multiplicador)
        {
            return new Dinheiro(Valor * multiplicador, Moeda);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Valor;
            yield return Moeda;
        }

        public override string ToString()
        {
            return $"{Moeda} {Valor:F2}";
        }
    }
}