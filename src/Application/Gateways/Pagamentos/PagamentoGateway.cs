using Application.Interfaces;
using Domain.Aggregates.PagamentoAggregates;
using Domain.Enums;
using Shared.DTOs.Pagamentos.Input;

namespace Application.Gateways.Pagamentos;

public class PagamentoGateway
{
    private readonly IPagamentoDataSource _dataSource;

    private PagamentoGateway(IPagamentoDataSource dataSource) => _dataSource = dataSource;

    public static PagamentoGateway Create(IPagamentoDataSource dataSource) => new(dataSource);

    public async Task<Pagamento?> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _dataSource.GetById(id, ct);
        return dto is null ? null : Reconstituir(dto);
    }

    public async Task<Pagamento?> GetByOrcamentoId(Guid orcamentoId, CancellationToken ct)
    {
        var dto = await _dataSource.GetByOrcamentoId(orcamentoId, ct);
        return dto is null ? null : Reconstituir(dto);
    }

    public async Task<Pagamento?> GetByTransacaoExterna(string idTransacao, CancellationToken ct)
    {
        var dto = await _dataSource.GetByTransacaoExterna(idTransacao, ct);
        return dto is null ? null : Reconstituir(dto);
    }

    public async Task Create(Pagamento pagamento, CancellationToken ct)
        => await _dataSource.Create(Serializar(pagamento), ct);

    public async Task Update(Pagamento pagamento, CancellationToken ct)
        => await _dataSource.Update(Serializar(pagamento), ct);

    private static Pagamento Reconstituir(PagamentoInputDTO dto) =>
        new(
            dto.Id, dto.OrcamentoId, dto.OrdemServicoId, dto.ClienteId,
            dto.Valor, Enum.Parse<EStatusPagamento>(dto.Status),
            dto.IdTransacaoExterna, dto.LinkPagamento, dto.MotivoFalha,
            dto.IdUsuarioCriacao, dto.DataCriacao,
            dto.IdUsuarioAtualizacao, dto.DataAtualizacao, dto.Ativo
        );

    private static PagamentoInputDTO Serializar(Pagamento p) => new()
    {
        Id = p.Id,
        OrcamentoId = p.OrcamentoId,
        OrdemServicoId = p.OrdemServicoId,
        ClienteId = p.ClienteId,
        Valor = p.Valor,
        Status = p.Status.ToString(),
        IdTransacaoExterna = p.IdTransacaoExterna,
        LinkPagamento = p.LinkPagamento,
        MotivoFalha = p.MotivoFalha,
        IdUsuarioCriacao = p.IdUsuarioCriacao,
        DataCriacao = p.DataCriacao,
        IdUsuarioAtualizacao = p.IdUsuarioAtualizacao,
        DataAtualizacao = p.DataAtualizacao,
        Ativo = p.Ativo
    };
}
