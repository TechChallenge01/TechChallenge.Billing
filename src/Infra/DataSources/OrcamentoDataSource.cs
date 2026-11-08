using Application.Interfaces;
using MongoDB.Driver;
using Shared.DTOs.Orcamentos.Input;

namespace Infra.DataSources;

public class OrcamentoDataSource : IOrcamentoDataSource
{
    private readonly IMongoCollection<OrcamentoInputDTO> _collection;

    public OrcamentoDataSource(IMongoDatabase database)
        => _collection = database.GetCollection<OrcamentoInputDTO>("orcamentos");

    public async Task<OrcamentoInputDTO?> GetById(Guid id, CancellationToken ct)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<OrcamentoInputDTO?> GetByOrdemServicoId(Guid ordemServicoId, CancellationToken ct)
        => await _collection.Find(x => x.OrdemServicoId == ordemServicoId).FirstOrDefaultAsync(ct);

    public async Task<(List<OrcamentoInputDTO>, int)> GetPaginated(int page, int pageSize, CancellationToken ct)
    {
        var filter = Builders<OrcamentoInputDTO>.Filter.Empty;
        var total = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await _collection.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task Create(OrcamentoInputDTO dto, CancellationToken ct)
        => await _collection.InsertOneAsync(dto, cancellationToken: ct);

    public async Task Update(OrcamentoInputDTO dto, CancellationToken ct)
        => await _collection.ReplaceOneAsync(x => x.Id == dto.Id, dto, cancellationToken: ct);
}
