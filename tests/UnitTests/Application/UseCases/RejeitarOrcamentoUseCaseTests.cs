using Application.Gateways.Orcamentos;
using Application.Interfaces;
using Application.UseCases.Orcamentos;
using FluentAssertions;
using Moq;
using Shared.DTOs.Orcamentos.Input;

namespace UnitTests.Application.UseCases;

public class RejeitarOrcamentoUseCaseTests
{
    private readonly Mock<IOrcamentoDataSource> _mockDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private RejeitarOrcamentoUseCase CriarUseCase()
        => RejeitarOrcamentoUseCase.Create(OrcamentoGateway.Create(_mockDS.Object), _mockPublisher.Object);

    private static OrcamentoInputDTO OrcamentoPendenteDTO() => new()
    {
        Id = Guid.NewGuid(), OrdemServicoId = Guid.NewGuid(), ClienteId = Guid.NewGuid(),
        Itens = [new() { Descricao = "S", Quantidade = 1, ValorUnitario = 100m, Tipo = "S" }],
        Status = "Pendente", ValorTotal = 100m, DataExpiracao = DateTime.UtcNow.AddDays(3),
        IdUsuarioCriacao = Guid.NewGuid(), DataCriacao = DateTime.UtcNow, Ativo = true
    };

    [Fact]
    public async Task Run_QuandoOrcamentoExiste_DeveRejeitarEPublicarEvento()
    {
        var dto = OrcamentoPendenteDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<OrcamentoInputDTO>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var useCase = CriarUseCase();
        await useCase.Run(dto.Id, "Valor alto demais", Guid.NewGuid(), CancellationToken.None);

        _mockPublisher.Verify(x => x.PublishAsync("orcamento.rejeitado", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoOrcamentoNaoEncontrado_DeveLancarException()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrcamentoInputDTO?)null);

        var useCase = CriarUseCase();
        var act = () => useCase.Run(Guid.NewGuid(), "motivo", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
