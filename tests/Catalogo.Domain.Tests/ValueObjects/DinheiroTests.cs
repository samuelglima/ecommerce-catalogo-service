using System;
using Xunit;
using FluentAssertions;
using Catalogo.Domain.ValueObjects;

namespace Catalogo.Domain.Tests.ValueObjects
{
    public class DinheiroTests
    {
        [Fact]
        public void Dinheiro_CriarComValorValido_DeveCriarCorretamente()
        {
            // Arrange & Act
            var dinheiro = new Dinheiro(100.50m, "BRL");

            // Assert
            dinheiro.Valor.Should().Be(100.50m);
            dinheiro.Moeda.Should().Be("BRL");
        }

        [Fact]
        public void Dinheiro_CriarComValorNegativo_DeveLancarExcecao()
        {
            // Arrange & Act
            Action act = () => new Dinheiro(-10m, "BRL");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*não pode ser negativo*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Dinheiro_CriarComMoedaInvalida_DeveLancarExcecao(string moeda)
        {
            // Arrange & Act
            Action act = () => new Dinheiro(100m, moeda);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*moeda deve ser informada*");
        }

        [Fact]
        public void Dinheiro_SomarValoresMesmaMoeda_DeveRetornarSomaCorreta()
        {
            // Arrange
            var valor1 = new Dinheiro(100m, "BRL");
            var valor2 = new Dinheiro(50m, "BRL");

            // Act
            var resultado = valor1.Somar(valor2);

            // Assert
            resultado.Valor.Should().Be(150m);
            resultado.Moeda.Should().Be("BRL");
        }

        [Fact]
        public void Dinheiro_SomarValoresMoedasDiferentes_DeveLancarExcecao()
        {
            // Arrange
            var valor1 = new Dinheiro(100m, "BRL");
            var valor2 = new Dinheiro(50m, "USD");

            // Act
            Action act = () => valor1.Somar(valor2);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*moedas diferentes*");
        }

        [Fact]
        public void Dinheiro_SubtrairValores_DeveRetornarDiferencaCorreta()
        {
            // Arrange
            var valor1 = new Dinheiro(100m, "BRL");
            var valor2 = new Dinheiro(30m, "BRL");

            // Act
            var resultado = valor1.Subtrair(valor2);

            // Assert
            resultado.Valor.Should().Be(70m);
            resultado.Moeda.Should().Be("BRL");
        }

        [Fact]
        public void Dinheiro_MultiplicarPorNumero_DeveRetornarValorCorreto()
        {
            // Arrange
            var valor = new Dinheiro(100m, "BRL");

            // Act
            var resultado = valor.Multiplicar(1.5m);

            // Assert
            resultado.Valor.Should().Be(150m);
            resultado.Moeda.Should().Be("BRL");
        }

        [Fact]
        public void Dinheiro_CompararDoisValoresIguais_DeveRetornarTrue()
        {
            // Arrange
            var valor1 = new Dinheiro(100m, "BRL");
            var valor2 = new Dinheiro(100m, "BRL");

            // Act & Assert
            valor1.Should().Be(valor2);
            (valor1 == valor2).Should().BeTrue();
            (valor1 != valor2).Should().BeFalse();
        }

        [Fact]
        public void Dinheiro_CompararDoisValoresDiferentes_DeveRetornarFalse()
        {
            // Arrange
            var valor1 = new Dinheiro(100m, "BRL");
            var valor2 = new Dinheiro(200m, "BRL");

            // Act & Assert
            valor1.Should().NotBe(valor2);
            (valor1 == valor2).Should().BeFalse();
            (valor1 != valor2).Should().BeTrue();
        }

        [Fact]
        public void Dinheiro_ToString_DeveRetornarFormatoCorreto()
        {
            // Arrange
            var valor = new Dinheiro(1234.56m, "BRL");

            // Act
            var resultado = valor.ToString();

            // Assert
            resultado.Should().Be("BRL 1234.56");
        }
    }
}