using Application.Presenters.Pagamentos;
using Domain.Aggregates.PagamentoAggregates;
using FluentAssertions;
using System.Net;

namespace UnitTests.Application.Presenters;

public class PagamentoPresenterTests
{
    private static Pagamento CriarPagamento() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 300m, Guid.NewGuid());

    private static PagamentoPresenter CriarPresenter() => new("OK");

    [Fact]
    public void TransformObject_DeveRetornarStatusOK()
    {
        var result = CriarPresenter().TransformObject(CriarPagamento());

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
        result.Data.Valor.Should().Be(300m);
    }

    [Fact]
    public void NotFound_DeveRetornarStatusNotFound()
    {
        var result = CriarPresenter().NotFound();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void BadRequest_ComMensagemCustom_DeveUsarMensagemCustom()
    {
        var result = CriarPresenter().BadRequest("Erro customizado");
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Be("Erro customizado");
    }

    [Fact]
    public void NoContent_DeveRetornarStatusNoContent()
    {
        var result = CriarPresenter().NoContent();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public void InternalError_DeveRetornarStatus500()
    {
        var result = CriarPresenter().InternalError();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Created_DeveRetornarStatusCreated()
    {
        var result = CriarPresenter().Created("link_pagamento");
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Data.Should().Be("link_pagamento");
    }
}
