using Domain.ValueObjects;
using FluentAssertions;

namespace UnitTests.Domain;

public class ItemOrcamentoTests
{
    [Fact]
    public void CriarItem_DeveCalcularValorTotal()
    {
        var item = new ItemOrcamento("Troca de óleo", 2, 50m, "Servico");

        item.ValorTotal.Should().Be(100m);
    }

    [Fact]
    public void CriarItem_DevePreencherPropriedades()
    {
        var item = new ItemOrcamento("Filtro de ar", 3, 25m, "Peca");

        item.Descricao.Should().Be("Filtro de ar");
        item.Quantidade.Should().Be(3);
        item.ValorUnitario.Should().Be(25m);
        item.Tipo.Should().Be("Peca");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CriarItem_ComQuantidadeInvalida_DeveLancarException(int quantidade)
    {
        var act = () => new ItemOrcamento("Teste", quantidade, 10m, "Servico");

        act.Should().Throw<ArgumentException>().WithMessage("*Quantidade*");
    }

    [Fact]
    public void CriarItem_ComValorNegativo_DeveLancarException()
    {
        var act = () => new ItemOrcamento("Teste", 1, -1m, "Servico");

        act.Should().Throw<ArgumentException>().WithMessage("*Valor unitário*");
    }

    [Fact]
    public void CriarItem_ComValorZero_NaoDeveLancarException()
    {
        var act = () => new ItemOrcamento("Desconto", 1, 0m, "Desconto");

        act.Should().NotThrow();
    }
}
