using Application.Gateways.Pagamentos;
using Application.Interfaces;

namespace Application.UseCases.Pagamentos;

public class EstornarPagamentoUseCase
{
    private readonly PagamentoGateway _gateway;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly IEventPublisher _eventPublisher;

    private EstornarPagamentoUseCase(PagamentoGateway gateway, IMercadoPagoService mercadoPagoService, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _mercadoPagoService = mercadoPagoService;
        _eventPublisher = eventPublisher;
    }

    public static EstornarPagamentoUseCase Create(PagamentoGateway gateway, IMercadoPagoService mercadoPagoService, IEventPublisher eventPublisher)
        => new(gateway, mercadoPagoService, eventPublisher);

    public async Task Run(Guid pagamentoId, Guid idUsuario, CancellationToken ct)
    {
        var pagamento = await _gateway.GetById(pagamentoId, ct)
            ?? throw new KeyNotFoundException($"Pagamento {pagamentoId} não encontrado.");

        if (pagamento.IdTransacaoExterna is not null)
            await _mercadoPagoService.Estornar(pagamento.IdTransacaoExterna, ct);

        pagamento.Estornar(idUsuario);
        await _gateway.Update(pagamento, ct);

        await _eventPublisher.PublishAsync("pagamento.estornado", new
        {
            PagamentoId = pagamento.Id,
            pagamento.OrcamentoId,
            pagamento.OrdemServicoId,
            pagamento.ClienteId
        }, ct);
    }
}
