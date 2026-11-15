using Application.Gateways.Orcamentos;
using Application.Gateways.Pagamentos;
using Application.Interfaces;
using Application.UseCases.Orcamentos;
using FluentAssertions;
using Moq;
using Shared.DTOs.Orcamentos.Input;
using Shared.DTOs.Pagamentos.Input;

namespace UnitTests.Application.UseCases;

public class AprovarOrcamentoUseCaseTests
{
    private readonly Mock<IOrcamentoDataSource> _mockOrcDS = new();
    private readonly Mock<IPagamentoDataSource> _mockPagDS = new();
    private readonly Mock<IMercadoPagoService> _mockMP = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private AprovarOrcamentoUseCase CriarUseCase()
    {
        var orcGateway = OrcamentoGateway.Create(_mockOrcDS.Object);
        var pagGateway = PagamentoGateway.Create(_mockPagDS.Object);
        return AprovarOrcamentoUseCase.Create(orcGateway, pagGateway, _mockMP.Object, _mockPublisher.Object);
    }

    private static OrcamentoInputDTO OrcamentoPendenteDTO() => new()
    {
        Id = Guid.NewGuid(),
        OrdemServicoId = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(),
        Itens = [new() { Descricao = "Serviço", Quantidade = 1, ValorUnitario = 300m, Tipo = "Servico" }],
        Status = "Pendente",
        ValorTotal = 300m,
        DataExpiracao = DateTime.UtcNow.AddDays(3),
        IdUsuarioCriacao = Guid.NewGuid(),
        DataCriacao = DateTime.UtcNow,
        Ativo = true
    };

    [Fact]
    public async Task Run_QuandoOrcamentoExiste_DeveRetornarLinkPagamento()
    {
        var dto = OrcamentoPendenteDTO();
        _mockOrcDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
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

        var useCase = CriarUseCase();
        var link = await useCase.Run(dto.Id, Guid.NewGuid(), "cliente@email.com", CancellationToken.None);

        link.Should().Be("https://link.com");
    }

    [Fact]
    public async Task Run_QuandoOrcamentoNaoEncontrado_DeveLancarException()
    {
        _mockOrcDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoInputDTO?)null);

        var useCase = CriarUseCase();
        var act = () => useCase.Run(Guid.NewGuid(), Guid.NewGuid(), "email@test.com", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Run_QuandoMercadoPagoFalha_DeveRetornarStringVazia()
    {
        var dto = OrcamentoPendenteDTO();
        _mockOrcDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockOrcDS.Setup(x => x.Update(It.IsAny<OrcamentoInputDTO>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPagDS.Setup(x => x.Create(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMP.Setup(x => x.CriarCobranca(It.IsAny<CriarCobrancaRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("MP offline"));

        var useCase = CriarUseCase();
        var link = await useCase.Run(dto.Id, Guid.NewGuid(), "email@test.com", CancellationToken.None);

        link.Should().Be(string.Empty);
    }
}
