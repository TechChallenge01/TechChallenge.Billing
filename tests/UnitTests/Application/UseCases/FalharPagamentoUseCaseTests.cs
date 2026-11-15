using Application.Gateways.Pagamentos;
using Application.Interfaces;
using Application.UseCases.Pagamentos;
using FluentAssertions;
using Moq;
using Shared.DTOs.Pagamentos.Input;

namespace UnitTests.Application.UseCases;

public class FalharPagamentoUseCaseTests
{
    private readonly Mock<IPagamentoDataSource> _mockDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private FalharPagamentoUseCase CriarUseCase()
        => FalharPagamentoUseCase.Create(PagamentoGateway.Create(_mockDS.Object), _mockPublisher.Object);

    [Fact]
    public async Task Run_QuandoPagamentoExiste_DeveRecusarEPublicarEvento()
    {
        var dto = new PagamentoInputDTO
        {
            Id = Guid.NewGuid(), OrcamentoId = Guid.NewGuid(), OrdemServicoId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(), Valor = 150m, Status = "Pendente",
            IdTransacaoExterna = "txn_fail",
            IdUsuarioCriacao = Guid.NewGuid(), DataCriacao = DateTime.UtcNow, Ativo = true
        };
        _mockDS.Setup(x => x.GetByTransacaoExterna("txn_fail", It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var useCase = CriarUseCase();
        await useCase.Run("txn_fail", "Cartão recusado", Guid.NewGuid(), CancellationToken.None);

        _mockPublisher.Verify(x => x.PublishAsync("pagamento.falhou", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoPagamentoNaoEncontrado_DeveLancarException()
    {
        _mockDS.Setup(x => x.GetByTransacaoExterna(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagamentoInputDTO?)null);

        var useCase = CriarUseCase();
        var act = () => useCase.Run("txn_xxx", "motivo", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
