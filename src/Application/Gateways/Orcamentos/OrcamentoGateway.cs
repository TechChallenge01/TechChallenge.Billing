using Application.Interfaces;
using Domain.Aggregates.OrcamentoAggregates;
using Domain.Enums;
using Domain.ValueObjects;
using Shared.DTOs.Orcamentos.Input;

namespace Application.Gateways.Orcamentos;

public class OrcamentoGateway
{
    private readonly IOrcamentoDataSource _dataSource;

    private OrcamentoGateway(IOrcamentoDataSource dataSource) => _dataSource = dataSource;

    public static OrcamentoGateway Create(IOrcamentoDataSource dataSource) => new(dataSource);

    public async Task<Orcamento?> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _dataSource.GetById(id, ct);
        return dto is null ? null : Reconstituir(dto);
    }

    public async Task<Orcamento?> GetByOrdemServicoId(Guid ordemServicoId, CancellationToken ct)
    {
        var dto = await _dataSource.GetByOrdemServicoId(ordemServicoId, ct);
        return dto is null ? null : Reconstituir(dto);
    }

    public async Task<(List<Orcamento>, int)> GetPaginated(int page, int pageSize, CancellationToken ct)
    {
        var (dtos, total) = await _dataSource.GetPaginated(page, pageSize, ct);
        return (dtos.Select(Reconstituir).ToList(), total);
    }

    public async Task Create(Orcamento orcamento, CancellationToken ct)
        => await _dataSource.Create(Serializar(orcamento), ct);

    public async Task Update(Orcamento orcamento, CancellationToken ct)
        => await _dataSource.Update(Serializar(orcamento), ct);

    private static Orcamento Reconstituir(OrcamentoInputDTO dto) =>
        new(
            dto.Id, dto.OrdemServicoId, dto.ClienteId,
            dto.Itens.Select(i => new ItemOrcamento(i.Descricao, i.Quantidade, i.ValorUnitario, i.Tipo)).ToList(),
            Enum.Parse<EStatusOrcamento>(dto.Status),
            dto.ValorTotal, dto.DataExpiracao, dto.MotivoRejeicao,
            dto.IdUsuarioCriacao, dto.DataCriacao,
            dto.IdUsuarioAtualizacao, dto.DataAtualizacao, dto.Ativo
        );

    private static OrcamentoInputDTO Serializar(Orcamento o) => new()
    {
        Id = o.Id,
        OrdemServicoId = o.OrdemServicoId,
        ClienteId = o.ClienteId,
        Itens = o.Itens.Select(i => new ItemOrcamentoInputDTO
        {
            Descricao = i.Descricao, Quantidade = i.Quantidade, ValorUnitario = i.ValorUnitario, Tipo = i.Tipo
        }).ToList(),
        Status = o.Status.ToString(),
        ValorTotal = o.ValorTotal,
        DataExpiracao = o.DataExpiracao,
        MotivoRejeicao = o.MotivoRejeicao,
        IdUsuarioCriacao = o.IdUsuarioCriacao,
        DataCriacao = o.DataCriacao,
        IdUsuarioAtualizacao = o.IdUsuarioAtualizacao,
        DataAtualizacao = o.DataAtualizacao,
        Ativo = o.Ativo
    };
}
