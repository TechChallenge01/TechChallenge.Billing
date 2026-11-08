using Domain.Aggregates.PagamentoAggregates;
using Domain.Enums;
using FluentAssertions;

namespace UnitTests.Domain;

public class PagamentoTests
{
    private static Pagamento CriarPagamento() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 500m, Guid.NewGuid());

    [Fact]
    public void CriarPagamento_DeveTerStatusPendente()
    {
        var pagamento = CriarPagamento();

        pagamento.Status.Should().Be(EStatusPagamento.Pendente);
    }

    [Fact]
    public void Confirmar_QuandoPendente_DeveTornarAprovado()
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
    public void Recusar_QuandoPendente_DeveTornarRecusado()
    {
        var pagamento = CriarPagamento();

        pagamento.Recusar("Saldo insuficiente", Guid.NewGuid());

        pagamento.Status.Should().Be(EStatusPagamento.Recusado);
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
    public void Estornar_QuandoAprovado_DeveTornarEstornado()
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
    public void RegistrarCobranca_QuandoPendente_DeveDefinirIds()
    {
        var pagamento = CriarPagamento();
        const string idTransacao = "txn_123";
        const string link = "https://mp.com/pay/123";

        pagamento.RegistrarCobranca(idTransacao, link, Guid.NewGuid());

        pagamento.IdTransacaoExterna.Should().Be(idTransacao);
        pagamento.LinkPagamento.Should().Be(link);
    }

    [Fact]
    public void RegistrarCobranca_QuandoNaoPendente_DeveLancarException()
    {
        var pagamento = CriarPagamento();
        pagamento.Confirmar(Guid.NewGuid());

        var act = () => pagamento.RegistrarCobranca("txn", "link", Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }
}
