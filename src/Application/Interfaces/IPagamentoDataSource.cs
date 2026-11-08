using Shared.DTOs.Pagamentos.Input;

namespace Application.Interfaces;

public interface IPagamentoDataSource
{
    Task<PagamentoInputDTO?> GetById(Guid id, CancellationToken ct);
    Task<PagamentoInputDTO?> GetByOrcamentoId(Guid orcamentoId, CancellationToken ct);
    Task<PagamentoInputDTO?> GetByTransacaoExterna(string idTransacaoExterna, CancellationToken ct);
    Task Create(PagamentoInputDTO pagamento, CancellationToken ct);
    Task Update(PagamentoInputDTO pagamento, CancellationToken ct);
}
