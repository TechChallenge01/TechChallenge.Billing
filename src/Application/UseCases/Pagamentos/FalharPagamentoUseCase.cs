using Application.Gateways.Pagamentos;
using Application.Interfaces;

namespace Application.UseCases.Pagamentos;

public class FalharPagamentoUseCase
{
    private readonly PagamentoGateway _gateway;
    private readonly IEventPublisher _eventPublisher;

    private FalharPagamentoUseCase(PagamentoGateway gateway, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _eventPublisher = eventPublisher;
    }

    public static FalharPagamentoUseCase Create(PagamentoGateway gateway, IEventPublisher eventPublisher)
        => new(gateway, eventPublisher);

    public async Task Run(string idTransacaoExterna, string motivo, Guid idUsuario, CancellationToken ct)
    {
        var pagamento = await _gateway.GetByTransacaoExterna(idTransacaoExterna, ct)
            ?? throw new KeyNotFoundException($"Pagamento com transação {idTransacaoExterna} não encontrado.");

        pagamento.Recusar(motivo, idUsuario);
        await _gateway.Update(pagamento, ct);

        await _eventPublisher.PublishAsync("pagamento.falhou", new
        {
            PagamentoId = pagamento.Id,
            pagamento.OrcamentoId,
            pagamento.OrdemServicoId,
            pagamento.ClienteId,
            Motivo = motivo
        }, ct);
    }
}
