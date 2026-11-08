using Shared.DTOs.Orcamentos.Input;

namespace Application.Interfaces;

public interface IOrcamentoDataSource
{
    Task<OrcamentoInputDTO?> GetById(Guid id, CancellationToken ct);
    Task<OrcamentoInputDTO?> GetByOrdemServicoId(Guid ordemServicoId, CancellationToken ct);
    Task<(List<OrcamentoInputDTO> items, int total)> GetPaginated(int page, int pageSize, CancellationToken ct);
    Task Create(OrcamentoInputDTO orcamento, CancellationToken ct);
    Task Update(OrcamentoInputDTO orcamento, CancellationToken ct);
}
