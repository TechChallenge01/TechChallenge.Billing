using Domain.Aggregates.OrcamentoAggregates;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace UnitTests.Domain;

public class OrcamentoTests
{
    private static List<ItemOrcamento> CriarItens() =>
    [
        new("Troca de óleo", 1, 50m, "Servico"),
        new("Filtro de ar", 2, 30m, "Peca")
    ];

    private static Orcamento CriarOrcamento(List<ItemOrcamento>? itens = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), itens ?? CriarItens(), Guid.NewGuid());

    [Fact]
    public void CriarOrcamento_DeveCalcularValorTotal()
    {
        var itens = CriarItens();
        var orcamento = CriarOrcamento(itens);

        orcamento.ValorTotal.Should().Be(110m);
    }

    [Fact]
    public void CriarOrcamento_DeveTerStatusPendente()
    {
        var orcamento = CriarOrcamento();

        orcamento.Status.Should().Be(EStatusOrcamento.Pendente);
    }

    [Fact]
    public void CriarOrcamento_DeveDefinirDataExpiracao()
    {
        var antes = DateTime.UtcNow;
        var orcamento = CriarOrcamento();

        orcamento.DataExpiracao.Should().BeCloseTo(antes.AddDays(3), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Aprovar_QuandoPendente_DeveMudarStatus()
    {
        var orcamento = CriarOrcamento();

        orcamento.Aprovar(Guid.NewGuid());

        orcamento.Status.Should().Be(EStatusOrcamento.Aprovado);
    }

    [Fact]
    public void Aprovar_QuandoNaoPendente_DeveLancarException()
    {
        var orcamento = CriarOrcamento();
        orcamento.Aprovar(Guid.NewGuid());

        var act = () => orcamento.Aprovar(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Rejeitar_QuandoPendente_DeveMudarStatusEMotivo()
    {
        var orcamento = CriarOrcamento();
        const string motivo = "Valor muito alto";

        orcamento.Rejeitar(motivo, Guid.NewGuid());

        orcamento.Status.Should().Be(EStatusOrcamento.Rejeitado);
        orcamento.MotivoRejeicao.Should().Be(motivo);
    }

    [Fact]
    public void Rejeitar_QuandoNaoPendente_DeveLancarException()
    {
        var orcamento = CriarOrcamento();
        orcamento.Aprovar(Guid.NewGuid());

        var act = () => orcamento.Rejeitar("motivo", Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Expirar_QuandoPendente_DeveMudarStatus()
    {
        var orcamento = CriarOrcamento();

        orcamento.Expirar(Guid.NewGuid());

        orcamento.Status.Should().Be(EStatusOrcamento.Expirado);
    }

    [Fact]
    public void Expirar_QuandoNaoPendente_NaoDeveAlterar()
    {
        var orcamento = CriarOrcamento();
        orcamento.Aprovar(Guid.NewGuid());

        orcamento.Expirar(Guid.NewGuid());

        orcamento.Status.Should().Be(EStatusOrcamento.Aprovado);
    }
}
