using Application.Gateways.Orcamentos;
using Application.Interfaces;
using Application.UseCases.Orcamentos;
using FluentAssertions;
using Moq;
using Shared.DTOs.Orcamentos.Input;
using Shared.DTOs.Orcamentos.Request;

namespace UnitTests.Application.UseCases;

public class GerarOrcamentoUseCaseTests
{
    private readonly Mock<IOrcamentoDataSource> _mockDataSource = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private GerarOrcamentoUseCase CriarUseCase()
    {
        var gateway = OrcamentoGateway.Create(_mockDataSource.Object);
        return GerarOrcamentoUseCase.Create(gateway, _mockPublisher.Object);
    }

    [Fact]
    public async Task Run_ComRequestValido_DeveRetornarId()
    {
        _mockDataSource.Setup(x => x.Create(It.IsAny<OrcamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new GerarOrcamentoRequestDTO
        {
            OrdemServicoId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Itens = [new() { Descricao = "Serviço", Quantidade = 1, ValorUnitario = 100m, Tipo = "Servico" }]
        };

        var useCase = CriarUseCase();
        var id = await useCase.Run(request, Guid.NewGuid(), CancellationToken.None);

        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Run_DevePublicarEventoOrcamentoGerado()
    {
        _mockDataSource.Setup(x => x.Create(It.IsAny<OrcamentoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new GerarOrcamentoRequestDTO
        {
            OrdemServicoId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Itens = [new() { Descricao = "Serviço", Quantidade = 1, ValorUnitario = 200m, Tipo = "Servico" }]
        };

        var useCase = CriarUseCase();
        await useCase.Run(request, Guid.NewGuid(), CancellationToken.None);

        _mockPublisher.Verify(x => x.PublishAsync(
            "orcamento.gerado", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
