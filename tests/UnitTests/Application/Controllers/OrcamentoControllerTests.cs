using Application.Controllers.Orcamentos;
using Application.Interfaces;
using FluentAssertions;
using Moq;
using Shared.DTOs.Orcamentos.Input;
using Shared.DTOs.Orcamentos.Request;
using Shared.DTOs.Pagamentos.Input;
using System.Net;
using Xunit;

namespace UnitTests.Application.Controllers;

public class OrcamentoControllerTests
{
    private readonly Mock<IOrcamentoDataSource> _mockOrcDS = new();
    private readonly Mock<IPagamentoDataSource> _mockPagDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();
    private readonly Mock<IMercadoPagoService> _mockMP = new();

    private OrcamentoController CriarController() =>
        new(_mockOrcDS.Object, _mockPagDS.Object);

    private static OrcamentoInputDTO OrcamentoDTO(string status = "Pendente") => new()
    {
        Id = Guid.NewGuid(),
        OrdemServicoId = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(),
        Itens = [new() { Descricao = "Serviço", Quantidade = 1, ValorUnitario = 300m, Tipo = "Servico" }],
        Status = status,
        ValorTotal = 300m,
        DataExpiracao = DateTime.UtcNow.AddDays(3),
        IdUsuarioCriacao = Guid.NewGuid(),
        DataCriacao = DateTime.UtcNow,
        Ativo = true
    };

    [Fact]
    public async Task Gerar_DeveRetornarCreated()
    {
        _mockOrcDS.Setup(x => x.Create(It.IsAny<OrcamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new GerarOrcamentoRequestDTO
        {
            OrdemServicoId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Itens = [new() { Descricao = "Serviço", Quantidade = 1, ValorUnitario = 100m, Tipo = "Servico" }]
        };

        var result = await CriarController().Gerar(request, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetById_QuandoEncontrado_DeveRetornarOK()
    {
        var dto = OrcamentoDTO();
        _mockOrcDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await CriarController().GetById(dto.Id, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockOrcDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoInputDTO?)null);

        var result = await CriarController().GetById(Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaginated_DeveRetornarPartialContent()
    {
        var dto = OrcamentoDTO();
        _mockOrcDS.Setup(x => x.GetPaginated(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<OrcamentoInputDTO> { dto }, 1));

        var result = await CriarController().GetPaginated(1, 10, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.PartialContent);
    }

    [Fact]
    public async Task Rejeitar_QuandoEncontrado_DeveRetornarNoContent()
    {
        var dto = OrcamentoDTO();
        _mockOrcDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockOrcDS.Setup(x => x.Update(It.IsAny<OrcamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new RejeitarOrcamentoRequestDTO { Motivo = "Valor alto" };
        var result = await CriarController().Rejeitar(dto.Id, request, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Rejeitar_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockOrcDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoInputDTO?)null);

        var request = new RejeitarOrcamentoRequestDTO { Motivo = "motivo" };
        var result = await CriarController().Rejeitar(Guid.NewGuid(), request, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Aprovar_QuandoEncontrado_DeveRetornarCreated()
    {
        var dto = OrcamentoDTO();
        _mockOrcDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockOrcDS.Setup(x => x.Update(It.IsAny<OrcamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPagDS.Setup(x => x.Create(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPagDS.Setup(x => x.Update(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockMP.Setup(x => x.CriarCobranca(It.IsAny<CriarCobrancaRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CriarCobrancaResult { IdTransacao = "txn_1", LinkPagamento = "https://link.com" });

        var result = await CriarController().Aprovar(dto.Id, "cli@test.com", Guid.NewGuid(), _mockMP.Object, _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Aprovar_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockOrcDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoInputDTO?)null);

        var result = await CriarController().Aprovar(Guid.NewGuid(), "cli@test.com", Guid.NewGuid(), _mockMP.Object, _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
