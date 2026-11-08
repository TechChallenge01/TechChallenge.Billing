using Application.Gateways.Pagamentos;
using Domain.Aggregates.PagamentoAggregates;

namespace Application.UseCases.Pagamentos;

public class GetByIdUseCase
{
    private readonly PagamentoGateway _gateway;

    private GetByIdUseCase(PagamentoGateway gateway) => _gateway = gateway;

    public static GetByIdUseCase Create(PagamentoGateway gateway) => new(gateway);

    public async Task<Pagamento?> Run(Guid id, CancellationToken ct)
        => await _gateway.GetById(id, ct);
}
