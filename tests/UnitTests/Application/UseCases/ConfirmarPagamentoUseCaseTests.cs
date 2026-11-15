using Application.Gateways.Pagamentos;
using Application.Interfaces;
using Application.UseCases.Pagamentos;
using FluentAssertions;
using Moq;
using Shared.DTOs.Pagamentos.Input;

namespace UnitTests.Application.UseCases;

public class ConfirmarPagamentoUseCaseTests
{
    private readonly Mock<IPagamentoDataSource> _mockDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private ConfirmarPagamentoUseCase CriarUseCase()
        => ConfirmarPagamentoUseCase.Create(PagamentoGateway.Create(_mockDS.Object), _mockPublisher.Object);

    private static PagamentoInputDTO PagamentoPendenteDTO() => new()
    {
        Id = Guid.NewGuid(), OrcamentoId = Guid.NewGuid(), OrdemServicoId = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(), Valor = 500m, Status = "Pendente",
        IdTransacaoExterna = "txn_123",
        IdUsuarioCriacao = Guid.NewGuid(), DataCriacao = DateTime.UtcNow, Ativo = true
    };

    [Fact]
    public async Task Run_QuandoPagamentoExiste_DeveConfirmarEPublicarEvento()
    {
        var dto = PagamentoPendenteDTO();
        _mockDS.Setup(x => x.GetByTransacaoExterna("txn_123", It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var useCase = CriarUseCase();
        await useCase.Run("txn_123", Guid.NewGuid(), CancellationToken.None);

        _mockPublisher.Verify(x => x.PublishAsync("pagamento.confirmado", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoPagamentoNaoEncontrado_DeveLancarException()
    {
        _mockDS.Setup(x => x.GetByTransacaoExterna(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagamentoInputDTO?)null);

        var useCase = CriarUseCase();
        var act = () => useCase.Run("txn_xxx", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
