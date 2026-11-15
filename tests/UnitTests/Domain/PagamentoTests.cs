using Domain.Aggregates.PagamentoAggregates;
using Domain.Enums;
using FluentAssertions;

namespace UnitTests.Domain;

public class PagamentoTests
{
    private static Pagamento CriarPagamento() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 500m, Guid.NewGuid());

    private static Pagamento CriarPagamentoComStatus(EStatusPagamento status)
    {
        var id = Guid.NewGuid();
        return new(
            id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 500m, status,
            status == EStatusPagamento.Pendente ? null : "txn_123",
            null, null,
            Guid.NewGuid(), DateTime.UtcNow, null, null, true);
    }

    [Fact]
    public void CriarPagamento_DeveTerStatusPendente()
    {
        var pagamento = CriarPagamento();
        pagamento.Status.Should().Be(EStatusPagamento.Pendente);
    }

    [Fact]
    public void CriarPagamento_DevePreencherValor()
    {
        var pagamento = CriarPagamento();
        pagamento.Valor.Should().Be(500m);
    }

    [Fact]
    public void RegistrarCobranca_QuandoPendente_DevePreencherDados()
    {
        var pagamento = CriarPagamento();

        pagamento.RegistrarCobranca("txn_abc", "https://link.com/pay", Guid.NewGuid());

        pagamento.IdTransacaoExterna.Should().Be("txn_abc");
        pagamento.LinkPagamento.Should().Be("https://link.com/pay");
    }

    [Fact]
    public void RegistrarCobranca_QuandoNaoPendente_DeveLancarException()
    {
        var pagamento = CriarPagamento();
        pagamento.Confirmar(Guid.NewGuid());

        var act = () => pagamento.RegistrarCobranca("txn_abc", "https://link.com/pay", Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Confirmar_QuandoPendente_DeveMudarStatusParaAprovado()
    {
        var pagamento = CriarPagamento();

        pagamento.Confirmar(Guid.NewGuid());

        pagamento.Status.Should().Be(EStatusPagamento.Aprovado);
    }

    [Fact]
    public void Confirmar_QuandoNaoPendente_DeveLancarException()
    {
        var pagamento = CriarPagamento();
        pagamento.Confirmar(Guid.NewGuid());

        var act = () => pagamento.Confirmar(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Recusar_QuandoPendente_DeveMudarStatusEMotivoFalha()
    {
        var pagamento = CriarPagamento();
        const string motivo = "Saldo insuficiente";

        pagamento.Recusar(motivo, Guid.NewGuid());

        pagamento.Status.Should().Be(EStatusPagamento.Recusado);
        pagamento.MotivoFalha.Should().Be(motivo);
    }

    [Fact]
    public void Recusar_QuandoNaoPendente_DeveLancarException()
    {
        var pagamento = CriarPagamento();
        pagamento.Confirmar(Guid.NewGuid());

        var act = () => pagamento.Recusar("motivo", Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Estornar_QuandoAprovado_DeveMudarStatusParaEstornado()
    {
        var pagamento = CriarPagamento();
        pagamento.Confirmar(Guid.NewGuid());

        pagamento.Estornar(Guid.NewGuid());

        pagamento.Status.Should().Be(EStatusPagamento.Estornado);
    }

    [Fact]
    public void Estornar_QuandoNaoAprovado_DeveLancarException()
    {
        var pagamento = CriarPagamento();

        var act = () => pagamento.Estornar(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CriarPagamento_ConstrutorReconstituicao_DevePreencherTodosOsCampos()
    {
        var id = Guid.NewGuid();
        var orcId = Guid.NewGuid();
        var osId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        var pagamento = new Pagamento(
            id, orcId, osId, clienteId, 1200m, EStatusPagamento.Aprovado,
            "txn_xyz", "https://link.com", null,
            Guid.NewGuid(), DateTime.UtcNow, null, null, true);

        pagamento.Id.Should().Be(id);
        pagamento.OrcamentoId.Should().Be(orcId);
        pagamento.OrdemServicoId.Should().Be(osId);
        pagamento.ClienteId.Should().Be(clienteId);
        pagamento.Valor.Should().Be(1200m);
        pagamento.Status.Should().Be(EStatusPagamento.Aprovado);
        pagamento.IdTransacaoExterna.Should().Be("txn_xyz");
        pagamento.LinkPagamento.Should().Be("https://link.com");
        pagamento.Ativo.Should().BeTrue();
    }
}
