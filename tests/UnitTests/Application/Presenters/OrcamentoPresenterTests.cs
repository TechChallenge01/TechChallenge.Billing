using Application.Presenters.Orcamentos;
using Domain.Aggregates.OrcamentoAggregates;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using System.Net;

namespace UnitTests.Application.Presenters;

public class OrcamentoPresenterTests
{
    private static Orcamento CriarOrcamento() =>
        new(Guid.NewGuid(), Guid.NewGuid(),
            [new("Serviço", 1, 100m, "Servico")],
            Guid.NewGuid());

    private static OrcamentoPresenter CriarPresenter() => new("Operação realizada");

    [Fact]
    public void TransformObject_DeveRetornarCommandResultComStatusOK()
    {
        var presenter = CriarPresenter();
        var orcamento = CriarOrcamento();

        var result = presenter.TransformObject(orcamento);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
        result.Data.ValorTotal.Should().Be(100m);
    }

    [Fact]
    public void TransformPaged_DeveRetornarCommandResultComStatusPartialContent()
    {
        var presenter = CriarPresenter();
        var orcamentos = new List<Orcamento> { CriarOrcamento(), CriarOrcamento() };

        var result = presenter.TransformPaged(orcamentos, 1, 10, 10);

        result.StatusCode.Should().Be(HttpStatusCode.PartialContent);
        result.Data.Items.Should().HaveCount(2);
        result.Data.Total.Should().Be(10);
        result.Data.Page.Should().Be(1);
    }

    [Fact]
    public void Created_DeveRetornarStatusCreated()
    {
        var presenter = CriarPresenter();

        var result = presenter.Created(Guid.NewGuid());

        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public void NoContent_DeveRetornarStatusNoContent()
    {
        var presenter = CriarPresenter();

        var result = presenter.NoContent();

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public void NotFound_DeveRetornarStatusNotFound()
    {
        var presenter = CriarPresenter();

        var result = presenter.NotFound();

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void BadRequest_DeveRetornarStatusBadRequest()
    {
        var presenter = CriarPresenter();

        var result = presenter.BadRequest("Dados inválidos");

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Be("Dados inválidos");
    }

    [Fact]
    public void InternalError_DeveRetornarStatusInternalServerError()
    {
        var presenter = CriarPresenter();

        var result = presenter.InternalError();

        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Forbidden_DeveRetornarStatusForbidden()
    {
        var presenter = CriarPresenter();

        var result = presenter.Forbidden();

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
