using Application.Gateways.Orcamentos;
using Application.Interfaces;

namespace Application.UseCases.Orcamentos;

public class RejeitarOrcamentoUseCase
{
    private readonly OrcamentoGateway _gateway;
    private readonly IEventPublisher _eventPublisher;

    private RejeitarOrcamentoUseCase(OrcamentoGateway gateway, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _eventPublisher = eventPublisher;
    }

    public static RejeitarOrcamentoUseCase Create(OrcamentoGateway gateway, IEventPublisher eventPublisher)
        => new(gateway, eventPublisher);

    public async Task Run(Guid orcamentoId, string motivo, Guid idUsuario, CancellationToken ct)
    {
        var orcamento = await _gateway.GetById(orcamentoId, ct)
            ?? throw new KeyNotFoundException($"Orçamento {orcamentoId} não encontrado.");

        orcamento.Rejeitar(motivo, idUsuario);
        await _gateway.Update(orcamento, ct);

        await _eventPublisher.PublishAsync("orcamento.rejeitado", new
        {
            OrcamentoId = orcamento.Id,
            orcamento.OrdemServicoId,
            orcamento.ClienteId,
            Motivo = motivo
        }, ct);
    }
}
