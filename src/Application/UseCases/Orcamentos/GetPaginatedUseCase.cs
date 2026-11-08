using Application.Gateways.Orcamentos;
using Domain.Aggregates.OrcamentoAggregates;

namespace Application.UseCases.Orcamentos;

public class GetPaginatedUseCase
{
    private readonly OrcamentoGateway _gateway;

    private GetPaginatedUseCase(OrcamentoGateway gateway) => _gateway = gateway;

    public static GetPaginatedUseCase Create(OrcamentoGateway gateway) => new(gateway);

    public async Task<(List<Orcamento>, int)> Run(int page, int pageSize, CancellationToken ct)
        => await _gateway.GetPaginated(page, pageSize, ct);
}
