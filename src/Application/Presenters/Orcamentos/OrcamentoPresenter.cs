using System.Net;
using Domain.Aggregates.OrcamentoAggregates;
using Shared.DTOs;
using Shared.DTOs.Orcamentos.Output;
using Shared.Result;

namespace Application.Presenters.Orcamentos;

public class OrcamentoPresenter
{
    private readonly string _message;

    public OrcamentoPresenter(string message) => _message = message;

    public ICommandResult<OrcamentoOutputDTO> TransformObject(Orcamento orcamento)
        => new CommandResult<OrcamentoOutputDTO>(_message, HttpStatusCode.OK, Transform(orcamento));

    public ICommandResult<PagedResultDTO<OrcamentoOutputDTO>> TransformPaged(List<Orcamento> items, int page, int total, int pageSize = 10)
        => new CommandResult<PagedResultDTO<OrcamentoOutputDTO>>(_message, HttpStatusCode.PartialContent,
            new PagedResultDTO<OrcamentoOutputDTO>
            {
                Items = items.Select(Transform).ToList(),
                Total = total,
                Page = page,
                PageSize = pageSize
            });

    public ICommandResult<T> Created<T>(T data)
        => new CommandResult<T>(_message, HttpStatusCode.Created, data);

    public ICommandResult NoContent()
        => new CommandResult(_message, HttpStatusCode.NoContent);

    public ICommandResult NotFound(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.NotFound);

    public ICommandResult BadRequest(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.BadRequest);

    public ICommandResult InternalError(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.InternalServerError);

    public ICommandResult Forbidden(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.Forbidden);

    private static OrcamentoOutputDTO Transform(Orcamento o) => new()
    {
        Id = o.Id,
        OrdemServicoId = o.OrdemServicoId,
        ClienteId = o.ClienteId,
        Itens = o.Itens.Select(i => new ItemOrcamentoOutputDTO
        {
            Descricao = i.Descricao,
            Quantidade = i.Quantidade,
            ValorUnitario = i.ValorUnitario,
            ValorTotal = i.ValorTotal,
            Tipo = i.Tipo
        }).ToList(),
        Status = o.Status.ToString(),
        ValorTotal = o.ValorTotal,
        DataExpiracao = o.DataExpiracao,
        MotivoRejeicao = o.MotivoRejeicao,
        DataCriacao = o.DataCriacao,
        DataAtualizacao = o.DataAtualizacao
    };
}
