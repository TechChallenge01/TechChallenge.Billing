using Application.Gateways.Orcamentos;
using Application.Interfaces;
using Domain.Aggregates.OrcamentoAggregates;
using Domain.ValueObjects;
using Shared.DTOs.Orcamentos.Request;

namespace Application.UseCases.Orcamentos;

public class GerarOrcamentoUseCase
{
    private readonly OrcamentoGateway _gateway;
    private readonly IEventPublisher _eventPublisher;

    private GerarOrcamentoUseCase(OrcamentoGateway gateway, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _eventPublisher = eventPublisher;
    }

    public static GerarOrcamentoUseCase Create(OrcamentoGateway gateway, IEventPublisher eventPublisher)
        => new(gateway, eventPublisher);

    public async Task<Guid> Run(GerarOrcamentoRequestDTO request, Guid idUsuario, CancellationToken ct)
    {
        var itens = request.Itens.Select(i =>
            new ItemOrcamento(i.Descricao, i.Quantidade, i.ValorUnitario, i.Tipo)).ToList();

        var orcamento = new Orcamento(request.OrdemServicoId, request.ClienteId, itens, idUsuario);

        await _gateway.Create(orcamento, ct);

        await _eventPublisher.PublishAsync("orcamento.gerado", new
        {
            OrcamentoId = orcamento.Id,
            orcamento.OrdemServicoId,
            orcamento.ClienteId,
            orcamento.ValorTotal,
            orcamento.DataExpiracao
        }, ct);

        return orcamento.Id;
    }
}
