using Application.Gateways.Orcamentos;
using Domain.Aggregates.OrcamentoAggregates;

namespace Application.UseCases.Orcamentos;

public class GetByIdUseCase
{
    private readonly OrcamentoGateway _gateway;

    private GetByIdUseCase(OrcamentoGateway gateway) => _gateway = gateway;

    public static GetByIdUseCase Create(OrcamentoGateway gateway) => new(gateway);

    public async Task<Orcamento?> Run(Guid id, CancellationToken ct)
        => await _gateway.GetById(id, ct);
}
