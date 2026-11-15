using Application.Controllers.Pagamentos;
using Application.Interfaces;
using FluentAssertions;
using Moq;
using Shared.DTOs.Pagamentos.Input;
using System.Net;
using Xunit;

namespace UnitTests.Application.Controllers;

public class PagamentoControllerTests
{
    private readonly Mock<IPagamentoDataSource> _mockDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();
    private readonly Mock<IMercadoPagoService> _mockMP = new();

    private PagamentoController CriarController() => new(_mockDS.Object);

    private static PagamentoInputDTO PagamentoDTO(string status = "Pendente") => new()
    {
        Id = Guid.NewGuid(),
        OrcamentoId = Guid.NewGuid(),
        OrdemServicoId = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(),
        Valor = 500m,
        Status = status,
        IdTransacaoExterna = "txn_123",
        IdUsuarioCriacao = Guid.NewGuid(),
        DataCriacao = DateTime.UtcNow,
        Ativo = true
    };

    [Fact]
    public async Task GetById_QuandoEncontrado_DeveRetornarOK()
    {
        var dto = PagamentoDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await CriarController().GetById(dto.Id, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagamentoInputDTO?)null);

        var result = await CriarController().GetById(Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmarPorWebhook_QuandoEncontrado_DeveRetornarNoContent()
    {
        var dto = PagamentoDTO();
        _mockDS.Setup(x => x.GetByTransacaoExterna("txn_123", It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CriarController().ConfirmarPorWebhook("txn_123", Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ConfirmarPorWebhook_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetByTransacaoExterna(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagamentoInputDTO?)null);

        var result = await CriarController().ConfirmarPorWebhook("txn_x", Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FalharPorWebhook_QuandoEncontrado_DeveRetornarNoContent()
    {
        var dto = PagamentoDTO();
        _mockDS.Setup(x => x.GetByTransacaoExterna("txn_123", It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CriarController().FalharPorWebhook("txn_123", "Saldo insuficiente", Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FalharPorWebhook_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetByTransacaoExterna(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagamentoInputDTO?)null);

        var result = await CriarController().FalharPorWebhook("txn_x", "motivo", Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Estornar_QuandoEncontrado_DeveRetornarNoContent()
    {
        var dto = PagamentoDTO("Aprovado");
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockMP.Setup(x => x.Estornar(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CriarController().Estornar(dto.Id, Guid.NewGuid(), _mockMP.Object, _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Estornar_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagamentoInputDTO?)null);

        var result = await CriarController().Estornar(Guid.NewGuid(), Guid.NewGuid(), _mockMP.Object, _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
