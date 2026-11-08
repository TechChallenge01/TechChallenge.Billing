using Application.Gateways.Pagamentos;
using Application.Interfaces;

namespace Application.UseCases.Pagamentos;

public class ConfirmarPagamentoUseCase
{
    private readonly PagamentoGateway _gateway;
    private readonly IEventPublisher _eventPublisher;

    private ConfirmarPagamentoUseCase(PagamentoGateway gateway, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _eventPublisher = eventPublisher;
    }

    public static ConfirmarPagamentoUseCase Create(PagamentoGateway gateway, IEventPublisher eventPublisher)
        => new(gateway, eventPublisher);

    public async Task Run(string idTransacaoExterna, Guid idUsuario, CancellationToken ct)
    {
        var pagamento = await _gateway.GetByTransacaoExterna(idTransacaoExterna, ct)
            ?? throw new KeyNotFoundException($"Pagamento com transação {idTransacaoExterna} não encontrado.");

        pagamento.Confirmar(idUsuario);
        await _gateway.Update(pagamento, ct);

        await _eventPublisher.PublishAsync("pagamento.confirmado", new
        {
            PagamentoId = pagamento.Id,
            pagamento.OrcamentoId,
            pagamento.OrdemServicoId,
            pagamento.ClienteId,
            pagamento.Valor
        }, ct);
    }
}
