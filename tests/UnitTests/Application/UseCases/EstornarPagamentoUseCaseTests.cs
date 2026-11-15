using Application.Gateways.Pagamentos;
using Application.Interfaces;
using Application.UseCases.Pagamentos;
using FluentAssertions;
using Moq;
using Shared.DTOs.Pagamentos.Input;

namespace UnitTests.Application.UseCases;

public class EstornarPagamentoUseCaseTests
{
    private readonly Mock<IPagamentoDataSource> _mockDS = new();
    private readonly Mock<IMercadoPagoService> _mockMP = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private EstornarPagamentoUseCase CriarUseCase()
        => EstornarPagamentoUseCase.Create(PagamentoGateway.Create(_mockDS.Object), _mockMP.Object, _mockPublisher.Object);

    private static PagamentoInputDTO PagamentoAprovadoDTO(bool comTransacao = true) => new()
    {
        Id = Guid.NewGuid(), OrcamentoId = Guid.NewGuid(), OrdemServicoId = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(), Valor = 300m, Status = "Aprovado",
        IdTransacaoExterna = comTransacao ? "txn_abc" : null,
        IdUsuarioCriacao = Guid.NewGuid(), DataCriacao = DateTime.UtcNow, Ativo = true
    };

    [Fact]
    public async Task Run_ComTransacaoExterna_DeveEstornarNaMP()
    {
        var dto = PagamentoAprovadoDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMP.Setup(x => x.Estornar("txn_abc", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var useCase = CriarUseCase();
        await useCase.Run(dto.Id, Guid.NewGuid(), CancellationToken.None);

        _mockMP.Verify(x => x.Estornar("txn_abc", It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisher.Verify(x => x.PublishAsync("pagamento.estornado", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_SemTransacaoExterna_NaoDeveChamarMercadoPago()
    {
        var dto = PagamentoAprovadoDTO(comTransacao: false);
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<PagamentoInputDTO>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var useCase = CriarUseCase();
        await useCase.Run(dto.Id, Guid.NewGuid(), CancellationToken.None);

        _mockMP.Verify(x => x.Estornar(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_QuandoPagamentoNaoEncontrado_DeveLancarException()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagamentoInputDTO?)null);

        var useCase = CriarUseCase();
        var act = () => useCase.Run(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
