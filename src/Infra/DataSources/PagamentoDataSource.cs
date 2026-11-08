using Application.Interfaces;
using MongoDB.Driver;
using Shared.DTOs.Pagamentos.Input;

namespace Infra.DataSources;

public class PagamentoDataSource : IPagamentoDataSource
{
    private readonly IMongoCollection<PagamentoInputDTO> _collection;

    public PagamentoDataSource(IMongoDatabase database)
        => _collection = database.GetCollection<PagamentoInputDTO>("pagamentos");

    public async Task<PagamentoInputDTO?> GetById(Guid id, CancellationToken ct)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<PagamentoInputDTO?> GetByOrcamentoId(Guid orcamentoId, CancellationToken ct)
        => await _collection.Find(x => x.OrcamentoId == orcamentoId).FirstOrDefaultAsync(ct);

    public async Task<PagamentoInputDTO?> GetByTransacaoExterna(string idTransacao, CancellationToken ct)
        => await _collection.Find(x => x.IdTransacaoExterna == idTransacao).FirstOrDefaultAsync(ct);

    public async Task Create(PagamentoInputDTO dto, CancellationToken ct)
        => await _collection.InsertOneAsync(dto, cancellationToken: ct);

    public async Task Update(PagamentoInputDTO dto, CancellationToken ct)
        => await _collection.ReplaceOneAsync(x => x.Id == dto.Id, dto, cancellationToken: ct);
}
